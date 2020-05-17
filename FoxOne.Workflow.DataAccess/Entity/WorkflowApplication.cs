/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/6/10 15:10:42
 * 描述说明：
 * *******************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FoxOne.Workflow.Kernel;
using FoxOne.Data.Attributes;
using FoxOne.Core;
using System.ComponentModel;
using FoxOne.Business;
namespace FoxOne.Workflow.DataAccess
{
    [Category("工作流管理")]
    [DisplayName("流程应用信息")]
    [Table("WFL_Application")]
    public class WorkflowApplication :EntityBase, IWorkflowApplication,IAutoCreateTable
    {

        public string Name
        {
            get;
            set;
        }

        public string WorkflowId
        {
            get;
            set;
        }

        public string Type
        {
            get;
            set;
        }


        public string FormType
        {
            get;
            set;
        }

        public string InstanceTitleTemplate
        {
            get;
            set;
        }

        public string Icon
        {
            get;
            set;
        }

        public string DocUrl
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        /// <summary>
        /// 所属部门
        /// </summary>
        public string DepartmentType { get; set; }

        /// <summary>
        /// 拥有此流程发起的角色
        /// </summary>
        public string RoleType { get; set; }

        /// <summary>
        /// 是否需要附件
        /// </summary>
        public bool NeedAttachement { get; set; }


        public string Status { get; set; }
    }
}
