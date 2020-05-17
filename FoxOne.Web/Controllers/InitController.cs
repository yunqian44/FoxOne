using FoxOne.Business;
using FoxOne.Business.Security;
using FoxOne.Controls;
using FoxOne.Core;
using FoxOne.Data;
using FoxOne.Data.Mapping;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Xml.Serialization;
namespace FoxOne.Web.Controllers
{
    public class InitController : BaseController
    {
        IList<Type> types = TypeHelper.GetAllImpl<IAutoCreateTable>();

        [CustomUnAuthorize]
        public ActionResult Index()
        {
            if (SysConfig.SystemStatus.Equals("Run", StringComparison.OrdinalIgnoreCase))
            {
                throw new PageNotFoundException();
            }
            var allEntity = new List<SelectListItem>();
            foreach (Type item in TypeHelper.GetAllSubType<EntityBase>())
            {
                allEntity.Add(new SelectListItem()
                {
                    Selected = false,
                    Text = item.GetDisplayName(),
                    Value = item.FullName
                });
            }
            ViewData["AllEntity"] = allEntity;
            return View();
        }

        [CustomUnAuthorize]
        public ActionResult HomeIndex()
        {
            if (SysConfig.SystemStatus.Equals("Run", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("不允许此操作");
            }
            Sec.Provider.ResetPassword("liuhf", "123");
            FormsAuthentication.SetAuthCookie("liuhf", false);
            return RedirectToAction("Index", "Home");
        }

        [CustomUnAuthorize]
        [HttpPost]
        public JsonResult CreateTable(string id)
        {
            if (SysConfig.SystemStatus.Equals("Run", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("不允许此操作");
            }
            if (id.IsNullOrEmpty())
            {
                types.ForEach(o =>
                {
                    Dao.Get().CreateTable(o, true);
                });
            }
            else
            {
                Dao.Get().CreateTable(TypeHelper.GetType(id));
            }
            TableMapper.RefreshTableCache();
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        
        [CustomUnAuthorize]
        [HttpPost]
        public JsonResult ClearTable()
        {
            if (SysConfig.SystemStatus.Equals("Run", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("不允许此操作");
            }
            types.ForEach(o =>
            {
                Dao.Get().Delete(o, null);
            });
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [CustomUnAuthorize]
        [HttpPost]
        public JsonResult InitData()
        {
            if (SysConfig.SystemStatus.Equals("Run", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("不允许此操作");
            }
            var dirInfo = new DirectoryInfo(Server.MapPath("~/InitData"));
            var files = dirInfo.GetFiles("*.xml", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                var type = typeof(List<>);
                var targetType = TypeHelper.GetType(file.Name.Replace(file.Extension, ""));
                if (targetType == null) continue;
                type = type.MakeGenericType(targetType);
                var serializer = new XmlSerializer(type);
                var result = serializer.Deserialize(file.OpenRead()) as IEnumerable;
                foreach (var item in result)
                {
                    Dao.Get().Insert(item);
                }
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        
        public JsonResult Out()
        {
            var dirInfo = Server.MapPath("~");
            if (!dirInfo.EndsWith("\\"))
            {
                dirInfo += "\\";
            }
            dirInfo = "{0}InitData\\".FormatTo(dirInfo);
            if (!Directory.Exists(dirInfo))
            {
                Directory.CreateDirectory(dirInfo);
            }
            var allTypes = TypeHelper.GetAllSubType<EntityBase>();
            string fileName = string.Empty;
            foreach (var type in allTypes)
            {
                var t = typeof(List<>);
                fileName = type.FullName;
                t = t.MakeGenericType(type);
                var instance = Activator.CreateInstance(t);
                var serializer = new XmlSerializer(t);
                Dao.Get().Select(type).ForEach(o =>
                {
                    t.InvokeMember("Add", System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.InvokeMethod, null, instance, new object[] { o });
                });
                var stream = System.IO.File.Create(dirInfo + fileName + ".xml");
                serializer.Serialize(stream, instance);
                stream.Close();
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }
    }
}
