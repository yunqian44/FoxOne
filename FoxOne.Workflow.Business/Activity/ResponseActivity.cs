/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/1/26 15:42:22
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
    [DisplayName("审批步骤")]
    public class ResponseActivity : BaseActivity, IEditForm
    {
        [DefaultValue(typeof(UserSelectActor)), DisplayName("知会参与者")]
        public IActor NotifyActor
        {
            get;
            set;
        }

        [DisplayName("允许自由转送")]
        public bool IsFreeApproval { get; set; }


        [DisplayName("允许多选")]
        public virtual bool IsMultipleSelect
        {
            get;
            set;
        }

        [DisplayName("多选标记")]
        public virtual string MultipleSelectTag
        {
            get;
            set;
        }

        [DisplayName("过期规则"), DefaultValue(ExpireRule.ByWorkDay)]
        public virtual ExpireRule ExpireRule
        {
            get;
            set;
        }

        [DisplayName("过期天数")]
        public virtual int Interval { get; set; }

        [DisplayName("审批方式")]
        public ResponseRuleType ResponseRuleType
        {
            get;
            set;
        }

        [DisplayName("能否编辑表单")]
        public virtual bool CanEditForm
        {
            get;
            set;
        }

         [DisplayName("是否为开始节点")]
        public virtual bool IsRoot { get; set; }

        public bool ShowUserSelect(IWorkflowContext context)
        {
            return CanExitInner(context, false);
        }

        public override bool CanExit(IWorkflowContext context)
        {

            return CanExitInner(context, true);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="updateNextWorkItem">是否更新下一TaskStat=5工作项的状态为0</param>
        /// <returns></returns>
        private bool CanExitInner(IWorkflowContext context, bool updateNextWorkItem = false)
        {
            bool returnValue = true;
            if (ResponseRuleType != ResponseRuleType.OneResponse)
            {
                var workitems = context.FlowInstance.WorkItems.Where(o => o.ItemSeq == context.CurrentTask.ItemSeq);
                if (ResponseRuleType == ResponseRuleType.AllResponse)
                {
                    //如果审批方式 是 全部响应，则判断 相同的TaskSeq下还有无未结束的工作项，如果有则不允许退出步骤。
                    int workItemsCount = workitems.Count(o => o.Status < WorkItemStatus.Finished && o.ItemId != context.CurrentTask.ItemId);
                    if (workItemsCount > 0)
                    {
                        Log(string.Format("{0}:步骤[{1}]审批方式为【全部响应】，仍有{2}个工作项未结束，不能退出", context.FlowInstance.Id, this.Name, workItemsCount));
                        returnValue = false;
                    }
                }
                if (ResponseRuleType == ResponseRuleType.SerialResponse)
                {
                    //如果是串行审批，把当前工作项的下一工作项状态置为0
                    var nextItem = workitems.FirstOrDefault(o => o.Status == WorkItemStatus.Pause && o.ItemId == context.CurrentTask.ItemId + 1);
                    if (nextItem != null)
                    {
                        if (updateNextWorkItem)
                        {
                            Log(string.Format("{0}:步骤[{1}]审批方式为【串行审批】，设置下一工作项状态为0，接收时间和过期时间重新计算", context.FlowInstance.Id, this.Name));
                            nextItem.Status = 0;
                            nextItem.ReceiveTime = DateTime.Now;
                            nextItem.ExpiredTime = GetExpiredTime();
                            context.FlowInstance.UpdateWorkItem(nextItem);
                        }
                        returnValue = false;
                    }
                }
            }
            return returnValue;
        }

        public virtual DateTime GetExpiredTime()
        {
            if (this.Interval <= 0)
            {
                this.Interval = 3;
            }
            var workDayService = ObjectHelper.GetObject<IWorkDayService>();
            DateTime returnValue = DateTime.Now;
            if (ExpireRule == ExpireRule.ByNaturalDay)
            {
                returnValue = DateTime.Now.AddDays(Interval);
            }
            else
            {
                returnValue = workDayService.GetTimeAfterSpan(DateTime.Now, new TimeSpan(Interval, 0, 0, 0));
            }
            return returnValue;
        }

        public override void Enter(IWorkflowContext context)
        {
            var users = Actor.Resolve(context).OrderBy(o => o.Rank);
            string instanceId = context.FlowInstance.Id;
            var lastItem = context.FlowInstance.WorkItems.OrderByDescending(o => o.ItemId).First();
            DateTime dt = DateTime.Now;
            int i = 1;
            string levelCode = context.CurrentTask.LevelCode;
            string keyInfo = context.CurrentTask.ParallelInfo;
            if (!context.LevelCode.Equals(context.CurrentTask.LevelCode, StringComparison.CurrentCultureIgnoreCase))
            {
                //从普通审批步骤，进入到会签入的审批步骤，或者从会签入的审批步骤，退出到普通的审批步骤，都会进入这里。
                //主要体现是上下文中的levelCode不等于当前工作项的levelCode，因为经过“并行开始”或“并行结束”步骤，都会把上下文的levelCode改掉。
                levelCode = context.LevelCode;

                if (levelCode.EndsWith(":"))
                {
                    //并行开始步骤 直接 连接 审批步骤 时，会进入这里。levelCode不能单纯取上下文中的levelCode，要去自动生成
                    //上下文的可能是00-4:,自动生成后，就会变成00-4:01,00-4:02....
                    levelCode = context.FlowInstance.GetLevelCode(context.LevelCode, 1)[0];
                }
                else
                {
                    if (levelCode != "00")
                    {
                        //从嵌套会签中退出外层会签时，需要为当前工作项找回外层会签的并行keyInfo
                        var firstWorkItem = context.FlowInstance.WorkItems.OrderBy(o => o.ItemId).FirstOrDefault(o => o.LevelCode == levelCode);
                        if (firstWorkItem != null)
                        {
                            keyInfo = firstWorkItem.ParallelInfo;
                        }
                    }
                }
            }
            foreach (var user in users)
            {
                var workflowItem = ObjectHelper.GetObject<IWorkflowItem>();
                workflowItem.PartUserId = user.Id;
                workflowItem.PartUserName = user.Name;
                workflowItem.InstanceId = instanceId;
                workflowItem.ItemId = lastItem.ItemId + i;
                workflowItem.ItemSeq = lastItem.ItemSeq + 1;
                workflowItem.PartDepepartmentName = user.Department.Name;
                workflowItem.PartDepartmentId = user.Department.Id;
                workflowItem.ParallelInfo = keyInfo;
                if (ResponseRuleType == ResponseRuleType.SerialResponse && i != 1)
                {
                    workflowItem.Status = WorkItemStatus.Pause;
                }
                else
                {
                    workflowItem.Status = WorkItemStatus.Sent; //如果是 串行审批  工作项的状态在这里是不一样的。
                }
                workflowItem.ReceiveTime = dt;
                workflowItem.ExpiredTime = GetExpiredTime();
                workflowItem.PreItemId = context.CurrentTask.ItemId;
                workflowItem.AppCode = context.FlowInstance.ApplicationId;
                workflowItem.CurrentActivity = this.Name;
                workflowItem.Alias = Alias;
                workflowItem.LevelCode = levelCode;
                workflowItem.PasserUserId = context.CurrentUser.Id;
                workflowItem.PasserUserName = context.CurrentUser.Name;
                context.FlowInstance.InsertWorkItem(workflowItem);
                i++;
            }
            SendOtherToRead(context);
        }

        private void SendOtherToRead(IWorkflowContext context)
        {
            if (NotifyActor != null)
            {
                var notifyUser = NotifyActor.Resolve(context);
                if (notifyUser != null && notifyUser.Count > 0)
                {
                    var currentTask = context.CurrentTask;
                    int taskId = context.FlowInstance.GetMaxReadTaskID();
                    foreach (var user in notifyUser)
                    {
                        var newItem = ObjectHelper.GetObject<IWorkflowItem>();
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

        public override void Execute(IWorkflowContext context)
        {
            context.CurrentTask.Status = WorkItemStatus.Finished;
            if (!context.CurrentTask.PartUserId.Equals(context.CurrentUser.Id, StringComparison.CurrentCultureIgnoreCase))
            {
                context.CurrentTask.AssigneeUserId = context.CurrentUser.Id;
                context.CurrentTask.AssigneeUserName = context.CurrentUser.Name;
            }
            if (context.UserChoice != null && context.UserChoice.Count > 0)
            {
                context.CurrentTask.UserChoice = string.Join(",", context.UserChoice.Select(o => o.Choice).ToArray());
            }
            context.CurrentTask.FinishTime = DateTime.Now;
            context.FlowInstance.UpdateWorkItem(context.CurrentTask);
        }

        public override void Exit(IWorkflowContext context)
        {
            if (ResponseRuleType == ResponseRuleType.OneResponse)
            {
                //如果审批方式 是 任一人响应，则判断 相同的TaskSeq下还有无未结束的工作项，如果有 则 自动结束
                //var itemHelper = ObjectHelper.GetObject<IWorkflowItemService>();
                var workitems = context.FlowInstance.WorkItems.Where(o => o.ItemSeq == context.CurrentTask.ItemSeq && o.ItemId != context.CurrentTask.ItemId && o.Status < WorkItemStatus.Finished).ToList();
                var workitemsCount = workitems.Count;
                if (workitems != null && workitemsCount > 0)
                {
                    Log(string.Format("{0}:步骤[{1}]审批方式为【任一人响应】，仍有{2}个工作项未结束，设为自动结束", context.FlowInstance.Id, this.Name, workitemsCount));
                    //更新工作项 为 自动完成
                    foreach (var workItem in workitems)
                    {
                        workItem.FinishTime = DateTime.Now;
                        workItem.AutoFinish = true;
                        workItem.Status = WorkItemStatus.AutoFinished;
                        workItem.AssigneeUserId = context.CurrentUser.Id;
                        workItem.AssigneeUserName = context.CurrentUser.Name;
                        workItem.UserChoice = "任一人响应自动结束";
                    }
                    context.FlowInstance.UpdateWorkItem(workitems);
                }
            }
        }
    }

    public enum ExpireRule
    {
        [Description("工作日")]
        ByWorkDay,

        [Description("自然日")]
        ByNaturalDay
    }

    public enum ResponseRuleType
    {
        /// <summary>
        /// 只要一个审批人选择“同意”，就可以退出这个环节
        /// </summary>
        [Description("任一人响应")]
        OneResponse,
        /// <summary>
        /// 只有在所有人都批示的情况下，才可以退出这个环节
        /// </summary>
        [Description("全部人响应")]
        AllResponse,
        /// <summary>
        /// 串行审批
        /// </summary>
        [Description("串行审批")]
        SerialResponse
    }
}
