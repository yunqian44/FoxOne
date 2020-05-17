/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@bingosoft.net
 * 创建时间：2014/12/29 17:24:46
 * 描述说明：
 * *******************************************************/
using System;
using System.Collections.Generic;
using FoxOne.Workflow.Kernel;
using FoxOne.Core;
using FoxOne.Workflow.DataAccess;
using System.Security.Principal;
using FoxOne.Workflow.Business;
using System.Linq;
using System.Transactions;
using FoxOne.Data;
namespace FoxOne.Workflow.Business
{
    public partial class WorkflowHelper : IDisposable
    {
        private IWorkflowInstanceService _instanceService;
        private IWorkflowInstanceService instanceService
        {
            get
            {
                return _instanceService ?? (_instanceService = ObjectHelper.GetObject<IWorkflowInstanceService>());
            }
        }

        public WorkflowHelper(string loginId)
        {
            CurrentUser = DBContext<IUser>.Instance.FirstOrDefault(o => o.LoginId.Equals(loginId, StringComparison.OrdinalIgnoreCase));
        }
        public WorkflowHelper(IPrincipal user)
            : this(user.Identity.Name)
        {

        }

        public WorkflowHelper(IUser user)
        {
            CurrentUser = user;
        }

        public IWorkflowItem CurrentItem { get; private set; }

        public IActivity CurrentActivity { get; private set; }

        public IWorkflow CurrentWorkflow { get; private set; }

        public IUser CurrentUser { get; private set; }

        public IWorkflowInstance FlowInstance
        {
            get;
            private set;
        }

        public void StartWorkflow(string appCode, string procName, string dataLocator, int impoLevel = 0, int secret = 0)
        {
            CurrentWorkflow = Build(appCode);
            FlowInstance = GetNewInstance(appCode, procName, dataLocator, impoLevel, secret);
            using (TransactionScope tran = new TransactionScope())
            {
                CurrentWorkflow.AddInstance(FlowInstance);
                CurrentItem = GetNewWorkItem(CurrentWorkflow.Root, CurrentUser, appCode, FlowInstance.Id);
                FlowInstance.InsertWorkItem(CurrentItem);
                CurrentActivity = CurrentWorkflow.Root;
                tran.Complete();
            }
        }

        public void UpdateInstance(string procName, string dataLocator, int impoLevel, int secret)
        {
            Validate();
            FlowInstance.InstanceName = procName;
            FlowInstance.DataLocator = dataLocator;
            FlowInstance.ImportantLevel = impoLevel;
            FlowInstance.SecretLevel = secret;
            CurrentWorkflow.UpdateInstance(FlowInstance);
        }

        public IWorkflow Build(string appCode)
        {
            var app = DBContext<IWorkflowApplication>.Instance.Get(appCode);
            string workflowId = app == null ? "1" : app.WorkflowId;//测试时，未定义应用。
            var returnValue = ObjectHelper.GetObject<IWorkflowBuilder>().Build(workflowId);
            ValidateWorkflow(returnValue);
            return returnValue;
        }

        public void OpenWorkflow(string dataLocator)
        {
            var instance = instanceService.GetInstanceByDataLocator(dataLocator);
            if (instance == null)
            {
                throw new FoxOneException("不存在datalocator为：{0}的流程实例", dataLocator);
            }
            OpenWorkflow(instance, instance.WorkItemNewTask);
        }

        public void OpenWorkflow(string instanceId, int itemId)
        {
            var instance = instanceService.Get(instanceId);
            if (instance == null)
            {
                throw new FoxOneException("不存在实例号为：{0}的流程实例", instanceId);
            }
            OpenWorkflow(instance, itemId);
        }

        public static void ClearCache(string definitionId)
        {
            ObjectHelper.GetObject<IWorkflowBuilder>().ClearCache(definitionId);
        }

