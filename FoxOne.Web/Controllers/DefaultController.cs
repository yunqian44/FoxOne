using FoxOne.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FoxOne.Web.Controllers
{
    public class DefaultController : Controller
    {
        //
        // GET: /Default/

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult FileUpload()
        {
            string content = Request.ServerVariables["HTTP_CONTENT_DISPOSITION"];
            string[] kv = content.Split(';');
            string fileName = kv[2].Split('=')[1].Trim('"');
            var filePath = UploadHelper.UploadStream(Request.InputStream, "editorUploadFiles", fileName, true);
            var result = new { err = string.Empty, msg = SysConfig.DomainName + "/" + filePath };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
