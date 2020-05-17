/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/6/25 15:40:36
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
    [DisplayName("会签发起人参与者")]
    public class ParallelOriginatorActor:BaseActor
    {

        public override IList<IUser> Resolve(IWorkflowContext context)
        {
            string levelCode = context.CurrentTask.LevelCode;
            if (levelCode.IndexOf('-') > 0)
            {
                int corTaskId = int.Parse(levelCode.Split('-').Last().Split(':')[0]);
                int taskId = context.FlowInstance.WorkItems.FirstOrDefault(o => o.ItemId == corTaskId).PreItemId;
                var workItem = context.FlowInstance.WorkItems.FirstOrDefault(o => o.ItemId == taskId);
                var user = DBContext<IUser>.Instance.Get(workItem.PartUserId);
                user.DepartmentId = workItem.PartDepartmentId;
                return new List<IUser>() { user };
            }
            return null;
        }
    }
}
