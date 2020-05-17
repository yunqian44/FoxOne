/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/6/26 10:40:36
 * 描述说明：
 * *******************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FoxOne.Workflow.Kernel;
using System.Threading;
using FoxOne.Core;
using System.ComponentModel;

namespace FoxOne.Workflow.Business
{
    [DisplayName("部门条件")]
    public class DepartmentCondition:BaseCondition
    {
        [DisplayName("部门名称")]
        public string DeptName { get; set; }

        [DisplayName("类型")]
        public DepartmentConditionType DepartmentType
        {
            get;
            set;
        }

        public override bool Resolve(IWorkflowContext context)
        {
            if (string.IsNullOrEmpty(DeptName))
            {
                throw new ArgumentNullException("部门条件必须设置部门名称");
            }
            var result = false;
            string currentDepartment = string.Empty;
            if (DepartmentType == DepartmentConditionType.CreatorDepartment)
            {
                var user = context.CurrentUser as IUser;
                if (user != null)
                {
                    currentDepartment = user.Department.Name;
                }
            }
            else
            {
                var instance = context.FlowInstance;
                currentDepartment = instance.Creator.Department.Name;
            }
            if (DeptName.IndexOf(',') > 0)
            {
                string[] depts = DeptName.Split(',');
                if(depts.Contains(currentDepartment,StringComparer.Create(Thread.CurrentThread.CurrentCulture,true)))
                {
                    result = true;
                }
            }
            else
            {
                if (currentDepartment.Equals(DeptName, StringComparison.CurrentCultureIgnoreCase))
                {
                    result = true;
                }
            }
            return result;
        }
    }

    public enum DepartmentConditionType
    {
        /// <summary>
        /// 当前部门
        /// </summary>
        CurrentDepartment = 0,

        /// <summary>
        /// 发起部门
        /// </summary>
        CreatorDepartment = 1
    }
}
