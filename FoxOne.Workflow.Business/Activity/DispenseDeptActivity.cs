/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/6/8 16:46:07
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
    [DisplayName("分发部门步骤")]
    public class DispenseDeptActivity:ResponseActivity
    {
        public override void Enter(IWorkflowContext context)
        {
            if (context.LevelCode == "00")
            {
                throw new Exception("分发部门步骤只能用于并行流程开始步骤后");
            }
            if (context.LevelCode.Equals(context.CurrentTask.LevelCode, StringComparison.CurrentCultureIgnoreCase)
                || context.CurrentTask.LevelCode.StartsWith(context.LevelCode, StringComparison.CurrentCultureIgnoreCase))
            {
                //当前上下文中的LevelCode与当前工作项的LevelCode一致，说明是从会签内的某一步退回到分发部门步骤
                //这时只需当成普通的审核步骤来处理工作项即可
                base.Enter(context);
            }
            else
            {
                var users = Actor.Resolve(context);
                var usersGroupBy = users.GroupBy(o => o.DepartmentId);
                int taskSeqCount = usersGroupBy.Count();
                string instanceId = context.FlowInstance.Id;
                var newestItem = context.FlowInstance.WorkItems.OrderByDescending(o => o.ItemId).First();
                DateTime dt = DateTime.Now;
                int i = 1, j = 1;
                bool isFirst = true;
                var levelCodes =context.FlowInstance.GetLevelCode(context.LevelCode, taskSeqCount);
                foreach (var user in usersGroupBy)
                {
                    isFirst = true;
                    foreach (var u in user)
                    {
                        var workflowItem = ObjectHelper.GetObject<IWorkflowItem>();
                        workflowItem.PartUserId = u.Id;
                        workflowItem.PartUserName = u.Name;
                        workflowItem.InstanceId = instanceId;
                        workflowItem.ItemId = newestItem.ItemId + j;
                        workflowItem.ItemSeq = newestItem.ItemSeq + i;
                        workflowItem.PartDepepartmentName = u.Department.Name;
                        workflowItem.PartDepartmentId = u.Department.Id;
                        workflowItem.ParallelInfo = u.Department.Id;
                        if (ResponseRuleType == ResponseRuleType.SerialResponse && !isFirst)
                        {
                            workflowItem.Status = WorkItemStatus.Pause;
                        }
                        else
                        {
                            workflowItem.Status = WorkItemStatus.Sent; //如果是 串行审批  工作项的状态在这里是不一样的。
                            isFirst = false;
                        }
                        workflowItem.ReceiveTime = dt;
                        workflowItem.ExpiredTime = GetExpiredTime();
                        workflowItem.PreItemId = context.CurrentTask.ItemId;
                        workflowItem.AppCode = context.FlowInstance.ApplicationId;
                        workflowItem.CurrentActivity = this.Name;
                        workflowItem.Alias = Alias;
                        workflowItem.PasserUserId = context.CurrentUser.Id;
                        workflowItem.PasserUserName = context.CurrentUser.Name;
                        workflowItem.LevelCode = levelCodes[i - 1];
                        context.FlowInstance.InsertWorkItem(workflowItem);
                        j++;
                    }
                    i++;
                }
            }
        }
    }
}
