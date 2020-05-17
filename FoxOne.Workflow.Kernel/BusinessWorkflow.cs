/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/1/26 14:14:26
 * 描述说明：
 * *******************************************************/
using FoxOne.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;

namespace FoxOne.Workflow.Kernel
{
    public class BusinessWorkflow : IWorkflow
    {
        public virtual IActivity Root
        {
            get;
            set;
        }

        public virtual IList<IActivity> Activities
        {
            get;
            set;
        }

        public virtual IList<ITransition> Transitions
        {
            get;
            set;
        }

        public virtual IActivity this[string ActivityName]
        {
            get
            {
                foreach (var activity in Activities)
                {
                    if (activity.Name.Equals(ActivityName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return activity;
                    }
                }
                return null;
            }
        }

        public virtual string Name
        {
            get;
            set;
        }

        public virtual void AddInstance(IWorkflowInstance instance)
        {
            EntityEventManager.RaiseEvent<IWorkflowInstance>(EventStep.Before, EventType.Insert, instance);
            InstanceService.Insert(instance);
            EntityEventManager.RaiseEvent<IWorkflowInstance>(EventStep.After, EventType.Insert, instance);
            WorkflowInstanceLog(instance, "新增");
        }

        private void WorkflowInstanceLog(IWorkflowInstance instance, string type)
        {
            Logger.GetLogger("Workflow").Info(string.Format("{0}:{1}流程实例：procName:{2} - creatorName:{3} - datalocator:{4} - flowtag:{5}",
                instance.Id,
                type,
                instance.InstanceName,
                instance.Creator.Name,
                instance.DataLocator,
                instance.FlowTag));
        }

        public virtual void UpdateInstance(IWorkflowInstance instance)
        {
            EntityEventManager.RaiseEvent<IWorkflowInstance>(EventStep.Before, EventType.Update, instance);
            InstanceService.Update(instance);
            EntityEventManager.RaiseEvent<IWorkflowInstance>(EventStep.After, EventType.Update, instance);
            WorkflowInstanceLog(instance, "更新");
        }

        public virtual void DeleteInstance(IWorkflowInstance instance)
        {
            EntityEventManager.RaiseEvent<IWorkflowInstance>(EventStep.Before, EventType.Delete, instance);
            InstanceService.Delete(instance);
            EntityEventManager.RaiseEvent<IWorkflowInstance>(EventStep.After, EventType.Delete, instance);
            WorkflowInstanceLog(instance, "删除");
        }

        private bool BoolExecute(string stage, Func<IWorkflowContext, bool> func, IWorkflowContext context, IActivity activity)
        {
            var result = func(context);
            Logger.GetLogger("Workflow").InfoFormat("{0}:执行步骤：{1} 的{2}方法，返回结果为：{3}", context.FlowInstance.Id, activity.Name, stage, result);
            return result;
        }

        private void VoidExecute(ActivityStep step, Action<IWorkflowContext> action, IWorkflowContext context, IActivity activity)
        {
            WorkflowEventManager.RaiseEvent(EventStep.Before, step, context, activity);
            action(context);
            WorkflowEventManager.RaiseEvent(EventStep.After, step, context, activity);
        }

        protected virtual void InnerRun(IActivity activity, IWorkflowContext context)
        {
            string procID = context.FlowInstance.Id;
            if (!BoolExecute("CanExecute", activity.CanExecute, context, activity)) return;
            VoidExecute(ActivityStep.Execute, activity.Execute, context, activity);
            if (!BoolExecute("CanExit", activity.CanExit, context, activity)) return;
            VoidExecute(ActivityStep.Exit, activity.Exit, context, activity);
            if (activity.Transitions.IsNullOrEmpty()) return;
            foreach (var tran in activity.Transitions)
            {
                if (tran.Resolve(context))
                {
                    if (BoolExecute("CanEnter", tran.To.CanEnter, context, tran.To))
                    {
                        VoidExecute(ActivityStep.Enter, tran.To.Enter, context, tran.To);
                        if (tran.To.AutoRun)
                        {
                            InnerRun(tran.To, context);
                        }
                    }
                }
            }
        }

        public virtual void Run(IWorkflowContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("流程运行需要上下文");
            }
            if (context.FlowInstance.FlowTag > FlowStatus.Finished)
            {
                throw new Exception(string.Format("目前流程处于{0}状态，无法继续流转。", context.FlowInstance.FlowTag.ToString()));
            }
            if (context.CurrentTask.Status >= WorkItemStatus.Finished)
            {
                throw new Exception("当前工作项状态不允许运行");
            }
            using (TransactionScope tran = new TransactionScope())
            {
                if (context.FlowInstance.FlowTag == FlowStatus.Begin)
                {
                    context.FlowInstance.FlowTag = FlowStatus.Running;
                    UpdateInstance(context.FlowInstance);
                }
                InnerRun(this[context.CurrentTask.CurrentActivity], context);
                tran.Complete();
            }
        }