        public List<NextStep> GetNextStep()
        {
            Validate();
            List<NextStep> returnValue = new List<NextStep>();
            IWorkflowContext context = GetWorkflowContext();
            var trans = GetAvailableTransitions(context);
            foreach (var tran in trans)
            {
                var nextStep = new NextStep()
                {
                    Label = tran.Label,
                    LabelDescription = tran.Description,
                    StepName = tran.To.Name,
                    Rank = tran.Rank,
                    NeedUser = true
                };
                GetNextStepUser(nextStep, tran.To, context);
                returnValue.Add(nextStep);
            }
            return returnValue.OrderBy(o => o.Rank).ToList();
        }

        public List<NextStep> GetAllStep()
        {
            Validate();
            List<NextStep> returnValue = new List<NextStep>();
            IWorkflowContext context = GetWorkflowContext();
            foreach (var acti in CurrentWorkflow.Activities)
            {
                if ((acti is ResponseActivity) || (acti is EndActivity))
                {
                    var nextStep = new NextStep()
                    {
                        Label = acti.Alias,
                        LabelDescription = "",
                        StepName = acti.Name,
                        NeedUser = true
                    };
                    returnValue.Add(nextStep);
                }
            }
            return returnValue.OrderBy(o => o.Rank).ToList();
        }

        public NextStep GetStepUser(string activityName)
        {
            Validate();
            var returnValue = new NextStep();
            var activity = CurrentWorkflow[activityName];
            if (activity == null)
            {
                throw new FoxOneException("流程中不存在步骤名为：{0} 的步骤", activityName);
            }
            returnValue.NeedUser = true;
            returnValue.StepName = activity.Name;
            returnValue.Label = activity.Alias;
            GetNextStepUser(returnValue, activity, GetWorkflowContext());

            return returnValue;
        }

        private void GetNextStepUser(NextStep nextStep, IActivity activity, IWorkflowContext context)
        {
            IActor actor = activity.Actor;
            nextStep.Users = new List<NextStepUser>();
            if (activity is ResponseActivity)
            {
                nextStep.MultipleSelectTag = (activity as ResponseActivity).MultipleSelectTag;
            }
            if (actor is UserSelectActor)
            {
                UserSelectActor userSelectActor = actor as UserSelectActor;
                nextStep.AllowFree = userSelectActor.AllowFree;
                nextStep.OnlySingleSel = userSelectActor.OnlySingleSelect;
                nextStep.AutoSelectAll = userSelectActor.AutoSelectAll;
                nextStep.AllowSelect = true;
                actor = userSelectActor.InnerActor;
                actor.Owner = activity;
            }
            else
            {
                nextStep.AllowFree = false;
                nextStep.AutoSelectAll = true;
                nextStep.AllowSelect = false;
                nextStep.OnlySingleSel = false;
            }
            if (actor != null)
            {
                try
                {
                    var groupByActors = actor.Resolve(context).GroupBy(o => o.DepartmentId);
                    foreach (var actors in groupByActors)
                    {
                        foreach (IUser u in actors.OrderBy(o => o.Rank))
                        {
                            nextStep.Users.Add(new NextStepUser() { StepName = activity.Name, ID = u.Id, Name = u.Name, OrgId = u.Department.Id, OrgName = u.Department.Name, Rank = u.Rank, OrgRank = u.Department.Rank });
                        }
                    }
                }
                catch (Exception ex) { nextStep.Message = ex.Message; }
            }
            if (activity is EndActivity)
            {
                nextStep.NeedUser = false;
            }
        }

        /// <summary>
        /// 将特定字符串转换成用于运行流程的参数
        /// </summary>
        /// <param name="userChoice">格式：userId1_orgId1_stepName1,userId2_orgId2_stepName2...</param>
        /// <returns></returns>
        public IList<IWorkflowChoice> GetUserChoice(string userChoice)
        {
            IList<IWorkflowChoice> choices = new List<IWorkflowChoice>();
            string[] tempChoice = userChoice.Split(',');
            foreach (var c in tempChoice)
            {
                var tempUser = c.Split('_');
                var choice = choices.FirstOrDefault(o => o.Choice == tempUser[2]);
                if (tempUser[0].Equals("NULL", StringComparison.CurrentCultureIgnoreCase))
                {
                    var workflowChoice = ObjectHelper.GetObject<IWorkflowChoice>();
                    workflowChoice.Choice = tempUser[2];
                    workflowChoice.Participant = new List<IUser>();

                    choices.Add(workflowChoice);
                }
                else
                {
                    var user = DBContext<IUser>.Instance.Get(tempUser[0]);
                    user.DepartmentId = tempUser[1];
                    int rank = 0;
                    if (int.TryParse(tempUser[3], out rank))
                    {
                        user.Rank = rank;
                    }
                    if (choice == null)
                    {
                        var workflowChoice = ObjectHelper.GetObject<IWorkflowChoice>();
                        workflowChoice.Choice = tempUser[2];
                        workflowChoice.Participant = new List<IUser>() { user };
                        choices.Add(workflowChoice);
                    }
                    else
                    {
                        choice.Participant.Add(user);
                    }
                }
            }
            return choices;
        }

