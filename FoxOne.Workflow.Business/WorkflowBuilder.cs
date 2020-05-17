/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/6/17 18:19:25
 * 描述说明：
 * *******************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FoxOne.Workflow.Kernel;
using System.Xml.Serialization;
using System.IO;
using FoxOne.Core;
using FoxOne.Workflow.DataAccess;
using System.Reflection;
using FoxOne.Business;
using System.Web.Script.Serialization;

namespace FoxOne.Workflow.Business
{
    public class WorkflowBuilder : IWorkflowBuilder
    {
        private IList<IControl> Controls
        {
            get;
            set;
        }

        private const string WORKFLOW_CACHE_KEY_FORMAT = "WORKFLOW_DEFINITION_{0}";

        public IWorkflow Build(string workflowId)
        {
            return CacheHelper.GetFromCache<IWorkflow>(WORKFLOW_CACHE_KEY_FORMAT.FormatTo(workflowId), () =>
            {
                var components = DBContext<ComponentEntity>.Instance.Where(o => o.PageId.Equals(workflowId, StringComparison.OrdinalIgnoreCase));
                var workflow = ObjectHelper.GetObject<IWorkflow>();
                workflow.Activities = new List<IActivity>();
                workflow.Transitions = new List<ITransition>();
                Controls = new List<IControl>();
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializer.RegisterConverters(new[] { new ComponentConverter() });
                foreach (var component in components)
                {
                    var type = TypeHelper.GetType(component.Type);
                    if (!component.JsonContent.IsNullOrEmpty())
                    {
                        var instance = serializer.Deserialize(component.JsonContent, type);
                        var control = instance as IControl;
                        control.Id = component.Id;
                        control.ParentId = component.ParentId;
                        control.PageId = component.PageId;
                        control.TargetId = component.TargetId;
                        Controls.Add(control);
                    }
                }
                foreach (var e in Controls)
                {
                    if (e.ParentId.Equals(workflowId))
                    {
                        if (e is IActivity)
                        {
                            var acti = e as IActivity;
                            acti.Name = e.Id;
                            workflow.Activities.Add(acti);
                            if (e is ResponseActivity)
                            {
                                var acti1 = e as ResponseActivity;
                                if (acti1.IsRoot)
                                {
                                    workflow.Root = acti1;
                                }
                                if (acti1.IsFreeApproval)
                                {
                                    var vActi = new BreakdownActivity() { ResponseRuleType = ResponseRuleType.AllResponse, NeedChoice = false, Id = acti1.Name + "_BreakDown",Name = acti1.Name + "_BreakDown", Alias = acti1.Alias + "-分发", Actor = new UserSelectActor() { InnerActor = new ChildDepartmentActor() {DeptName="IT研发中心", IfGetAllChildren = true, IfReturnSelf = true, RoleName = SysConfig.DefaultUserRole } } };
                                    var vTran = new BusinessTransition() { Id = acti1.Name + "_To_" + vActi.Name, Label = "自由分发", Condition = new ChoiceCondition() { Choice = vActi.Name } };
                                    vTran.To = vActi;
                                    vTran.From = acti1;
                                    workflow.Transitions.Add(vTran);
                                    workflow.Activities.Add(vActi);
                                }
                            }
                        }
                        else if (e is ITransition)
                        {
                            var tran = e as BusinessTransition;
                            tran.To = Controls.FirstOrDefault(o => o.Id.Equals(tran.ToId)) as IActivity;
                            tran.From = Controls.FirstOrDefault(o => o.Id.Equals(tran.FromId)) as IActivity;
                            workflow.Transitions.Add(tran);
                        }
                        GetChildren(e);
                    }
                }
                foreach (var activity in workflow.Activities)
                {
                    activity.Owner = workflow;
                    if (activity.Actor != null)
                    {
                        activity.Actor.Owner = activity;
                        if (activity.Actor is UserSelectActor)
                        {
                            (activity.Actor as UserSelectActor).InnerActor.Owner = activity;
                        }
                    }
                }
                foreach (var tran in workflow.Transitions)
                {
                    tran.Owner = workflow;
                    if (tran.Condition != null)
                    {
                        tran.Condition.Owner = tran;
                    }
                    if (tran.From.Transitions == null)
                    {
                        tran.From.Transitions = new List<ITransition>();
                    }
                    tran.From.Transitions.Add(tran);
                }
                return workflow;
            });
        }

        private void GetChildren(IControl e)
        {
            var children = Controls.Where(o => o.ParentId.Equals(e.Id));
            if (children.IsNullOrEmpty()) return;
            var fastType = FastType.Get(e.GetType());
            foreach (var ee in children)
            {
                var gettter = fastType.GetGetter(ee.TargetId);
                if (gettter.Type.IsGenericType)
                {
                    var instance = gettter.GetValue(e);
                    if (instance == null)
                    {
                        var t = typeof(List<>);
                        var type = gettter.Type.GetGenericArguments()[0];
                        t = t.MakeGenericType(type);
                        instance = Activator.CreateInstance(t);
                        gettter.SetValue(e, instance);
                    }
                    var add = instance.GetType().GetMethod("Add");
                    add.Invoke(instance, new object[] { ee });
                }
                else
                {
                    if(gettter.GetValue(e)!=null)
                    {
                        throw new FoxOneException("This Property:{0} of Control:{1} is already Set", ee.TargetId, e.Id);
                    }
                    gettter.SetValue(e, ee);
                }
                GetChildren(ee);
            }
        }

        public bool ClearCache(string workflowId)
        {
            return CacheHelper.Remove(WORKFLOW_CACHE_KEY_FORMAT.FormatTo(workflowId));
        }
    }
}
