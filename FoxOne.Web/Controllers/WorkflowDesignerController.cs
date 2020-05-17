using FoxOne.Business;
using FoxOne.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FoxOne.Core;
using System.Web.Script.Serialization;
using FoxOne.Workflow.Kernel;
using FoxOne.Workflow.Business;
namespace FoxOne.Web.Controllers
{
    public class WorkflowDesignerController : BaseController
    {
        private const string TRAN_ID_FORMAT = "{0}_To_{1}";

        private const string TRAN_ID_START = "{0}_To_";

        private const string TRAN_ID_END = "_To_{0}";
        public ActionResult Index(string id)
        {
            var definition = DBContext<IWorkflowDefinition>.Instance.FirstOrDefault(o => o.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (definition == null)
            {
                throw new FoxOneException("Page_Not_Found");
            }
            WorkflowHelper.ClearCache(id);
            return View(definition);
        }

        public ActionResult FlowImage(string id)
        {
            ViewData["DefinitionId"] = id;
            return View();
        }

        public JsonResult Get(string id)
        {
            var components = DBContext<ComponentEntity>.Instance.Where(o => o.PageId.Equals(id, StringComparison.OrdinalIgnoreCase));
            var result = new WorkflowDesignModel();

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.RegisterConverters(new[] { new ComponentConverter() });

            foreach (var c in components)
            {
                var type = TypeHelper.GetType(c.Type);
                var instance = serializer.Deserialize(c.JsonContent, type);
                if (instance is BaseActivity)
                {
                    var activity = instance as BaseActivity;
                    result.Activities.Add(new ActivityModel() { Id = activity.Id, Type = c.Type, Name = activity.Name, Alias = activity.Alias, Top = activity.Top, Left = activity.Left });
                }
                else if (instance is BusinessTransition)
                {
                    var transition = instance as BusinessTransition;
                    result.Transitions.Add(new TransitionModel() { To = transition.ToId, From = transition.FromId });
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AddTran(string To, string From, string DefinitionId, string ToName)
        {
            if (To.IsNullOrEmpty() || From.IsNullOrEmpty() || DefinitionId.IsNullOrEmpty())
            {
                return Json(false);
            }
            var toActivity = DBContext<ComponentEntity>.Instance.FirstOrDefault(o => o.PageId.Equals(DefinitionId, StringComparison.OrdinalIgnoreCase) && o.Id.Equals(To, StringComparison.OrdinalIgnoreCase));
            if (toActivity == null)
            {
                return Json(false);
            }
            var fromActivity = DBContext<ComponentEntity>.Instance.FirstOrDefault(o => o.PageId.Equals(DefinitionId, StringComparison.OrdinalIgnoreCase) && o.Id.Equals(From, StringComparison.OrdinalIgnoreCase));
            if (fromActivity == null)
            {
                return Json(false);
            }
            string id = TRAN_ID_FORMAT.FormatTo(From, To);
            if (DBContext<ComponentEntity>.Instance.Count(o => o.PageId.Equals(DefinitionId, StringComparison.OrdinalIgnoreCase) && o.Id.Equals(id, StringComparison.OrdinalIgnoreCase)) > 0)
            {
                return Json(false);
            }
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.RegisterConverters(new[] { new ComponentConverter() });
            var instance = new BusinessTransition() { PageId = DefinitionId, Id = id, ParentId = DefinitionId, TargetId = "Transition", ToId = To, FromId = From, Label = ToName, CanPushBack = true, NotOperation = false };
            instance.Condition = new ChoiceCondition() { Choice = To };
            ComponentHelper.RecSave(instance);
            return Json(true);
        }

        private string GetDefinitionNewId(string definitionId)
        {
            var count = DBContext<ComponentEntity>.Instance.Count(o => o.PageId.Equals(definitionId, StringComparison.OrdinalIgnoreCase) && o.TargetId.Equals("Activity")) + 1;
            while (DBContext<ComponentEntity>.Instance.Count(o => o.PageId.Equals(definitionId, StringComparison.OrdinalIgnoreCase) && o.Id.Equals("Activity" + count)) > 0)
            {
                count++;
            }
            return "Activity" + count;
        }

        public JsonResult AddActivity(string actiType, int left, int top, string definitionId)
        {
            if (!actiType.IsNullOrEmpty())
            {
                var type = TypeHelper.GetType(actiType);
                if (type != null)
                {
                    var instance = Activator.CreateInstance(type) as BaseActivity;
                    if (instance != null)
                    {
                        instance.PageId = definitionId;
                        instance.TargetId = "Activity";
                        instance.ParentId = definitionId;
                        instance.Id = GetDefinitionNewId(definitionId);
                        instance.Top = top;
                        instance.Left = left;
                        instance.Name = instance.Id;
                        instance.Alias = type.GetDisplayName();
                        instance.NeedChoice = true;
                        if (instance.Id.EndsWith("1"))
                        {
                            (instance as ResponseActivity).IsRoot = true;
                        }
                        instance.Actor = new UserSelectActor() { InnerActor = new RoleActor() { } };
                        ComponentHelper.RecSave(instance);
                        return Json(new { Id = instance.Id, Alias = instance.Alias }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            throw new FoxOneException("Add_Activity_Fail");
        }

        public JsonResult UpdateWidthHeight(int width, int height, string definitionId)
        {
            var definition = DBContext<IWorkflowDefinition>.Instance.FirstOrDefault(o => o.Id.Equals(definitionId, StringComparison.OrdinalIgnoreCase));
            definition.Width = width;
            definition.Height = height;
            return Json(DBContext<IWorkflowDefinition>.Update(definition));
        }

        public JsonResult BatchAddActivity(string activitieNames, string definitionId)
        {
            bool result = false;
            var activities = activitieNames.Split(',');
            int top = 100, left = 100;
            foreach (var acti in activities)
            {
                var instance = new ResponseActivity();
                instance.PageId = definitionId;
                instance.TargetId = "Activity";
                instance.ParentId = definitionId;
                instance.Id = GetDefinitionNewId(definitionId);
                instance.Top = top;
                instance.Left = left;
                instance.Name = instance.Id;
                instance.Alias = typeof(ResponseActivity).GetDisplayName();
                instance.NeedChoice = true;
                if (instance.Id.EndsWith("1"))
                {
                    (instance as ResponseActivity).IsRoot = true;
                }
                instance.Actor = new FlowActor() { FlowUser = FlowActorType.Creator };
                ComponentHelper.RecSave(instance);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteActivity(string id, string definitionId)
        {
            ComponentHelper.DeleteComponent(definitionId, id);
            var components = DBContext<ComponentEntity>.Instance.Where(o => o.PageId.Equals(definitionId, StringComparison.OrdinalIgnoreCase) && (o.Id.StartsWith(TRAN_ID_START.FormatTo(id), StringComparison.OrdinalIgnoreCase) || o.Id.EndsWith(TRAN_ID_END.FormatTo(id), StringComparison.OrdinalIgnoreCase)));
            foreach (var com in components)
            {
                ComponentHelper.DeleteComponent(com.PageId, com.Id);
            }
            return Json(true);
        }

        public JsonResult DeleteTran(string To, string From, string DefinitionId)
        {
            if (To.IsNullOrEmpty() || From.IsNullOrEmpty() || DefinitionId.IsNullOrEmpty())
            {
                return Json(false);
            }
            string id = TRAN_ID_FORMAT.FormatTo(From, To);
            ComponentHelper.DeleteComponent(DefinitionId, id);
            return Json(true);
        }

        public JsonResult ChangePosition(string id, string definitionId, int left, int top)
        {
            bool result = false;
            var c = DBContext<ComponentEntity>.Instance.FirstOrDefault(o => o.PageId.Equals(definitionId, StringComparison.OrdinalIgnoreCase) && o.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (c != null)
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializer.RegisterConverters(new[] { new ComponentConverter() });
                var type = TypeHelper.GetType(c.Type);
                var instance = serializer.Deserialize(c.JsonContent, type) as BaseActivity;
                if (instance != null)
                {
                    instance.Left = left;
                    instance.Top = top;
                    string content = serializer.Serialize(instance);
                    c.JsonContent = content;
                    result = DBContext<ComponentEntity>.Update(c);
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }

    public class WorkflowDesignModel
    {
        public WorkflowDesignModel()
        {
            Activities = new List<ActivityModel>();
            Transitions = new List<TransitionModel>();
        }

        public IList<ActivityModel> Activities { get; set; }

        public IList<TransitionModel> Transitions { get; set; }


    }

    public class ActivityModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public string Alias { get; set; }

        public int Left { get; set; }

        public int Top { get; set; }


    }

    public class TransitionModel
    {
        public string To { get; set; }

        public string From { get; set; }
    }
}