        public IList<IWorkflowChoice> GetUserChoice(string stepName, NextStepUser user)
        {
            var returnValue = new List<IWorkflowChoice>();
            var choice = ObjectHelper.GetObject<IWorkflowChoice>();
            choice.Choice = stepName;
            choice.Participant = new List<IUser>();
            var tempUser = DBContext<IUser>.Instance.Get(user.ID);
            tempUser.DepartmentId = user.OrgId;
            choice.Participant.Add(tempUser);
            returnValue.Add(choice);
            return returnValue;
        }

        public void Run(IList<IWorkflowChoice> userChoice = null)
        {
            Validate();
            var context = GetWorkflowContext();
            if (userChoice != null)
            {
                context.UserChoice = userChoice;
            }
            CurrentWorkflow.Run(context);
        }

        public void DeleteWorkflow()
        {
            Validate();

            using (var tran = new TransactionScope())
            {

                FlowInstance.DeleteWorkItem();
                CurrentWorkflow.DeleteInstance(FlowInstance);
                tran.Complete();
            }
            Log("删除了流程实例（包括相关联的工作项及流程参数）");
        }

        public void SetParameter(string key, string value)
        {
            Validate();
            if (!FlowInstance.Parameters.ContainsKey(key) || !FlowInstance.Parameters[key].Equals(value))
            {
                FlowInstance.SetParameter(key, value);
                //Log(string.Format("设置了流程参数key:{0},value:{1}", key, value));
            }
        }

        private void Log(string operation)
        {
            Logger.GetLogger("Workflow").Info(string.Format("{0}:{1} -- 操作人:{2}", FlowInstance.Id, operation, CurrentUser.Name));
        }

        public void SetOpinion(string opinionContent, int opinionArea)
        {
            Validate();
            CurrentItem.OpinionContent = opinionContent;
            CurrentItem.OpinionType = opinionArea;
            FlowInstance.UpdateWorkItem(CurrentItem);
        }

        public bool ShowUserSelect()
        {
            Validate();
            if (CurrentActivity is ResponseActivity && (CurrentActivity as ResponseActivity).NeedChoice)
            {
                var context = GetWorkflowContext();
                return (CurrentActivity as ResponseActivity).ShowUserSelect(context);
            }
            else
            {
                return false;
            }
        }

        public void Switch(IWorkflowChoice userChoice)
        {
            Validate();
            if (userChoice == null || string.IsNullOrEmpty(userChoice.Choice))
            {
                throw new Exception(string.Format("请选择要跳转到的步骤", userChoice.Choice));
            }
            var targetActi = CurrentWorkflow[userChoice.Choice];
            if (targetActi == null)
            {
                throw new Exception(string.Format("流程定义中不存在名为【{0}】的步骤", userChoice.Choice));
            }
            if (!(targetActi is ResponseActivity))
            {
                throw new Exception("不允许跳转到非审批步骤中");
            }
            using (var tran = new TransactionScope())
            {
                SetAutoFinished("跳转流程");
                var context = GetWorkflowContext();
                context.LevelCode = "00";
                context.UserChoice = new List<IWorkflowChoice>() { userChoice };
                targetActi.Enter(context);
                tran.Complete();
            }
            Log(string.Format("跳转了流程，跳转到步骤【{0}】", userChoice.Choice));
        }

        public void Dispose()
        {

        }

