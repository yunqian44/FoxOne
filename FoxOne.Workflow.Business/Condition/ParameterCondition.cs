/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/1/26 11:50:49
 * 描述说明：
 * *******************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FoxOne.Workflow.Kernel;
using System.Threading;
using System.ComponentModel;
using FoxOne.Business;
using FoxOne.Business.Environment;
using FoxOne.Core;
namespace FoxOne.Workflow.Business
{
    [Serializable]
    [DisplayName("参数条件")]
    public class ParameterCondition : BaseCondition
    {
        [DisplayName("参数名")]
        public string ParameterName
        {
            get;
            set;
        }

        [DisplayName("运算符")]
        [FormField(ControlType = ControlType.DropDownList)]
        [TypeDataSource(typeof(ColumnOperator))]
        public string Operator { get; set; }

        [DisplayName("参数值")]
        [Description("值可以是环境变量表达式")]
        public string ParameterValue
        {
            get;
            set;
        }

        public override bool Resolve(IWorkflowContext context)
        {
            if (string.IsNullOrEmpty(ParameterName))
            {
                throw new ArgumentNullException("ParameterName"); 
            }
            bool result = false;
            object obj1 = null;
            if (ParameterName.Equals("$User.RoleName$"))
            {
                obj1 = string.Join("|", context.FlowInstance.Creator.Roles.Select(o => o.RoleType.Name));
            }
            else
            {
                if (!Env.TryResolve(ParameterName, out obj1))
                {
                    if (context.Parameter != null && context.Parameter.Count > 0 && context.Parameter.Keys.Contains(ParameterName, StringComparer.Create(Thread.CurrentThread.CurrentCulture, true)))
                    {
                        obj1 = context.Parameter[ParameterName];
                    }
                }
            }
            var op = AllColumnOperator.OperatorMapping[Operator];
            object obj2 = ParameterValue;
            if (ParameterValue.IsNotNullOrEmpty() && Env.TryResolve(ParameterValue, out obj2))
            {
                result = op.Operate(obj1, obj2);
            }
            else
            {
                result = op.Operate(obj1, ParameterValue);
            }
            Log("{6}:参数名【{0}】解析为：{1},参数值【{2}】解析为：{3}，运算符：{4}，结果:{5}".FormatTo(ParameterName, obj1, ParameterValue, obj2, op.GetType().GetDisplayName(), result,context.FlowInstance.Id));
            return result;
        }
    }
}
