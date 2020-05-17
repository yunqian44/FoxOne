/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/6/10 15:12:02
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
    [DisplayName("流程定义信息")]
    [Table("WFL_Definition")]
    public class WorkflowDefinition :EntityBase, IWorkflowDefinition,IAutoCreateTable
    {

        public string Name
        {
            get;
            set;
        }

        public string Definition
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }


        public string Status
        {
            get;
            set;
        }

        public int Width { get; set; }

        public int Height { get; set; }
    }
}
