/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/6/24 11:29:34
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
    [DisplayName("并行条件")]
    public class ParallelEndCondition:BaseCondition
    {

        public override bool Resolve(IWorkflowContext args)
        {
            if ((this.Owner.To is ParallelEndActivity) || (this.Owner.To is ParallelStartActivity))
            {
                bool returnValue = false;
                foreach (var choice in args.UserChoice)
                {
                    var chooseActi = Owner.Owner[choice.Choice];
                    foreach (var tran in this.Owner.To.Transitions)
                    {
                        if (chooseActi == tran.To)
                        {
                            return true;
                        }
                    }
                }
                return returnValue;
            }
            else
            {
                throw new Exception("并行条件只能用于连接并行开始或并行结束步骤！");
            }
        }

        public override bool PreResolve(IWorkflowContext args)
        {
            return true;
        }
    }
}
