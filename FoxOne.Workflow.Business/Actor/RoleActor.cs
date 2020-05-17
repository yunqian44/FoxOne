/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/1/26 11:49:30
 * 描述说明：
 * *******************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FoxOne.Workflow.Kernel;
using FoxOne.Core;
using FoxOne.Workflow.DataAccess;
using System.ComponentModel;

namespace FoxOne.Workflow.Business
{
    [Serializable, DisplayName("用户角色参与者")]
    public class RoleActor : BaseActor
    {

        [DisplayName("角色名称")]
        public string RoleName
        {
            get;
            set;
        }

        [DisplayName("部门名称")]
        public string DeptName
        {
            get;
            set;
        }

        [DisplayName("并行内")]
        public bool IsParallel
        {
            get;
            set;
        }

        [DefaultValue(typeof(FlowActor)), DisplayName("基准参与者")]
        public IActor BaseActor
        {
            get;
            set;
        }

        [DisplayName("自动向上求解")]
        public bool AutoResolvParent
        {
            get;
            set;
        }

        [DisplayName("显示部门类型")]
        public ShowDepartmentType ShowDeptType
        {
            get;
            set;
        }

        public override IList<IUser> Resolve(IWorkflowContext context)
        {
            IDepartment baseDept = GetBaseDepartment(context);
            return GetMembers(baseDept);
        }

        protected virtual IDepartment GetBaseDepartment(IWorkflowContext context)
        {
            IDepartment baseDept = null;
            
            if (IsParallel)
            {
                if (context.CurrentTask == null || string.IsNullOrEmpty(context.CurrentTask.ParallelInfo))
                {
                    throw new ArgumentNullException("并行内求解需要有并行信息keyInfo");
                }
                baseDept =DBContext<IDepartment>.Instance.Get(context.CurrentTask.ParallelInfo);
            }
            if (baseDept == null && !string.IsNullOrEmpty(DeptName))
            {
                baseDept = DBContext<IDepartment>.Instance.FirstOrDefault(o => o.Name.Equals(DeptName, StringComparison.OrdinalIgnoreCase));
            }
            if (baseDept == null)
            {
                if(BaseActor==null)
                {
                    BaseActor = new FlowActor() { FlowUser = FlowActorType.Creator };
                }
                BaseActor.Owner = Owner;
                var users = BaseActor.Resolve(context);
                if (users.Count > 0)
                {
                    baseDept = users.First().Department;
                }
            }
            if (baseDept == null)
            {
                throw new Exception("基准部门求解失败!");
            }
            return baseDept;
        }

        protected virtual IList<IUser> GetMembers(IDepartment department)
        {
            if (string.IsNullOrEmpty(RoleName) || RoleName ==SysConfig.DefaultUserRole)
            {
                return department.Member.ToList();
            }
            else
            {
                IList<IUser> result = new List<IUser>();
                try
                {
                    IRole role = department.Roles.FirstOrDefault(r => r.RoleType.Name == RoleName);
                    if (role != null)
                        result = role.Members.ToList();
                    if (ShowDeptType == ShowDepartmentType.RoleDepartment)
                    {
                        foreach (var u in result)
                        {
                            u.DepartmentId = department.Id;
                        }
                    }
                }
                catch (InvalidOperationException invalidEx)
                {
                    throw new InvalidOperationException(String.Format("获取角色{0}的成员时出错：{1}。", this.RoleName, invalidEx.Message), invalidEx);
                }
                catch (Exception ex)
                {
                    throw new Exception(String.Format("获取角色{0}的成员时出错:{1}。", this.RoleName, ex.Message), ex);
                }
                if (result.Count == 0 && department.Parent != null && AutoResolvParent)
                {
                    return GetMembers(department.Parent);
                }
                return result;
            }
        }
    }

    /// <summary>
    /// 显示部门类型
    /// </summary>
    public enum ShowDepartmentType
    {
        /// <summary>
        /// 人员所属部门
        /// </summary>
        [Description("人员所属部门")]
        UserDepartment = 0,

        /// <summary>
        /// 角色所属部门
        /// </summary>
        [Description("角色所属部门")]
        RoleDepartment = 1
    }
}
