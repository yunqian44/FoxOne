/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/6/10 15:02:15
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
    public class WorkflowChoice:IWorkflowChoice
    {
        public string Choice
        {
            get;
            set;
        }

        public IList<IUser> Participant
        {
            get;
            set;
        }
    }
}
