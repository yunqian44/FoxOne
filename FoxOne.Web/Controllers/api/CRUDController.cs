using FoxOne.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using FoxOne.Core;
namespace FoxOne.Web.Controllers
{
    [Authorize]
    public class CRUDController : ApiController
    {

        public List<IDictionary<string, object>> Get(string id)
        {
            var crudService = new CRUDDataSource() { CRUDName = "api-" + id };
            int recordCount = 0;
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            int pageIndex = request.QueryString["PageIndex"].ConvertTo<int>();
            int pageSize = request.QueryString["PageSize"].ConvertTo<int>();
            if (crudService.Parameter == null)
            {
                crudService.Parameter = new FoxOneDictionary<string, object>();
            }
            foreach (var item in request.QueryString.AllKeys)
            {
                crudService.Parameter.Add(item, request.QueryString[item]);
            }
            if (pageIndex == 0 || pageSize == 0)
            {
                return crudService.GetList().ToList();
            }
            else
            {
                return crudService.GetList(pageIndex, pageSize, out recordCount).ToList();
            }
        }

        public int Post(string id)
        {
            var crudService = new CRUDDataSource() { CRUDName = id };
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            return crudService.Insert(request.Form.ToDictionary());
        }
    }
}
