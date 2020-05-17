/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/6/10 15:03:47
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
    [DisplayName("流程扩展属性")]
    [Table("WFL_Parameter")]
    public class WorkflowParameter:EntityBase, IWorkflowParameter,IAutoCreateTable
    {
        public string InstanceId
        {
            get;
            set;
        }

        public string Key
        {
            get;
            set;
        }

        public string Value
        {
            get;
            set;
        }
    }
}
