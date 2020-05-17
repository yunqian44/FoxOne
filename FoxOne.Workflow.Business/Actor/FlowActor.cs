/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/6/9 10:32:58
 * 描述说明：
 * *******************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FoxOne.Workflow.Kernel;
using FoxOne.Workflow.DataAccess;
using FoxOne.Core;
using System.ComponentModel;

namespace FoxOne.Workflow.Business
{
    [Serializable, DisplayName("流程实例参与者")]
    public class FlowActor:BaseActor
    {
        [DisplayName("用户类型")]
        public FlowActorType FlowUser { get; set; }

        public override IList<IUser> Resolve(IWorkflowContext context)
        {
            var returnValue = new List<IUser>();
            
            if (FlowUser == FlowActorType.Creator)
            {
                returnValue.Add(context.FlowInstance.Creator);
            }
            else if (FlowUser == FlowActorType.CurrentUser)
            {
                returnValue.Add(context.CurrentUser);
            }
            else if (FlowUser == FlowActorType.CurrentParticipant)
            {
                var user = DBContext<IUser>.Instance.Get(context.CurrentTask.PartUserId);
                returnValue.Add(user);
            }
            return returnValue;
        }
    }
    public enum FlowActorType
    {
        [Description("创建人")]
        Creator = 0,

        [Description("当前用户")]
        CurrentUser = 2,

        [Description("参与人")]
        CurrentParticipant = 3
    }
}
