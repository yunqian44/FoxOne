/*********************************************************
* 作　　者：刘海峰
* 联系邮箱：mailTo:liuhf@FoxOne.net
* 创建时间：2015/9/28 15:01:48
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
    [DisplayName("分发步骤")]
    public class BreakdownActivity : ResponseActivity
    {
        public override void Exit(IWorkflowContext context)
        {
            var preItem = context.FlowInstance.WorkItems.FirstOrDefault(o => o.ItemId == context.CurrentTask.PreItemId);
            var lastItem = context.FlowInstance.WorkItems.OrderByDescending(o => o.ItemId).First();
            var workflowItem = ObjectHelper.GetObject<IWorkflowItem>();
            workflowItem.PartUserId = preItem.PartUserId;
            workflowItem.PartUserName = preItem.PartUserName;
            workflowItem.InstanceId = preItem.InstanceId;
            workflowItem.ItemId = lastItem.ItemId + 1;
            workflowItem.ItemSeq = lastItem.ItemSeq + 1;
            workflowItem.PartDepepartmentName = preItem.PartDepepartmentName;
            workflowItem.PartDepartmentId = preItem.PartDepartmentId;
            workflowItem.ParallelInfo = preItem.ParallelInfo;
            workflowItem.Status = WorkItemStatus.Sent;
            workflowItem.ReceiveTime = DateTime.Now;
            workflowItem.ExpiredTime = GetExpiredTime();
            workflowItem.PreItemId = context.CurrentTask.ItemId;
            workflowItem.AppCode = preItem.AppCode;
            workflowItem.CurrentActivity = preItem.CurrentActivity;
            workflowItem.Alias = preItem.Alias;
            workflowItem.LevelCode = preItem.LevelCode;
            workflowItem.PasserUserId = context.CurrentUser.Id;
            workflowItem.PasserUserName = context.CurrentUser.Name;
            context.FlowInstance.InsertWorkItem(workflowItem);
        }
    }
}