        public bool ForceToEnd()
        {
            Validate();
            using (TransactionScope tran = new TransactionScope())
            {

                SetAutoFinished("强制结束");
                var context = GetWorkflowContext();
                context.LevelCode = "00";
                var endActivity = new EndActivity() { Name = "强制结束", Alias = "强制结束" };
                endActivity.Owner = CurrentWorkflow;
                endActivity.Enter(context);
                tran.Complete();
            }
            Log("强制结束了流程");
            return true;
        }


        public bool Pushback()
        {
            Validate();
            if (FlowInstance.FlowTag == FlowStatus.Finished)
            {
                throw new FoxOneException("流程实例已结束，不能执行该操作!");
            }
            if (CurrentItem.Status >= WorkItemStatus.Finished)
            {
                throw new FoxOneException("当前工作项已结束，不能执行该操作!");
            }
            if (CurrentItem.PreItemId == 0)
            {
                throw new FoxOneException("开始步骤不允许退回操作！");
            }
            var preTask = FlowInstance.WorkItems.FirstOrDefault(o => o.ItemId == CurrentItem.PreItemId);
            if (preTask == null)
            {
                throw new FoxOneException("上一工作项为空，不能执行该操作!");
            }
            if (preTask.PartUserName.Equals("系统", StringComparison.CurrentCultureIgnoreCase)
                || preTask.LevelCode != CurrentItem.LevelCode
                || preTask.Status == WorkItemStatus.AutoFinished)
            {
                throw new FoxOneException("不允许回退到系统处理步骤或跨越会签内外步骤");
            }
            var workItems = FlowInstance.WorkItems.Where(o => o.PreItemId == CurrentItem.PreItemId).ToList();
            var updateItems = new List<IWorkflowItem>();
            if (workItems.Count > 1)
            {
                //上一工作项处理人员发送了多人处理
                foreach (var item in workItems)
                {
                    if (item.Status != WorkItemStatus.Finished && item.ItemId != CurrentItem.ItemId)
                    {
                        item.Status = WorkItemStatus.AutoFinished;
                        item.FinishTime = DateTime.Now;
                        item.AssigneeUserId = CurrentUser.Id;
                        item.AssigneeUserName = CurrentUser.Name;
                        item.AutoFinish = true;
                        item.UserChoice = "自动结束：回退上一步";
                        updateItems.Add(item);
                    }
                }
            }
            CurrentItem.FinishTime = DateTime.Now;
            CurrentItem.UserChoice = "回退上一步";
            CurrentItem.Status = WorkItemStatus.Finished;
            CurrentItem.AutoFinish = false;
            updateItems.Add(CurrentItem);
            IUser user = DBContext<IUser>.Instance.Get(preTask.PartUserId);
            var newItem = GetNewWorkItem(CurrentWorkflow[preTask.CurrentActivity], user, CurrentItem.AppCode, CurrentItem.InstanceId);
            var newestItem = FlowInstance.WorkItems.OrderByDescending(o => o.ItemId).First();
            newItem.ItemId = newestItem.ItemId + 1;
            newItem.ItemSeq = newestItem.ItemSeq + 1;
            newItem.PreItemId = preTask.PreItemId;
            newItem.PasserUserId = preTask.PasserUserId;
            newItem.PasserUserName = preTask.PasserUserName;
            newItem.LevelCode = preTask.LevelCode;
            newItem.ParallelInfo = preTask.ParallelInfo;
            using (var tran = new TransactionScope())
            {
                FlowInstance.UpdateWorkItem(updateItems);
                FlowInstance.InsertWorkItem(newItem);
                tran.Complete();
            }
            Log(string.Format("退回了流程到步骤【{0}】", preTask.CurrentActivity));
            return true;
        }

