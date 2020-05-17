using FoxOne.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using FoxOne.Core;
using FoxOne.Business.Security;

namespace FoxOne.Web.Controllers
{
    /// <summary>
    /// 组织架构服务接口
    /// </summary>
    [Authorize]
    public class UCController : ApiController
    {
        /// <summary>
        /// 获取所有用户
        /// </summary>
        /// <returns></returns>
        public IEnumerable<UCUser> GetAllUser()
        {
            return DBContext<IUser>.Instance.Where(o => true).Select(o => new UCUser() { UserId = o.Code, Name = o.Name, Mobile = o.MobilePhone, Role = o.Roles.Select(r => new UCRole() { RoleId = r.Id, Name = r.RoleType.Name }), DepartmentId = o.Department.Code.ConvertTo<int>() });
        }

        /// <summary>
        /// 根据ID获取特定用户
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns></returns>
        public UCUser GetUser(string id)
        {
            if (id.IsNullOrEmpty())
            {
                throw new ArgumentNullException("id");
            }
            var user = DBContext<IUser>.Instance.FirstOrDefault(o => o.Code.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (user == null)
            {
                throw new FoxOneException("User_Not_Found");
            }
            return new UCUser()
            {
                UserId = user.Code,
                DepartmentId = user.Department.Code.ConvertTo<int>(),
                Mobile = user.MobilePhone,
                Name = user.Name,
                Role = user.Roles.Select(r => new UCRole() { RoleId = r.Id, Name = r.RoleType.Name })
            };
        }

        /// <summary>
        /// 根据部门ID获取该部门的所有用户（不递归）
        /// </summary>
        /// <param name="id">部门ID</param>
        /// <returns></returns>
        public IEnumerable<UCUser> GetUserByDepartmentId(int id)
        {
            var department = DBContext<IDepartment>.Instance.FirstOrDefault(o => o.Code.Equals(id.ToString(), StringComparison.OrdinalIgnoreCase));
            if (department == null)
            {
                throw new FoxOneException("Department_Not_Found");
            }
            return department.Member.Select(o => new UCUser() { UserId = o.Code, Name = o.Name, Mobile = o.MobilePhone, Role = o.Roles.Select(r => new UCRole() { RoleId = r.Id, Name = r.RoleType.Name }), DepartmentId = o.Department.Code.ConvertTo<int>() });
        }

        /// <summary>
        /// 获取所有部门
        /// </summary>
        /// <returns></returns>
        public IEnumerable<UCDepartment> GetAllDepartment()
        {
            return DBContext<IDepartment>.Instance.Where(o => o.ParentId.IsNotNullOrEmpty()).Select(o => new UCDepartment() { DepartmentId = o.Code.ConvertTo<int>(), Name = o.Name, LevelCode = o.WBS, ParentId = o.Parent.Code.ConvertTo<int>() });
        }

        /// <summary>
        /// 根据部门ID获取特定部门
        /// </summary>
        /// <param name="id">部门ID</param>
        /// <returns></returns>
        public UCDepartment GetDepartment(int id)
        {
            var o = DBContext<IDepartment>.Instance.FirstOrDefault(d => d.Code.Equals(id.ToString(), StringComparison.OrdinalIgnoreCase));
            return new UCDepartment() { DepartmentId = o.Code.ConvertTo<int>(), LevelCode = o.WBS, Name = o.Name, ParentId = o.Parent.Code.ConvertTo<int>() };
        }

        /// <summary>
        /// 根据系统编号及用户ID获取该用户在指定系统中的所有权限
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="systemId">系统编号</param>
        /// <returns></returns>
        public IEnumerable<UCPermission> GetAllUserPermission(string id, string systemId)
        {
            var user = DBContext<IUser>.Instance.FirstOrDefault(o => o.Code.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (user == null)
            {
                throw new FoxOneException("User_Not_Exist");
            }
            var systemPermission = DBContext<IPermission>.Instance.FirstOrDefault(o => o.Type == PermissionType.System && o.Code.Equals(systemId, StringComparison.OrdinalIgnoreCase));
            if (systemPermission == null)
            {
                throw new FoxOneException("System_Not_Exist");
            }
            var allUserPermission = Sec.Provider.GetAllUserPermission(user);
            var result = new List<UCPermission>();
            foreach (var module in systemPermission.Childrens)
            {
                foreach (var page in module.Childrens)
                {
                    if (page.Type == PermissionType.Page)
                    {
                        if (allUserPermission.Any(o => o.Id.Equals(page.Id, StringComparison.OrdinalIgnoreCase)))
                        {
                            var temp = new UCPermission()
                            {
                                Id = page.Code,
                                Url = page.Url,
                                Name = page.Name,
                                ControlIds = string.Empty
                            };
                            var ctrls = page.Childrens.Where(o => allUserPermission.Any(j => j.Id == o.Id));
                            if (!ctrls.IsNullOrEmpty())
                            {
                                temp.ControlIds = string.Join(",", ctrls.Select(o => o.Url).ToArray());
                            }
                            result.Add(temp);
                        }
                    }
                }
            }
            return result;
        }
    }

    /// <summary>
    /// 用户信息
    /// </summary>
    public class UCUser
    {
        public string UserId { get; set; }

        public string Name { get; set; }

        public string Mobile { get; set; }

        public int DepartmentId { get; set; }

        public IEnumerable<UCRole> Role { get; set; }

    }

    public class UCRole
    {
        public string RoleId { get; set; }

        public string Name { get; set; }
    }

    public class UCDepartment
    {
        public int DepartmentId { get; set; }

        public string Name { get; set; }

        public string LevelCode { get; set; }

        public int ParentId { get; set; }
    }

    public class UCPermission
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }

        public string ControlIds { get; set; }
    }
}