        private IWorkflowInstanceService instanceService;
        public IWorkflowInstanceService InstanceService
        {
            get
            {
                return instanceService ?? (instanceService = ObjectHelper.GetObject<IWorkflowInstanceService>());
            }
        }
    }

    public enum ActivityStep
    {
        Enter,
        Execute,
        Exit
    }

    public enum ItemActionType
    {
        Insert,
        Update,
        Delete
    }

    public static class WorkflowEventManager
    {
        private static IDictionary<string, Func<IWorkflowContext, IActivity, bool>> EntityEventList = new Dictionary<string, Func<IWorkflowContext, IActivity, bool>>();
        private static IDictionary<string, Action<IWorkflowInstance, IWorkflowItem>> WorkItemEventList = new Dictionary<string, Action<IWorkflowInstance, IWorkflowItem>>();
        private const string KeyTemplate = "{0}_{1}";
        public static bool RaiseEvent(EventStep step, ActivityStep type, IWorkflowContext context, IActivity activity)
        {
            string key = KeyTemplate.FormatTo(step.ToString(), type.ToString());
            if (EntityEventList.ContainsKey(key))
            {
                Logger.GetLogger("Workflow").Info("{0}:Raise WorkflowEvent:{1}".FormatTo(context.FlowInstance.Id, key));
                try
                {
                    return EntityEventList[key](context, activity);
                }
                catch (Exception ex)
                {
                    Logger.GetLogger("Workflow").Info("{0}:Raise WorkflowEvent:{1} Error:{2},{3}".FormatTo(context.FlowInstance.Id, key, ex.Message, ex.StackTrace));
                }

            }
            return true;
        }

        public static void RegisterEvent(string appCode, EventStep step, ActivityStep type, Func<IWorkflowContext, IActivity, bool> predicate)
        {
            if (predicate != null)
            {
                string key = KeyTemplate.FormatTo(step.ToString(), type.ToString());
                EntityEventList.Add(key, predicate);
            }
        }

        public static void RaiseWorkItemEvent(EventStep step, ItemActionType type, IWorkflowInstance instance, IWorkflowItem item)
        {
            string key = KeyTemplate.FormatTo(step.ToString(), type.ToString());
            if (WorkItemEventList.ContainsKey(key))
            {
                try
                {
                    Logger.GetLogger("Workflow").Info("{0}:Raise WorkflowItemEvent:{1}".FormatTo(instance.Id, key));
                    WorkItemEventList[key](instance, item);
                }
                catch (Exception ex)
                {
                    Logger.GetLogger("Workflow").Info("{0}:Raise WorkflowEvent:{1} Error:{2},{3}".FormatTo(instance.Id, key, ex.Message, ex.StackTrace));
                }
            }
        }

        public static void RegisterWorkItemEvent(EventStep step, ItemActionType type, Action<IWorkflowInstance, IWorkflowItem> predicate)
        {
            if (predicate != null)
            {
                string key = KeyTemplate.FormatTo(step.ToString(), type.ToString());
                WorkItemEventList.Add(key, predicate);
            }
        }
    }
}