        public bool Rollback()
        {
            Validate();
            if (FlowInstance.FlowTag == FlowStatus.Finished)
            {
                throw new FoxOneException("流程实例已结束，不能执行该操作!");
            }
            if (CurrentItem.Status != WorkItemStatus.Finished)
            {
                throw new FoxOneException("当前工作项未完成，不能执行该操作!");
            }
            var workItems = FlowInstance.WorkItems.Where(o => o.PreItemId == CurrentItem.ItemId).ToList();
            if (workItems.Count(o => o.Status == WorkItemStatus.Finished || o.Status == WorkItemStatus.AutoFinished) > 0)
            {
                throw new FoxOneException("后续工作项已有完成，不允许执行撤回操作！");
            }
            foreach (var item in workItems)
            {
                item.Status = WorkItemStatus.BeRollBack;
                item.AssigneeUserId = CurrentUser.Id;
                item.AssigneeUserName = CurrentUser.Name;
                item.FinishTime = DateTime.Now;
                item.AutoFinish = true;
                item.UserChoice = "自动结束，原因：被撤回";
            }
            using (var tran = new TransactionScope())
            {

                FlowInstance.UpdateWorkItem(workItems);
                CurrentItem.Status = WorkItemStatus.RollBack;
                FlowInstance.UpdateWorkItem(CurrentItem);
                IUser user = DBContext<IUser>.Instance.Get(CurrentItem.PartUserId);
                var newItem = GetNewWorkItem(CurrentWorkflow[CurrentItem.CurrentActivity], user, CurrentItem.AppCode, CurrentItem.InstanceId);
                var newestItem = FlowInstance.WorkItems.OrderByDescending(o => o.ItemId).First();
                newItem.ItemId = newestItem.ItemId + 1;
                newItem.ItemSeq = newestItem.ItemSeq + 1;
                newItem.PasserUserId = CurrentItem.PasserUserId;
                newItem.PasserUserName = CurrentItem.PasserUserName;
                newItem.LevelCode = CurrentItem.LevelCode;
                newItem.ParallelInfo = CurrentItem.ParallelInfo;
                FlowInstance.InsertWorkItem(newItem);
                tran.Complete();
            }
            Log(string.Format("在步骤【{0}】撤回了流程", CurrentItem.CurrentActivity));
            return true;
        }

        public bool PushbackToRoot()
        {
            Validate();
            using (var tran = new TransactionScope())
            {
                SetAutoFinished("退回拟稿人");
                var context = GetWorkflowContext();
                context.LevelCode = "00";
                CurrentWorkflow.Root.Enter(context);
                tran.Complete();
            }
            Log("退回拟稿人");
            return true;
        }

        public void BackToActivity(int itemId)
        {
            Validate();
            if (FlowInstance.FlowTag == FlowStatus.Finished)
            {
                //throw new FoxOneException("不允许此操作，流程实例已结束！");
            }
            var item = FlowInstance.WorkItems.FirstOrDefault(o => o.ItemId == itemId);
            if (item == null || item.PartUserName == "系统" || item.Alias == "传阅" || item.LevelCode != "00")
            {
                throw new FoxOneException("暂不支持回到传阅步骤、自动步骤及会签内步骤");
            }
            using (TransactionScope tran = new TransactionScope())
            {
                FlowInstance.BackToTask(itemId);
                instanceService.BackToRunning(FlowInstance.Id, itemId, item.ItemSeq, item.CurrentActivity);
                tran.Complete();
            }
        }

        public void SendToOtherToRead(IList<string> userIds)
        {
            SendToOtherToRead(string.Join(",", userIds.ToArray()));
        }

        /// <summary>
        /// 传阅
        /// </summary>
        /// <param name="userIds">用户ID，多个用逗号隔开</param>
        public void SendToOtherToRead(string userIds)
        {
            Validate();
            var userIdSplit = userIds.Split(new char[] { ',', '|', ';' }, StringSplitOptions.RemoveEmptyEntries);
            var users = DBContext<IUser>.Instance.Where(o => userIdSplit.Contains(o.Id, StringComparer.OrdinalIgnoreCase)).ToList();
            int taskId = FlowInstance.GetMaxReadTaskID();
            using (var tran = new TransactionScope())
            {
                foreach (var user in users)
                {
                    var newItem = ObjectHelper.GetObject<IWorkflowItem>("Read");
                    newItem.InstanceId = CurrentItem.InstanceId;
                    newItem.ReceiveTime = DateTime.Now;
                    newItem.PartUserId = user.Id;
                    newItem.PartUserName = user.Name;
                    newItem.PartDepartmentId = user.Department.Id;
                    newItem.PartDepepartmentName = user.Department.Name;
                    newItem.Status = WorkItemStatus.Sent;
                    newItem.ItemId = ++taskId;
                    newItem.Alias = "传阅";
                    newItem.CurrentActivity = "传阅";
                    newItem.AppCode = CurrentItem.AppCode;
                    newItem.PreItemId = CurrentItem.ItemId;
                    newItem.PasserUserId = CurrentUser.Id;
                    newItem.PasserUserName = CurrentUser.Name;
                    FlowInstance.InsertWorkItem(newItem);
                }
                tran.Complete();
            }
        }

