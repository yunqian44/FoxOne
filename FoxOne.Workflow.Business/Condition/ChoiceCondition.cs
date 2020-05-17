/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/1/26 11:50:28
 * 描述说明：
 * *******************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FoxOne.Workflow.Kernel;
using System.ComponentModel;
using FoxOne.Core;
namespace FoxOne.Workflow.Business
{
    [Serializable]
    [DisplayName("选择条件")]
    public class ChoiceCondition : BaseCondition
    {
        [DisplayName("选择结果")]
        public string Choice
        {
            get;
            set;
        }

        public override bool PreResolve(IWorkflowContext context)
        {
            return true;
        }

        public override bool Resolve(IWorkflowContext context)
        {
            bool result = false;
            if (string.IsNullOrEmpty(Choice))
            {
                result = true;
            }
            else
            {
                if (context.UserChoice != null && context.UserChoice.Count > 0)
                {
                    if (Choice.IndexOf(',') > 0)
                    {
                        var choices = Choice.Split(',');
                        foreach (var ch in choices)
                        {
                            foreach (var uc in context.UserChoice)
                            {
                                if (uc.Choice.Equals(ch, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    result = true;
                                    break;
                                }
                            }
                            if (result)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        foreach (var uc in context.UserChoice)
                        {
                            if (uc.Choice.Equals(Choice, StringComparison.CurrentCultureIgnoreCase))
                            {
                                result = true;
                                break;
                            }
                        }
                    }
                }
            }
            Log("{0}:用户选择：{1}，期待选择结果：{2}，结果：{3}".FormatTo(context.FlowInstance.Id, string.Join(",", context.UserChoice.Select(o => o.Choice)), Choice, result));
            return result;
        }
    }
}
