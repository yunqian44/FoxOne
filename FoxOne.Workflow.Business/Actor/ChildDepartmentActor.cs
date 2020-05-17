/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/1/26 8:21:22
 * 描述说明：
 * *******************************************************/
using System;
using System.Collections.Generic;
using FoxOne.Workflow.Kernel;
using System.Linq;
using System.ComponentModel;
using FoxOne.Core;
namespace FoxOne.Workflow.Business
{
    [Serializable, DisplayName("子级部门参与者")]
    public class ChildDepartmentActor : ParentDepartmentActor
    {
        [DisplayName("包含基准部门")]
        public bool IfReturnSelf
        {
            get;
            set;
        }

        [DisplayName("递归获取子部门")]
        public bool IfGetAllChildren
        {
            get;
            set;
        }

        public override IList<IUser> Resolve(IWorkflowContext context)
        {
            IDepartment department = GetBaseDepartment(context);
            List<IDepartment> departments = new List<IDepartment>();
            if (IfGetAllChildren)
            {
                RecGetChildren(department, departments);
            }
            else
            {
                departments.AddRange(department.Childrens);
            }
            if (IfReturnSelf)
            {
                departments.Add(department);
            }
            var result = new List<IUser>();
            foreach (var d in departments)
            {
                foreach (IUser u in GetMembers(d))
                {
                    if (result.Count(o => o.Name == u.Name) == 0)
                    {
                        result.Add(u);
                    }
                }
            }
            return result;
        }

        private static void RecGetChildren(IDepartment department, IList<IDepartment> departments)
        {
            foreach (var c in department.Childrens)
            {
                departments.Add(c);
                if (c.Childrens.Count() > 0)
                {
                    RecGetChildren(c, departments);
                }
            }
        }

    }
}