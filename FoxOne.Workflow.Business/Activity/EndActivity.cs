/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/1/26 15:42:45
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
    [DisplayName("结束步骤")]
    public class EndActivity:BaseActivity
    {

        public override void Enter(IWorkflowContext context)
        {
            //增加一条工作项，系统自动完成，LevelCode取CurrentTask的
            string levelCode = context.CurrentTask.LevelCode;
            var workitem = ObjectHelper.GetObject<IWorkflowItem>();

            string instanceId = context.FlowInstance.Id;
            var newestItem = context.FlowInstance.WorkItems.OrderByDescending(o => o.ItemId).First();
            workitem.AutoFinish = true;
            workitem.Status = WorkItemStatus.AutoFinished;
            workitem.AppCode = context.CurrentTask.AppCode;
            workitem.ItemId = newestItem.ItemId + 1;
            workitem.ItemSeq = newestItem.ItemSeq + 1;
            workitem.PartUserName = "系统";
            workitem.InstanceId = context.FlowInstance.Id;
            workitem.ReceiveTime = DateTime.Now;
            workitem.FinishTime = DateTime.Now;
            workitem.LevelCode = levelCode;
            workitem.Alias = Alias;
            workitem.CurrentActivity = Name;
            workitem.PreItemId = context.CurrentTask.ItemId;
            workitem.PasserUserId = context.CurrentUser.Id;
            workitem.PasserUserName = context.CurrentUser.Name;
            context.FlowInstance.InsertWorkItem(workitem);
            context.FlowInstance.FlowTag = FlowStatus.Finished;
            context.FlowInstance.EndTime = DateTime.Now;
            context.FlowInstance.WorkItemNewSeq = workitem.ItemSeq;
            context.FlowInstance.WorkItemNewTask = workitem.ItemId;
            Owner.UpdateInstance(context.FlowInstance);
            context.FlowInstance.DeleteParameter();
            SendOtherToRead(context);
        }

        public void SendOtherToRead(IWorkflowContext context)
        {
            if (Actor != null)
            {
                var users = Actor.Resolve(context);
                if (users.Count > 0)
                {
                    var currentTask = context.CurrentTask;
                    int taskId = context.FlowInstance.GetMaxReadTaskID();
                    foreach (var user in users)
                    {
                        var newItem = ObjectHelper.GetObject<IWorkflowItem>("Read");
                        newItem.InstanceId = currentTask.InstanceId;
                        newItem.ReceiveTime = DateTime.Now;
                        newItem.PartUserId = user.Id;
                        newItem.PartUserName = user.Name;
                        newItem.PartDepartmentId = user.Department.Id;
                        newItem.PartDepepartmentName = user.Department.Name;
                        newItem.Status = WorkItemStatus.Sent;
                        newItem.ItemId = ++taskId;
                        newItem.Alias = "传阅";
                        newItem.CurrentActivity = "传阅";
                        newItem.AppCode = currentTask.AppCode;
                        newItem.PreItemId = currentTask.ItemId;
                        newItem.PasserUserId = context.CurrentUser.Id;
                        newItem.PasserUserName = context.CurrentUser.Name;
                        context.FlowInstance.InsertWorkItem(newItem);
                    }
                }
            }
        }
    }
}
