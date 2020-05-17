/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/1/26 11:50:37
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
    [Serializable]
    [DisplayName("步骤条件")]
    public class ActivityCondition : BaseCondition
    {
        [DisplayName("步骤名")]
        public string ActivityName
        {
            get;
            set;
        }

        [DisplayName("是否已完成")]
        public bool IsFinished { get; set; }

        public override bool Resolve(IWorkflowContext context)
        {
            string instanceId = context.FlowInstance.Id;
            var workItems = context.FlowInstance.WorkItems;
            if (workItems.Count > 0)
            {
                foreach (var wi in workItems)
                {
                    if (wi.CurrentActivity.Equals(ActivityName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
