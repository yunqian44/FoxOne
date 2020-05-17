using FoxOne.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FoxOne.Business.OAuth
{
    /// <summary>
    /// 生成登陆授权
    /// </summary>
    /// <typeparam name="AuthenticationOptions"></typeparam>
    public class AuthenticationHandler
    {
        protected AuthenticationOptions _options { get; set; }
        public AuthenticationHandler(AuthenticationOptions options)
        {
            this._options = options;
        }

        /// <summary>
        /// 依据指定信息生成链接
        /// </summary>
        /// <param name="state"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        public virtual string GetAuthorizationUrl(AuthenticationScope scope)
        {
            return _options.AuthorizeUrl;
        }

        /// <summary>
        /// 生成预授权Ticket
        /// </summary>
        /// <param name="ticket"></param>
        public virtual AuthenticationTicket PreAuthorization(AuthenticationTicket ticket)
        {
            return ticket;
        }
        /// <summary>
        /// 生成Token等
        /// </summary>
        /// <param name="ticket"></param>
        /// <returns></returns>
        public virtual AuthenticationTicket AuthenticateCore(AuthenticationTicket ticket)
        {
            return ticket;
        }
        /// <summary>
        /// 获取授权用户信息
        /// </summary>
        /// <param name="ticket"></param>
        /// <returns></returns>
        public virtual IUser GetUserInfo(AuthenticationTicket ticket)
        {
            var userClaim = DBContext<UserClaim>.Instance.Where(o => o.OpenId.Equals(ticket.OpenId, StringComparison.OrdinalIgnoreCase) && o.Tag.Equals(ticket.Tag, StringComparison.OrdinalIgnoreCase));
            if (userClaim.IsNullOrEmpty())
            {
                return null;
            }
            else
            {
                var uc = userClaim.First();
                uc.UnionId = ticket.UnionId;
                uc.Token = ticket.AccessToken;
                DBContext<UserClaim>.Update(uc);
                return DBContext<IUser>.Instance.FirstOrDefault(o => o.Id.Equals(userClaim.FirstOrDefault().UserId, StringComparison.OrdinalIgnoreCase));
            }
        }
    }

    public class AuthenticationTicket
    {
        public string Code { get; set; }
        public string Tag { get; set; }
        public string OpenId { get; set; }

        public string UnionId { get; set; }

        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }

    /// <summary>
    /// 授权选项
    /// </summary>
    public class AuthenticationOptions
    {
        /// <summary>
        /// 客户端Id
        /// </summary>
        public string AppId { get; set; }
        /// <summary>
        /// 客户端密钥
        /// </summary>
        public string AppSecret { get; set; }
        /// <summary>
        /// 授权链接
        /// </summary>
        public string AuthorizeUrl { get; set; }
        /// <summary>
        /// 域名
        /// </summary>
        public string Host { get; set; }
        /// <summary>
        /// 回传
        /// </summary>
        public string Callback { get; set; }
    }

    public class AuthenticationScope
    {
        public string State { get; set; }
        public string Scope { get; set; }
    }
}
