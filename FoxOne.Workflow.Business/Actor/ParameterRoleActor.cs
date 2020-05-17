/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/1/26 8:21:22
 * 描述说明：
 * *******************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Xml;
using FoxOne.Workflow.Kernel;
using FoxOne.Core;

namespace FoxOne.Workflow.Business
{
    [Serializable, DisplayName("参数用户角色参与者")]
    public class ParameterRoleActor:RoleActor
    {
        [DisplayName("参数名")]
        public string ParameterName
        {
            get;
            set;
        }

        [DisplayName("参数值")]
        public string ParameterValue
        {
            get;
            set;
        }

        public override IList<IUser> Resolve(IWorkflowContext context)
        {
            if (context.Parameter.ContainsKey(ParameterName.ToString()) && !string.IsNullOrEmpty(ParameterValue))
            {
                if (string.Compare(context.Parameter[ParameterName.ToString()].ToString(), ParameterValue, true) == 0)
                {
                    return base.Resolve(context);
                }
            }
            return null;
        }
    }
}