        public string GetNormalActivityName(string originalName)
        {
            if (originalName.IndexOf('.') > 0)
            {
                return originalName.Substring(originalName.LastIndexOf('.') + 1);
            }
            return originalName;
        }

        #region 私有方法

        private void OpenWorkflow(IWorkflowInstance instance, int itemId)
        {
            if (itemId >= 10000)
            {
                CurrentItem = instance.WorkItemsRead.FirstOrDefault(o => o.ItemId == itemId);
            }
            else
            {
                CurrentItem = instance.WorkItems.FirstOrDefault(o => o.ItemId == itemId);
            }
            if (CurrentItem == null)
            {
                throw new FoxOneException("不存在ItemId为{0}的工作项".FormatTo(itemId));
            }
            FlowInstance = instance;
            CurrentWorkflow = Build(FlowInstance.ApplicationId);
            if (CurrentItem.CurrentActivity != "传阅")
            {
                CurrentActivity = CurrentWorkflow[CurrentItem.CurrentActivity];
                if (CurrentActivity == null)
                {
                    throw new FoxOneException(string.Format("流程定义中不存在步骤名为{0}的步骤", CurrentItem.CurrentActivity));
                }
            }
        }

        private IList<ITransition> GetAvailableTransitions(IWorkflowContext context)
        {
            IList<ITransition> result = new List<ITransition>();
            foreach (var tran in CurrentActivity.Transitions)
            {
                if (tran.PreResolve(context))
                {
                    if (tran.To is ParallelStartActivity || tran.To is ParallelEndActivity)
                    {
                        foreach (var tran1 in tran.To.Transitions)
                        {
                            if (tran1.PreResolve(context))
                            {
                                result.Add(tran1);
                            }
                        }
                    }
                    else
                    {
                        result.Add(tran);
                    }
                }
            }
            return result;
        }
        private IWorkflowInstance GetNewInstance(string appCode, string procName, string dataLocator, int impoLevel, int secret)
        {
            var instance = ObjectHelper.GetObject<IWorkflowInstance>();
            instance.ApplicationId = appCode;
            instance.CreatorId = CurrentUser.Id;
            instance.WorkItemNewSeq = 1;
            instance.WorkItemNewTask = 1;
            instance.StartTime = DateTime.Now;
            instance.FlowTag = FlowStatus.Begin;
            instance.SecretLevel = secret;
            instance.InstanceName = string.IsNullOrEmpty(procName) ? instance.Application.Name : procName;
            instance.ImportantLevel = impoLevel;
            instance.DataLocator = dataLocator;
            return instance;
        }

        private IWorkflowItem GetNewWorkItem(IActivity activity, IUser user, string appCode, string instanceId)
        {
            var newItem = ObjectHelper.GetObject<IWorkflowItem>();
            newItem.AppCode = appCode;
            newItem.Alias = activity.Alias;
            newItem.CurrentActivity = activity.Name;
            newItem.ItemSeq = 1;
            newItem.ItemId = 1;
            newItem.Status = WorkItemStatus.Sent;
            newItem.InstanceId = instanceId;
            newItem.ReceiveTime = DateTime.Now;
            if (activity is ResponseActivity)
            {
                newItem.ExpiredTime = (activity as ResponseActivity).GetExpiredTime();
            }
            newItem.PartUserId = user.Id;
            newItem.PartUserName = user.Name;
            newItem.LevelCode = "00";
            newItem.PartDepartmentId = user.Department.Id;
            newItem.PartDepepartmentName = user.Department.Name;
            return newItem;
        }

