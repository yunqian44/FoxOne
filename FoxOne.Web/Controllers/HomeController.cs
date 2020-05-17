using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using FoxOne.Data;
using FoxOne.Controls;
using FoxOne.Business;
using FoxOne.Core;
using FoxOne.Business.Security;
using FoxOne.Business.OAuth;
using FoxOne.Business.Environment;

namespace FoxOne.Web.Controllers
{
    public class HomeController : BaseController
    {

        private const string NextSendTimeKey = "NextSendTime";
        private const string OAuthUserKey = "OAuthUser";
        private const string PhoneKey = "phone";
        private const string RequestState = "RequestState";
        private const string FailTimes = "FailTimes";
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetMenu()
        {
            var temp = Sec.Provider.GetAllUserPermission().Where(o => o.Type < PermissionType.Control && o.Status.Equals(DefaultStatus.Enabled.ToString(), StringComparison.OrdinalIgnoreCase)).OrderBy(o => o.Rank);
            var result = new List<TreeNode>();
            temp.ForEach(o =>
            {
                result.Add(new TreeNode
                {
                    Value = o.Id,
                    ParentId = o.ParentId,
                    Text = o.Name,
                    Url = Env.Parse(o.Url),
                    Icon = o.Icon
                });
                if (o.Parent != null)
                {
                    if (result.Count(p => p.Value.Equals(o.Parent.Id, StringComparison.OrdinalIgnoreCase)) == 0)
                    {
                        result.Add(new TreeNode
                        {
                            Value = o.Parent.Id,
                            ParentId = string.Empty,
                            Text = o.Parent.Name,
                            Url = o.Parent.Url,
                            Icon = o.Parent.Icon
                        });
                    }
                }
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDataBySqlId(string sqlId, string type)
        {
            if (sqlId.StartsWith("crud", StringComparison.OrdinalIgnoreCase))
            {
                string[] temp = sqlId.Split('.');
                var entity = DBContext<CRUDEntity>.Instance.Get(temp[1]);
                switch (temp[2].ToUpper())
                {
                    case "INSERT":
                        sqlId = entity.InsertSQL;
                        break;
                    case "UPDATE":
                        sqlId = entity.UpdateSQL;
                        break;
                    case "DELETE":
                        sqlId = entity.DeleteSQL;
                        break;
                    case "SELECT":
                        sqlId = entity.SelectSQL;
                        break;
                    case "GET":
                        sqlId = entity.GetOneSQL;
                        break;
                }
            }
            else
            {
                if (DaoFactory.GetSqlSource().Find(sqlId) == null)
                {
                    throw new FoxOneException("SqlId_Not_Found", sqlId);
                }
            }
            if (type.Equals("exec:", StringComparison.OrdinalIgnoreCase))
            {
                return Json(Dao.Get().ExecuteNonQuery(sqlId) > 0, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(Dao.Get().QueryDictionaries(sqlId), JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetWidgetData()
        {
            string pageId = Request[NamingCenter.PARAM_PAGE_ID];
            string ctrlId = Request[NamingCenter.PARAM_CTRL_ID];
            string widgetType = Request[NamingCenter.PARAM_WIDGET_TYPE];
            if (ctrlId.IsNullOrEmpty())
            {
                throw new FoxOneException("Parameter_Not_Found", NamingCenter.PARAM_CTRL_ID);
            }
            var ctrl = PageBuilder.BuildPage(pageId).FindControl(ctrlId);
            if (ctrl == null)
            {
                throw new FoxOneException("Ctrl_Not_Found", ctrlId);
            }
            if (widgetType.Equals("Chart", StringComparison.OrdinalIgnoreCase))
            {
                var chart = ctrl as NoPagerListControlBase;
                if (chart == null)
                {
                    throw new FoxOneException("Need_To_Be_NoPagerListControlBase", chart.Id);
                }
                if (chart.DataSource == null)
                {
                    throw new FoxOneException("Need_DataSource", chart.Id);
                }
                return Json(chart.GetData(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                var tree = ctrl as Tree;
                if (tree == null)
                {
                    throw new FoxOneException("Need_To_Be_Tree", tree.Id);
                }
                if (tree.DataSource == null)
                {
                    throw new FoxOneException("Need_DataSource", tree.Id);
                }
                return Json(tree.DataSource.SelectItems(), JsonRequestBehavior.AllowGet);

            }
        }

        #region OAuth验证
        [CustomUnAuthorize]
        public ActionResult QQLogOn()
        {
            AuthenticationScope scope = new AuthenticationScope()
            {
                State = Guid.NewGuid().ToString().Replace("-", ""),
                Scope = "get_user_info"
            };
            Session[RequestState] = scope.State;
            string url = GetAuthHandler("QQ").GetAuthorizationUrl(scope);
            return Redirect(url);
        }

        [CustomUnAuthorize]
        public ActionResult DDLogOn()
        {
            AuthenticationScope scope = new AuthenticationScope()
            {
                State = Guid.NewGuid().ToString().Replace("-", ""),
                Scope = "snsapi_login"
            };
            Session[RequestState] = scope.State;
            string url = GetAuthHandler("DD").GetAuthorizationUrl(scope);
            return Redirect(url);
        }

        [ValidateInput(false)]
        [CustomUnAuthorize]
        public ActionResult QQLogOnCallback()
        {
            return LogOnCallbackInner("QQ");
        }

        [ValidateInput(false)]
        [CustomUnAuthorize]
        public ActionResult DDLogOnCallback()
        {
            return LogOnCallbackInner("DD");
        }

        private ActionResult LogOnCallbackInner(string tag)
        {
            var verifier = Request.Params["code"];
            var verifierState = Request.Params["state"];
            string state = Session[RequestState] == null ? string.Empty : Session[RequestState].ToString();
            if (verifierState.IsNotNullOrEmpty() && verifierState.Equals(state, StringComparison.OrdinalIgnoreCase))
            {
                AuthenticationTicket ticket = new AuthenticationTicket()
                {
                    Code = verifier,
                    Tag = tag
                };
                var tencentHandler = GetAuthHandler(tag);
                ticket = tencentHandler.PreAuthorization(ticket);
                ticket = tencentHandler.AuthenticateCore(ticket);
                var user = tencentHandler.GetUserInfo(ticket);
                if (user != null)
                {
                    Log(user, tag);
                    FormsAuthentication.SetAuthCookie(user.LoginId, true);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    Session[OAuthUserKey] = ticket;
                    return RedirectToAction("UserBind", "Home");
                }
            }
            return RedirectToAction("LogOn");
        }

        private AuthenticationHandler GetAuthHandler(string tag)
        {
            if (tag.Equals("QQ", StringComparison.OrdinalIgnoreCase))
            {
                var _options = new AuthenticationOptions()
                {
                    AppId = "qq-appid",
                    AppSecret = "qq-appsecret",
                    AuthorizeUrl = "https://graph.qq.com",
                    Host = "http://www.yousite.com",
                    Callback = "/Home/QQLogOnCallback",
                };
                return new QQAuthenticationHandler(_options);
            }
            else
            {
                var _options = new AuthenticationOptions()
                {
                    AppId = "dingding-appid",
                    AppSecret = "dingding-appsecret",
                    AuthorizeUrl = "https://oapi.dingtalk.com",
                    Host = "http://www.yousite.com",
                    Callback = "/Home/DDLogOnCallback"
                };
                return new DingDingAuthenticationHandler(_options);
            }
        }

        [CustomUnAuthorize]
        public ActionResult UserBind()
        {
            var ticket = Session[OAuthUserKey] as AuthenticationTicket;
            if (ticket == null)
            {
                return RedirectToAction("LogOn");
            }
            return View();
        }

        [CustomUnAuthorize]
        [HttpPost]
        public ActionResult UserBind(FormCollection form)
        {
            var ticket = Session[OAuthUserKey] as AuthenticationTicket;
            if (ticket == null)
            {
                return RedirectToAction("LogOn");
            }
            string errorMessage = string.Empty;
            string mobilePhone = Request.Form["MobilePhone"];
            if (mobilePhone.IsNullOrEmpty())
            {
                errorMessage = "必须输入手机号";
            }
            else
            {
                var users = DBContext<IUser>.Instance.Where(o => o.MobilePhone.IsNotNullOrEmpty() && o.MobilePhone.Equals(mobilePhone, StringComparison.OrdinalIgnoreCase));
                if (users.Count() == 0)
                {
                    errorMessage = "未找到对应的手机号";
                }
                else
                {
                    var user = users.FirstOrDefault();
                    var userClaim = new UserClaim()
                    {
                        Id = Guid.NewGuid().ToString(),
                        LoginId = user.LoginId,
                        UserId = user.Id,
                        OpenId = ticket.OpenId,
                        Tag = ticket.Tag,
                        RentId = 1,
                        UnionId = ticket.UnionId,
                        Token = ticket.AccessToken
                    };
                    Session.Remove(OAuthUserKey);
                    DBContext<UserClaim>.Insert(userClaim);
                    FormsAuthentication.SetAuthCookie(user.LoginId, true);
                    return RedirectToAction("Index", "Home");
                }
            }
            ViewData["ErrorMessage"] = errorMessage;
            return View();
        }

        #endregion


        [CustomUnAuthorize]
        public ActionResult LogOn()
        {
            FormsAuthentication.SignOut();
            Sec.Provider.Abandon();
            return View();
        }


        private void Log(IUser user, string logType)
        {
            Logger.GetLogger("SystemUse").InfoFormat("{0}:【{1}】登录，IP：{2}，登录方式：{3}", user.Id, user.Name, Utility.GetWebClientIp(), logType);
        }

        [CustomUnAuthorize]
        [HttpPost]
        public ActionResult LogOn(string userName, string password)
        {
            if (Sec.Provider.Authenticate(userName, password))
            {
                FormsAuthentication.SetAuthCookie(userName, false);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewData["ErrorMessage"] = ObjectHelper.GetObject<ILangProvider>().GetString("InValid_User_Or_Password");
                return View();
            }
        }



        [CustomUnAuthorize]
        public ActionResult Error(string id)
        {
            ViewData["ErrorMessage"] = id;
            return View();
        }

        public ActionResult LogOut()
        {
            Logger.GetLogger("SystemUse").InfoFormat("{0}:【{1}】注销，IP：{2}", Sec.User.Id, Sec.User.Name, Utility.GetWebClientIp());
            Sec.Provider.Abandon();
            FormsAuthentication.SignOut();
            return RedirectToAction("LogOn");
        }

        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public JsonResult ChangePassword(FormCollection form)
        {
            string password = form["OldPassword"];
            string newPassword = form["NewPassword"];
            string confirmPassword = form["ConfirmPassword"];

            if (!newPassword.Equals(confirmPassword, StringComparison.OrdinalIgnoreCase))
            {
                throw new FoxOneException("NewPassword_NotEqual_ConfirmPassword");
            }
            if (Sec.Provider.Authenticate(Sec.User.LoginId, password))
            {
                if (Sec.Provider.ResetPassword(Sec.User.LoginId, newPassword))
                {
                    Logger.GetLogger("SystemUse").InfoFormat("{0}:【{1}】重置密码，IP：{2}，", Sec.User.Id, Sec.User.Name, Utility.GetWebClientIp());
                    return Json(true);
                }
            }
            else
            {
                throw new FoxOneException("Invalid_Original_Password");
            }
            return Json(false);
        }
    }
}
