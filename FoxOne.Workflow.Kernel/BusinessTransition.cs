using FoxOne.Business;
/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/1/26 11:51:08
 * 描述说明：
 * *******************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using FoxOne.Core;

namespace FoxOne.Workflow.Kernel
{
    [DisplayName("迁移")]
    public class BusinessTransition : ControlBase, ITransition
    {
        [Browsable(false)]
        public IWorkflow Owner
        {
            get;
            set;
        }

        [DisplayName("迁移条件")]
        public ICondition Condition
        {
            get;
            set;
        }

        [DisplayName("标签")]
        public string Label
        {
            get;
            set;
        }

        [DisplayName("描述")]
        public string Description
        {
            get;
            set;
        }

        [Browsable(false)]
        public IActivity To
        {
            get;
            set;
        }

        [Browsable(false)]
        public IActivity From
        {
            get;
            set;
        }

        [DisplayName("目标节点")]
        public string ToId { get; set; }

        [DisplayName("源节点")]
        public string FromId { get; set; }

        [DisplayName("排序")]
        public int Rank
        {
            get;
            set;
        }

        public bool Resolve(IWorkflowContext context)
        {
            var result = true;
            if (Condition != null)
            {
                result = Condition.Resolve(context);
                if (NotOperation)
                {
                    result = !result;
                }
                Logger.GetLogger("Workflow").Info("{0}:迁移：{1}，条件类型：{2}，是否取反：{3}，条件执行结果：{4}".FormatTo(context.FlowInstance.Id, Id, Condition.GetType().GetDisplayName(), NotOperation, result));
            }
            return result;
        }


        public bool PreResolve(IWorkflowContext context)
        {
            var result = true;
            if (Condition != null)
            {
                result = Condition.PreResolve(context);
                if (NotOperation)
                {
                    result = !result;
                }
                Logger.GetLogger("Workflow").Info("{0}:预求解-迁移：{1}，条件类型：{2}，是否取反：{3}，条件执行结果：{4}".FormatTo(context.FlowInstance.Id, Id, Condition.GetType().GetDisplayName(), NotOperation, result));
            }
            return result;
        }

        [DisplayName("能否退回")]
        public bool CanPushBack
        {
            get;
            set;
        }

        [DisplayName("求反")]
        public bool NotOperation
        {
            get;
            set;
        }
    }
}