        /// <summary>
        /// 检查流程定义是否有效
        /// </summary>
        /// <param name="workflow"></param>
        private void ValidateWorkflow(IWorkflow workflow)
        {
            foreach (var acti in workflow.Activities)
            {
                if (!(acti is EndActivity) && !(acti is BreakdownActivity))
                {
                    if (acti.Transitions == null || acti.Transitions.Count == 0)
                    {
                        throw new FoxOneException(string.Format("流程定义错误 ，步骤【{0}】没有向外的迁移", acti.Name));
                    }
                }
                if (!ExistTranToActi(workflow, acti))
                {
                    throw new FoxOneException(string.Format("流程定义错误，没有指向步骤【{0}】的迁移", acti.Name));
                }
            }
            foreach (var tran in workflow.Transitions)
            {
                if (tran.To == null || tran.From == null)
                {
                    throw new FoxOneException("无效迁移");
                }
            }
        }

        /// <summary>
        /// 是否存在指向某步骤的迁移
        /// </summary>
        /// <param name="workflow"></param>
        /// <param name="acti"></param>
        /// <returns></returns>
        private bool ExistTranToActi(IWorkflow workflow, IActivity acti)
        {
            //如果该步骤是开始步骤，则无指向该步骤的迁移也正常。
            if (acti == workflow.Root) return true;
            foreach (var tran in workflow.Transitions)
            {
                if (tran.To == acti)
                {
                    return true;
                }
            }
            return false;
        }
        private IWorkflowContext GetWorkflowContext()
        {
            var context = ObjectHelper.GetObject<IWorkflowContext>();
            context.LevelCode = CurrentItem.LevelCode;
            context.CurrentUser = CurrentUser;
            context.FlowInstance = FlowInstance;
            context.CurrentTask = CurrentItem;
            context.Parameter = FlowInstance.Parameters;
            return context;
        }
        private void Validate()
        {
            if (CurrentWorkflow == null || CurrentItem == null || FlowInstance == null)
            {
                throw new FoxOneException("未打开流程，或工作项不存在");
            }
        }
        private void SetAutoFinished(string reason)
        {
            if (FlowInstance.FlowTag == FlowStatus.Finished)
            {
                throw new FoxOneException("流程实例已结束，不能执行该操作!");
            }
            var unFinishedItem = FlowInstance.WorkItems.Where(o => o.Status < WorkItemStatus.Finished).ToList();
            foreach (var item in unFinishedItem)
            {
                item.Status = WorkItemStatus.AutoFinished;
                item.AssigneeUserId = CurrentUser.Id;
                item.AssigneeUserName = CurrentUser.Name;
                item.FinishTime = DateTime.Now;
                item.AutoFinish = true;
                item.UserChoice = "自动结束，原因：" + reason;
            }
            FlowInstance.UpdateWorkItem(unFinishedItem);
        }
        #endregion

        public void SetReadTime()
        {
            Validate();
            var task = CurrentItem;
            if (task.Status == WorkItemStatus.Sent)
            {
                task.Status = WorkItemStatus.Readed;
                task.ReadTime = DateTime.Now;
                if (task.ItemId >= 10000)
                {
                    task.FinishTime = DateTime.Now;
                    task.Status = WorkItemStatus.Finished;
                }
                FlowInstance.UpdateWorkItem(task);
            }
        }

        public IList<ToDoList> GetToDoList(string partUserId)
        {
            var items = Dao.Get().Query<WorkflowItem>().Where(o => o.PartUserId == partUserId && o.Status < WorkItemStatus.Finished).ToList().ToList<IWorkflowItem>();
            return GetListInner(partUserId, items);
        }

