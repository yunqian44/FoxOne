/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/6/10 11:50:09
 * 描述说明：
 * *******************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FoxOne.Workflow.Kernel;
using System.ComponentModel;

namespace FoxOne.Workflow.Business
{
    [Serializable]
    [DisplayName("复合条件")]
    public class CompositeCondition : BaseCondition
    {
        [DisplayName("内部条件")]
        public IList<ICondition> InnerConditions
        {
            get;
            set;
        }

        [DisplayName("运算类型")]
        public OperateType OperateType { get; set; }

        public override bool Resolve(IWorkflowContext context)
        {
            bool tempResult = false;
            if (OperateType == OperateType.AND)
            {
                foreach (var c in InnerConditions)
                {
                    c.Owner = this.Owner;
                    tempResult = c.Resolve(context);
                    if (!tempResult)
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                foreach (var c in InnerConditions)
                {
                    c.Owner = this.Owner;
                    if (c.Resolve(context))
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }

    public enum OperateType
    {
        AND,
        OR
    }
}
