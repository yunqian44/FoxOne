using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using FoxOne.Business.Environment;
using FoxOne.Business.Security;
using FoxOne.Data;
using FoxOne.Core;
namespace FoxOne.Web.Controllers
{
    public class PermissionController : BaseController
    {
        public ActionResult List()
        {
            return View();
        }

        public JsonResult Get()
        {
            if (Sec.IsSuperAdmin)
            {
                var permissions = DBContext<IPermission>.Instance.Where(o => true).OrderBy(o => o.Type);
                return Json(permissions, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var returnValue = new List<IPermission>();
                var permissions = Sec.Provider.GetAllUserPermission();
                foreach (var d in permissions)
                {
                    switch (d.Type)
                    {
                        case PermissionType.Module:
                            returnValue.Add(d);
                            break;
                        case PermissionType.Page:
                            returnValue.Add(d);
                            if (!returnValue.Contains(d.Parent))
                            {
                                returnValue.Add(d.Parent);
                            }
                            break;
                        case PermissionType.Control:
                            returnValue.Add(d);
                            break;
                        case PermissionType.Rule:
                            returnValue.Add(d);
                            var brothers = d.Parent.Childrens.Where(o => o.Rank > d.Rank);
                            if (!brothers.IsNullOrEmpty())
                            {
                                returnValue.AddRange(brothers);
                            }
                            break;
                        default:
                            break;
                    }
                }
                returnValue = returnValue.OrderBy(o => o.Type).OrderBy(o => o.Rank).ToList();
                return Json(returnValue, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult RoleTypePermission()
        {
            string roleId = Request["RoleId"];
            string permissionId = Request["Id"];
            string action = Request["action"];
            string ruleId = Request["RuleId"];
            switch (action)
            {
                case "Get":
                    var roleType = DBContext<IRoleType>.Instance.FirstOrDefault(o => o.Id.Equals(roleId, StringComparison.OrdinalIgnoreCase));
                    if (roleType != null)
                    {
                        return Json(roleType.Permissions);
                    }
                    else
                    {
                        var role = DBContext<IRole>.Instance.FirstOrDefault(o => o.Id.Equals(roleId, StringComparison.OrdinalIgnoreCase));
                        if (role != null)
                        {
                            return Json(role.RoleType.Permissions);
                        }
                    }
                    break;
                case "Add":

                    string[] ids = permissionId.Split(',');
                    int effectCount = 0;
                    foreach (var id in ids)
                    {
                        var item = ObjectHelper.GetObject<IRoleTypePermission>();
                        item.Id = Guid.NewGuid().ToString();
                        item.PermissionId = id;
                        item.RoleTypeId = roleId;
                        effectCount += (DBContext<IRoleTypePermission>.Insert(item) ? 1 : 0);
                    }
                    Sec.Provider.Abandon();
                    return Json(ids.Length == effectCount);
                case "Remove":
                    var items = DBContext<IRoleTypePermission>.Instance.Where(o => o.RoleTypeId.Equals(roleId, StringComparison.OrdinalIgnoreCase) && permissionId.Split(',').Contains(o.PermissionId, StringComparer.OrdinalIgnoreCase));
                    int count = 0;
                    foreach (var i in items)
                    {
                        if (DBContext<IRoleTypePermission>.Delete(i))
                        {
                            count++;
                        }
                    }
                    Sec.Provider.Abandon();
                    return Json(count == items.Count());
                case "Update":
                    var permission = DBContext<IPermission>.Instance.FirstOrDefault(o => o.Id.Equals(permissionId, StringComparison.OrdinalIgnoreCase)).Childrens;
                    if (!permission.IsNullOrEmpty())
                    {
                        string[] permissionIds = permission.Select(o => o.Id).ToArray();
                        var items1 = DBContext<IRoleTypePermission>.Instance.Where(o => o.RoleTypeId.Equals(roleId, StringComparison.OrdinalIgnoreCase) && permissionIds.Contains(o.PermissionId, StringComparer.OrdinalIgnoreCase));
                        foreach (var i in items1)
                        {
                            DBContext<IRoleTypePermission>.Delete(i);
                        }
                    }
                    var item2 = ObjectHelper.GetObject<IRoleTypePermission>();
                    item2.Id = Guid.NewGuid().ToString();
                    item2.PermissionId = ruleId;
                    item2.RoleTypeId = roleId;
                    DBContext<IRoleTypePermission>.Insert(item2);
                    Sec.Provider.Abandon();
                    break;
            }
            return Json(false);
        }

        public JsonResult RolePermission()
        {
            string roleId = Request["RoleId"];
            string permissionId = Request["Id"];
            string action = Request["action"];
            string ruleId = Request["RuleId"];
            bool result = false;
            switch (action)
            {
                case "Get":
                    var role = DBContext<IRole>.Instance.FirstOrDefault(o => o.Id.Equals(roleId, StringComparison.OrdinalIgnoreCase));
                    if (role != null)
                    {
                        return Json(role.Permissions);
                    }
                    break;
                case "Add":
                    string[] ids = permissionId.Split(',');
                    int effectCount = 0;
                    foreach (var id in ids)
                    {
                        var item = ObjectHelper.GetObject<IRolePermission>();
                        item.Id = Guid.NewGuid().ToString();
                        item.PermissionId = id;
                        item.RoleId = roleId;
                        effectCount += (DBContext<IRolePermission>.Insert(item) ? 1 : 0);
                    }
                    Sec.Provider.Abandon();
                    return Json(ids.Length == effectCount);
                case "Remove":
                    var items = DBContext<IRolePermission>.Instance.Where(o => o.RoleId.Equals(roleId, StringComparison.OrdinalIgnoreCase) && permissionId.Split(',').Contains(o.PermissionId, StringComparer.OrdinalIgnoreCase));
                    int count = 0;
                    foreach (var i in items) 
                    {
                        if (DBContext<IRolePermission>.Delete(i))
                        {
                            count++;
                        }
                    }
                    result = (count == items.Count());
                    Sec.Provider.Abandon();
                    break;
                case "Update":
                    var permission = DBContext<IPermission>.Instance.FirstOrDefault(o => o.Id.Equals(permissionId, StringComparison.OrdinalIgnoreCase)).Childrens;
                    if (!permission.IsNullOrEmpty())
                    {
                        string[] permissionIds = permission.Select(o => o.Id).ToArray();
                        var items1 = DBContext<IRolePermission>.Instance.Where(o => o.RoleId.Equals(roleId, StringComparison.OrdinalIgnoreCase) && permissionIds.Contains(o.PermissionId, StringComparer.OrdinalIgnoreCase));
                        foreach (var i in items1)
                        {
                            DBContext<IRolePermission>.Delete(i);
                        }
                    }
                    var item2 = ObjectHelper.GetObject<IRolePermission>();
                    item2.Id = Guid.NewGuid().ToString();
                    item2.PermissionId = ruleId;
                    item2.RoleId = roleId;
                    DBContext<IRolePermission>.Insert(item2);
                    Sec.Provider.Abandon();
                    break;
            }
            return Json(result);
        }
    }
}