        private IList<ToDoList> GetListInner(string partUserId, IList<IWorkflowItem> items)
        {
            var result = new List<ToDoList>();
            if (items.IsNullOrEmpty()) return result;
            var itemIds = items.Select(o => o.InstanceId).ToArray();
            var ins = Dao.Get().Query<WorkflowInstance>().Where(o => itemIds.Contains(o.Id)).ToList();
            foreach (var ii in items.Distinct(o => o.InstanceId))
            {
                var item = ins.FirstOrDefault(o => o.Id.Equals(ii.InstanceId, StringComparison.OrdinalIgnoreCase));
                if (ii.PartUserId.Equals(partUserId, StringComparison.OrdinalIgnoreCase))
                {
                    var todo = new ToDoList()
                    {
                        InstanceId = item.Id,
                        InstanceCreateTime = item.StartTime.Value,
                        InstanceCreator = item.Creator.Name,
                        ApplicationId = item.Application.Id,
                        ApplicationName = item.Application.Name,
                        ApplicationType = item.Application.Type,
                        CurrentActivityAlias = ii.Alias,
                        CurrentActivityName = ii.CurrentActivity,
                        ExpiredTime = ii.ExpiredTime,
                        FinishTime = ii.FinishTime,
                        InstanceName = item.InstanceName,
                        InstanceStatus = item.FlowTag.GetDescription(),
                        ItemId = ii.ItemId,
                        ItemStatus = ii.Status.GetDescription(),
                        PartUserId = ii.PartUserId,
                        PartUserName = ii.PartUserName,
                        PasserUserId = ii.PasserUserId,
                        PasserUserName = ii.PasserUserName,
                        ReadTime = ii.ReadTime,
                        ReceiveTime = ii.ReceiveTime
                    };
                    result.Add(todo);
                }
            }
            return result;
        }

        public IList<ToDoList> GetDoneList(string partUserId)
        {
            var items = Dao.Get().Query<WorkflowItem>().Where(o => o.PartUserId == partUserId && o.Status == WorkItemStatus.Finished).ToList().ToList<IWorkflowItem>();
            return GetListInner(partUserId, items);
        }

        public IList<ToDoList> GetReadList(string partUserId)
        {
            var items = Dao.Get().Query<WorkflowItemRead>().Where(o => o.PartUserId == partUserId).ToList().ToList<IWorkflowItem>();
            return GetListInner(partUserId, items);
        }

        public IList<IWorkflowInstance> GetAllInstance()
        {
            return DBContext<IWorkflowInstance>.Instance;
        }

        public IList<IWorkflowApplication> GetAllApplication()
        {
            return DBContext<IWorkflowApplication>.Instance;
        }

        public IList<IWorkflowDefinition> GetAllDefinition()
        {
            return DBContext<IWorkflowDefinition>.Instance;
        }
    }

    public class NextStep
    {
        public string MultipleSelectTag { get; set; }

        public string StepName { get; set; }

        public string Label { get; set; }

        public int Rank { get; set; }

        public string LabelDescription { get; set; }

        public bool NeedUser { get; set; }

        public bool AllowSelect { get; set; }

        public bool AllowFree { get; set; }

        public bool OnlySingleSel { get; set; }

        public bool AutoSelectAll { get; set; }

        public List<NextStepUser> Users { get; set; }

        public string Message { get; set; }
    }

    public class NextStepUser
    {
        public string ID { get; set; }

        public string Name { get; set; }

        public string OrgId { get; set; }

        public string OrgName { get; set; }

        public int Rank { get; set; }

        public int OrgRank { get; set; }

        public string StepName { get; set; }
    }

    public class ToDoList
    {
        public string InstanceId { get; set; }

        public string InstanceName { get; set; }

        public string InstanceCreator { get; set; }

        public DateTime InstanceCreateTime { get; set; }

        public string InstanceStatus { get; set; }

        public string ApplicationId { get; set; }

        public string ApplicationName { get; set; }

        public string ApplicationType { get; set; }

        public int ItemId { get; set; }

        public string ItemStatus { get; set; }

        public string CurrentActivityName { get; set; }

        public string CurrentActivityAlias { get; set; }

        public string PartUserId { get; set; }

        public string PartUserName { get; set; }

        public string PasserUserId { get; set; }

        public string PasserUserName { get; set; }

        public DateTime? ReceiveTime { get; set; }

        public DateTime? ReadTime { get; set; }

        public DateTime? ExpiredTime { get; set; }

        public DateTime? FinishTime { get; set; }
    }
}
