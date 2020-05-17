/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/6/9 13:29:20
 * 描述说明：
 * *******************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FoxOne.Workflow.Kernel;
using FoxOne.Core;

namespace FoxOne.Workflow.Business
{
    public class WorkflowContext : IWorkflowContext
    {
        public IUser CurrentUser
        {
            get;
            set;
        }

        public IWorkflowInstance FlowInstance
        {
            get;
            set;
        }

        public IWorkflowItem CurrentTask
        {
            get;
            set;
        }

        public IList<IWorkflowChoice> UserChoice
        {
            get;
            set;
        }

        public IDictionary<string, object> Parameter
        {
            get;
            set;
        }

        public string LevelCode
        {
            get;
            set;
        }

        public string OpinionContent { get; set; }

        public int OpinionArea { get; set; }
    }
}
