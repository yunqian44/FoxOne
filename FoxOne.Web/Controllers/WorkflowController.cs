using FoxOne.Business;
using FoxOne.Business.Environment;
using FoxOne.Business.Security;
using FoxOne.Controls;
using FoxOne.Core;
using FoxOne.Data;
using FoxOne.Workflow.Business;
using FoxOne.Workflow.Kernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace FoxOne.Web.Controllers
{
    public class WorkflowController : BaseController
    {

        private const string APP_ID = "ApplicationId";
        private const string INST_ID = "InstanceId";
        private const string TASK_ID = "ItemId";
        private const string DATA_LOCATOR = "DataLocator";

        public ActionResult Index()
        {
            Form form = null;
            var appCode = Request.QueryString[APP_ID];
            string title = string.Empty;
            Page page = null;
            string formType = string.Empty;
            var relatePages = new List<PageRelateEntity>();
            IWorkflowApplication app = null;
            bool canDelete = false;
            if (appCode.IsNotNullOrEmpty())
            {
                app = DBContext<IWorkflowApplication>.Instance.Get(appCode);
                formType = app.FormType;
                page = PageBuilder.BuildPage(app.FormType);
                form = page.Controls.FirstOrDefault(o => o is Form) as Form;
                form.FormMode = FormMode.Insert;
                title = app.Name;
            }
            else
            {
                var instanceId = Request.QueryString[INST_ID];
                int itemId = Request.QueryString[TASK_ID].ConvertTo<int>();
                var helper = new WorkflowHelper(Sec.User);
                helper.OpenWorkflow(instanceId, itemId);
                
                if (helper.CurrentItem.Status >= WorkItemStatus.Finished)
                {
                    throw new FoxOneException("当前工作项已完成！");
                }
                if (!helper.CurrentItem.PartUserId.Equals(Sec.User.Id, StringComparison.OrdinalIgnoreCase) && !Sec.IsSuperAdmin)
                {
                    throw new FoxOneException("您没有处理该审批步骤的权限！");
                }
                canDelete = helper.CurrentItem.PartUserId.Equals(helper.FlowInstance.CreatorId, StringComparison.OrdinalIgnoreCase);
                helper.SetReadTime();
                formType = helper.FlowInstance.Application.FormType;
                app = helper.FlowInstance.Application;
                page = PageBuilder.BuildPage(formType);
                form = page.Controls.FirstOrDefault(o => o is Form) as Form;
                form.Key = helper.FlowInstance.DataLocator;
                InsertTable(helper);
                title = string.Format("{0} - {1}", helper.FlowInstance.InstanceName, helper.CurrentItem.Alias);
                var tempRelate = DBContext<PageRelateEntity>.Instance.Where(o => o.PageId.Equals(formType, StringComparison.OrdinalIgnoreCase));
                if (!tempRelate.IsNullOrEmpty())
                {
                    foreach (var item in tempRelate)
                    {
                        relatePages.Add(new PageRelateEntity()
                        {
                            Id = item.Id,
                            PageId = item.PageId,
                            RelateUrl = Env.Parse(item.RelateUrl).Replace("KEY", form.Key),
                            RentId = item.RentId,
                            TabName = item.TabName,
                            TabRank = item.TabRank
                        });
                    }
                }
                if (app.NeedAttachement)
                {
                    relatePages.Add(new PageRelateEntity()
                    {
                        Id = "AttachmentInfo",
                        RelateUrl = "/Attachment/Index/" + form.Key,
                        TabName = "附件信息",
                        TabRank = 1000
                    });
                }
            }
            form.AppendQueryString = true;
            form.PostUrl = "/Workflow/Save";
            ViewData["Tab"] = relatePages;
            ViewData["Form"] = page.Children.OrderBy(o => o.Rank).ToList();
            ViewData["Title"] = title;
            ViewData["DefinitionId"] = app.WorkflowId;
            ViewData["CanDelete"] = canDelete;
            return View();
        }

        public ActionResult Detail()
        {
            var relatePages = new List<PageRelateEntity>();
            var dataLocator = Request.QueryString[DATA_LOCATOR];
            WorkflowHelper helper = new WorkflowHelper(Sec.User);
            if (dataLocator.IsNotNullOrEmpty())
            {
                helper.OpenWorkflow(dataLocator);
            }
            else
            {
                var instanceId = Request.QueryString[INST_ID];
                int itemId = Request.QueryString[TASK_ID].ConvertTo<int>();
                helper.OpenWorkflow(instanceId, itemId);
            }
            helper.SetReadTime();
            var formType = helper.FlowInstance.Application.FormType;
            var app = helper.FlowInstance.Application;
            var page = PageBuilder.BuildPage(formType);
            var form = page.Controls.FirstOrDefault(o => o is Form) as Form;
            form.Key = helper.FlowInstance.DataLocator;
            form.FormMode = FormMode.View;
            InsertTable(helper);
            ViewData["Title"] = string.Format("{0} - {1}", helper.FlowInstance.InstanceName, helper.CurrentItem.Alias);
            var tempRelate = DBContext<PageRelateEntity>.Instance.Where(o => o.PageId.Equals(formType, StringComparison.OrdinalIgnoreCase));
            if (!tempRelate.IsNullOrEmpty())
            {
                foreach (var item in tempRelate)
                {
                    relatePages.Add(new PageRelateEntity()
                    {
                        Id = item.Id,
                        PageId = item.PageId,
                        RelateUrl = Env.Parse(item.RelateUrl).Replace("KEY", form.Key),
                        RentId = item.RentId,
                        TabName = item.TabName,
                        TabRank = item.TabRank
                    });
                }
            }
            if (app.NeedAttachement)
            {
                relatePages.Add(new PageRelateEntity()
                {
                    Id = "AttachmentInfo",
                    RelateUrl = "/Attachment/Index/" + form.Key,
                    TabName = "附件信息",
                    TabRank = 1000
                });
            }
            form.AppendQueryString = true;
            form.PostUrl = "/Workflow/Save";
            ViewData["Tab"] = relatePages;
            ViewData["Form"] = page.Children.OrderBy(o => o.Rank).ToList();
            ViewData["DefinitionId"] = app.WorkflowId;
            return View();
        }

        public ActionResult AutoRun(string id)
        {
            WorkflowHelper helper = new WorkflowHelper(Sec.User);
            helper.OpenWorkflow(id, 1);
            var unDoItem = helper.FlowInstance.WorkItems.Where(o => o.Status < WorkItemStatus.Finished);
            if (!unDoItem.IsNullOrEmpty())
            {
                var newItem = unDoItem.OrderBy(o => o.ItemId).First();
                var loginId = DBContext<IUser>.Instance.Get(newItem.PartUserId).LoginId;
                FormsAuthentication.SignOut();
                FormsAuthentication.SetAuthCookie(loginId, false);
                return Redirect("/Workflow/Index?InstanceId={0}&ItemId={1}&IsSimulate=1".FormatTo(newItem.InstanceId, newItem.ItemId));
            }
            throw new FoxOneException("流程已结束");
        }

        private void InsertTable(WorkflowHelper helper)
        {
            var workItemTable = new Table();
            workItemTable.AutoGenerateColum = false;
            workItemTable.AllowPaging = false;
            workItemTable.Columns.Add(new TableColumn() { ColumnName = "序号", FieldName = "ItemId", TextAlign = CellTextAlign.Center });
            workItemTable.Columns.Add(new TableColumn() { ColumnName = "步骤名", FieldName = "Alias", TextAlign = CellTextAlign.Center });
            workItemTable.Columns.Add(new TableColumn() { ColumnName = "办理人", FieldName = "DisplayPartName", TextAlign = CellTextAlign.Center });
            workItemTable.Columns.Add(new TableColumn() { ColumnName = "接收时间", FieldName = "ReceiveTime", TextAlign = CellTextAlign.Center });
            workItemTable.Columns.Add(new TableColumn() { ColumnName = "完成时间", FieldName = "FinishTime", TextAlign = CellTextAlign.Center });
            workItemTable.Columns.Add(new TableColumn() { ColumnName = "意见", FieldName = "DisplayPartComment", TextAlign = CellTextAlign.Center });
            workItemTable.Columns.Add(new TableColumn() { ColumnName = "状态", FieldName = "StatusText", TextAlign = CellTextAlign.Center });
            workItemTable.DataSource = new InnerTableDs(helper);
            ViewData["Table"] = workItemTable;

            var workItemTable1 = new Table();
            workItemTable1.AutoGenerateColum = false;
            workItemTable1.AllowPaging = false;
            workItemTable1.Columns.Add(new TableColumn() { ColumnName = "序号", FieldName = "ItemId", TextAlign = CellTextAlign.Center });
            workItemTable1.Columns.Add(new TableColumn() { ColumnName = "步骤名", FieldName = "Alias", TextAlign = CellTextAlign.Center });
            workItemTable1.Columns.Add(new TableColumn() { ColumnName = "知会人", FieldName = "PasserUserName", TextAlign = CellTextAlign.Center });
            workItemTable1.Columns.Add(new TableColumn() { ColumnName = "被知会人", FieldName = "PartUserName", TextAlign = CellTextAlign.Center });
            workItemTable1.Columns.Add(new TableColumn() { ColumnName = "知会时间", FieldName = "ReceiveTime", TextAlign = CellTextAlign.Center });
            workItemTable1.Columns.Add(new TableColumn() { ColumnName = "阅读时间", FieldName = "ReadTime", TextAlign = CellTextAlign.Center });
            workItemTable1.Columns.Add(new TableColumn() { ColumnName = "状态", FieldName = "StatusText", TextAlign = CellTextAlign.Center });
            workItemTable1.DataSource = new NoticeTableDs(helper);
            ViewData["NoticeTable"] = workItemTable1;
        }

        public class InnerTableDs : ListDataSourceBase
        {
            public InnerTableDs(WorkflowHelper helper)
            {
                _helper = helper;
            }

            private WorkflowHelper _helper;

            protected override IEnumerable<IDictionary<string, object>> GetListInner()
            {
                return _helper.FlowInstance.WorkItems.Where(o => o.Status <= WorkItemStatus.AutoFinished).OrderBy(o => o.ItemId).ToDictionary();
            }
        }

        public class NoticeTableDs : ListDataSourceBase
        {
            public NoticeTableDs(WorkflowHelper helper)
            {
                _helper = helper;
            }

            private WorkflowHelper _helper;

            protected override IEnumerable<IDictionary<string, object>> GetListInner()
            {
                return _helper.FlowInstance.WorkItemsRead.OrderBy(o => o.ItemId).ToDictionary();
            }
        }

        [ValidateInput(false)]
        public JsonResult Save()
        {
            IDictionary<string, object> data = Request.Form.ToDictionary();
            string key = string.Empty;
            string pageId = Request.QueryString[NamingCenter.PARAM_PAGE_ID];
            string ctrlId = Request.QueryString[NamingCenter.PARAM_CTRL_ID];
            string formViewMode = Request.Form[NamingCenter.PARAM_FORM_VIEW_MODE];
            var page = PageBuilder.BuildPage(pageId);
            if (page == null)
            {
                throw new FoxOneException("Page_Not_Found");
            }
            var form = page.FindControl(ctrlId) as Form;
            if (form == null)
            {
                throw new FoxOneException("Ctrl_Not_Found");
            }
            var ds = form.FormService as IFormService;
            int effectCount = 0;
            WorkflowHelper helper = new WorkflowHelper(Sec.User);
            if (formViewMode.Equals(FormMode.Edit.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                string instanceId = Request.Form[INST_ID];
                int itemId = Request.Form[TASK_ID].ConvertTo<int>();
                helper.OpenWorkflow(instanceId, itemId);
                key = helper.FlowInstance.DataLocator;
                effectCount = ds.Update(key, data);
                helper.UpdateInstance(Env.Parse(helper.FlowInstance.Application.InstanceTitleTemplate), key, 0, 0);
                return Json(new { Insert = false, InstanceId = instanceId, ItemId = itemId, DataLocator = key, ApplicationId = helper.FlowInstance.ApplicationId });
            }
            else
            {
                string applicationId = Request.Form[APP_ID];
                key = Guid.NewGuid().ToString();
                data["Id"] = key;
                effectCount = ds.Insert(data);
                IWorkflowApplication app = DBContext<IWorkflowApplication>.Instance.Get(applicationId);
                helper.StartWorkflow(applicationId, Env.Parse(app.InstanceTitleTemplate), key, 0, 0);
                return Json(new { Insert = true, InstanceId = helper.FlowInstance.Id, ItemId = 1, DataLocator = key, ApplicationId = applicationId });
            }
        }

        public JsonResult GetToDoList()
        {
            WorkflowHelper helper = new WorkflowHelper(Sec.User);
            return Json(helper.GetToDoList(Sec.User.Id), JsonRequestBehavior.AllowGet);
        }

        public JsonResult Start(WorkflowStartParameter startParameter)
        {
            WorkflowHelper helper = new WorkflowHelper(Sec.User);
            helper.StartWorkflow(startParameter.AppCode, startParameter.InstanceName, startParameter.DataLocator, startParameter.ImportLevel, startParameter.SecurityLevel);
            return Json(helper.FlowInstance.Id);
        }

        public JsonResult Delete(string id)
        {
            if (id.IsNotNullOrEmpty() && Sec.IsSuperAdmin)
            {
                WorkflowHelper helper = new WorkflowHelper(Sec.User);
                helper.OpenWorkflow(id);
                DeleteWorkflow(helper);
                return Json(true);
            }
            return Json(false);
        }

        private void DeleteWorkflow(WorkflowHelper helper)
        {
            using (TransactionScope tran = new TransactionScope())
            {
                var service = GetForm(helper);
                service.Delete(helper.FlowInstance.DataLocator);
                Dao.Get().Delete<AttachmentEntity>().Where(o => o.RelateId == helper.FlowInstance.DataLocator).Execute();
                helper.DeleteWorkflow();
                tran.Complete();
            }
        }

        public IFormService GetForm(WorkflowHelper helper)
        {
            var formType = helper.FlowInstance.Application.FormType;
            var page = PageBuilder.BuildPage(formType);
            var form = page.Controls.FirstOrDefault(o => o is Form) as Form;
            form.Key = helper.FlowInstance.DataLocator;
            var parameter = form.FormService.Get(form.Key);
            var result = form.FormService;

            //var parameter = result.SetParameter();默认把表单输入域的所有值都当成是流程参数
            if (!parameter.IsNullOrEmpty())
            {
                foreach (var item in parameter)
                {
                    helper.SetParameter(item.Key, item.Value.ToString());
                }
            }

            return result;
        }

        public JsonResult GetNextStep(WorkflowParameter runParameter)
        {
            List<NextStep> trans = new List<NextStep>();
            WorkflowHelper helper = new WorkflowHelper(Sec.User);
            helper.OpenWorkflow(runParameter.InstanceId, runParameter.ItemId);
            var form = GetForm(helper) as IFlowFormService;
            if (form == null || form.CanRunFlow())
            {
                if (!helper.ShowUserSelect())
                {
                    if (!string.IsNullOrEmpty(runParameter.OpinionContent))
                    {
                        helper.SetOpinion(runParameter.OpinionContent, runParameter.OpinionArea);
                    }
                    helper.Run();
                    trans.Add(new NextStep() { StepName = "自动发送", Label = "自动发送" });
                }
                else
                {
                    trans = helper.GetNextStep();
                    /*if (trans.Count == 1)
                    {
                        var step = trans[0];
                        if (step.Users.Count == 1 || step.NeedUser == false)
                        {
                            if (!string.IsNullOrEmpty(runParameter.OpinionContent))
                            {
                                helper.SetOpinion(runParameter.OpinionContent, runParameter.OpinionArea);
                            }
                            if (step.Users == null || step.Users.Count == 0)
                            {
                                helper.Run(helper.GetUserChoice("NULL_NULL_" + step.StepName));
                            }
                            else
                            {
                                helper.Run(helper.GetUserChoice(step.StepName, step.Users[0]));
                            }
                            trans[0].StepName = "自动发送";
                        }
                    }*/
                }
                if (trans.Count == 0)
                {
                    throw new FoxOneException("当前步骤无下一步可用迁移，请检查流程图及表单参数的设置");
                }
            }
            foreach (var t in trans)
            {
                if (!t.Users.IsNullOrEmpty())
                {
                    t.Users = t.Users.OrderBy(o => o.OrgRank).OrderBy(o => o.Rank).ToList();
                }
            }
            return Json(trans);
        }

        public JsonResult CC()
        {
            WorkflowHelper helper = new WorkflowHelper(Sec.User);
            var instanceId = Request[INST_ID];
            string partUserIds = Request["UserIds"];
            helper.OpenWorkflow(instanceId, 1);
            helper.SendToOtherToRead(partUserIds);
            return Json(true);
        }


        public JsonResult ExecCommand(WorkflowRunParameter runParameter)
        {
            WorkflowHelper helper = new WorkflowHelper(Sec.User);
            helper.OpenWorkflow(runParameter.InstanceId, runParameter.ItemId);
            if (!string.IsNullOrEmpty(runParameter.OpinionContent))
            {
                helper.SetOpinion(HttpUtility.HtmlEncode(runParameter.OpinionContent), runParameter.OpinionArea);
            }
            bool result = false;
            switch (runParameter.Command.ToLower())
            {
                case "run":
                    var form = GetForm(helper) as IFlowFormService;
                    if (form == null || form.CanRunFlow())
                    {
                        helper.Run(runParameter.RunContext);
                        result = true;
                    }
                    break;
                case "rollback":
                    result = helper.Rollback();
                    break;
                case "pushback":
                    result = helper.Pushback();
                    break;
                case "backtoroot":
                    result = helper.PushbackToRoot();
                    break;
                case "forceend":
                    result = helper.ForceToEnd();
                    break;
                case "switch":
                    helper.Switch(runParameter.RunContext.First());
                    result = true;
                    break;
                case "delete":
                    DeleteWorkflow(helper);
                    result = true;
                    break;
            }
            return Json(result);
        }
    }

    [ModelBinder(typeof(WorkflowRunParameterBinder))]
    public class WorkflowRunParameter : WorkflowParameter
    {
        public IList<WorkflowRunChoice> UserChoice { get; set; }

        public IList<IWorkflowChoice> RunContext
        {
            get
            {
                var result = new List<IWorkflowChoice>();
                if (!UserChoice.IsNullOrEmpty())
                {
                    UserChoice.ForEach(o =>
                    {
                        var item = result.FirstOrDefault(r => r.Choice.Equals(o.StepName, StringComparison.OrdinalIgnoreCase));
                        if (item == null)
                        {
                            item = ObjectHelper.GetObject<IWorkflowChoice>();
                            item.Choice = o.StepName;
                            result.Add(item);
                        }
                        if (item.Participant == null)
                        {
                            item.Participant = new List<IUser>();
                        }
                        if (o.Id.IsNotNullOrEmpty())
                        {
                            var user = DBContext<IUser>.Instance.FirstOrDefault(u => u.Id.Equals(o.Id, StringComparison.OrdinalIgnoreCase));
                            user.DepartmentId = o.DepartmentId;
                            item.Participant.Add(user);
                        }
                    });
                }
                return result;
            }
        }
    }

    public class WorkflowRunChoice
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string DepartmentId { get; set; }

        public string StepName { get; set; }
    }

    public class WorkflowStartParameter
    {
        public string AppCode { get; set; }

        public string InstanceName { get; set; }

        public string DataLocator { get; set; }

        public int ImportLevel { get; set; }

        public int SecurityLevel { get; set; }
    }
    public class WorkflowParameter
    {
        public string Command { get; set; }

        public string InstanceId { get; set; }

        public int ItemId { get; set; }

        public string OpinionContent { get; set; }

        public int OpinionArea { get; set; }
    }

    public class WorkflowRunParameterBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            return controllerContext.HttpContext.Request.Form.ToEntity<WorkflowRunParameter>();
        }
    }



    [DisplayName("工作流待办数据源")]
    public class WorkflowToDoListDataSource : ListDataSourceBase
    {
        public override IEnumerable<IDictionary<string, object>> GetList()
        {
            WorkflowHelper helper = new WorkflowHelper(Sec.User);
            return helper.GetToDoList(Sec.User.Id).OrderByDescending(o => o.InstanceCreateTime).ToDictionary();
        }
    }

    [DisplayName("工作流已办数据源")]
    public class WorkflowDoneListDataSource : ListDataSourceBase
    {
        public override IEnumerable<IDictionary<string, object>> GetList()
        {
            WorkflowHelper helper = new WorkflowHelper(Sec.User);
            return helper.GetDoneList(Sec.User.Id).OrderByDescending(o => o.InstanceCreateTime).ToDictionary();
        }
    }

    [DisplayName("工作流知会数据源")]
    public class WorkflowReadListDataSource : ListDataSourceBase
    {
        public override IEnumerable<IDictionary<string, object>> GetList()
        {
            WorkflowHelper helper = new WorkflowHelper(Sec.User);
            return helper.GetReadList(Sec.User.Id).ToDictionary();
        }
    }

    [DisplayName("工作流数据源")]
    public class WorkflowDataSource : ListDataSourceBase, IKeyValueDataSource,IFormService
    {
        [DisplayName("数据源类型")]
        public WorkflowDataSourceType SourceType { get; set; }

        private IList<IDictionary<string, object>> Items { get; set; }

        private bool DataIsEmpty { get; set; }

        public object Converter(string columnName, object columnValue, IDictionary<string, object> rowData)
        {
            if (Items == null && !DataIsEmpty)
            {
                Items = GetListInner().ToList();
                DataIsEmpty = Items.IsNullOrEmpty();
            }
            if(Items==null)
            {
                return columnValue;
            }
            else
            {
                return Items.FirstOrDefault(o => o["Id"].ToString() == columnValue.ToString())["Name"];
            }
        }

        protected override IEnumerable<IDictionary<string, object>> GetListInner()
        {
            WorkflowHelper helper = new WorkflowHelper(Sec.User);
            IList<IDictionary<string, object>> result = null;
            switch (SourceType)
            {
                case WorkflowDataSourceType.ToDo:
                    result = helper.GetToDoList(Sec.User.Id).ToDictionary();
                    break;
                case WorkflowDataSourceType.Done:
                    result = helper.GetDoneList(Sec.User.Id).ToDictionary();
                    break;
                case WorkflowDataSourceType.Read:
                    result = helper.GetReadList(Sec.User.Id).ToDictionary();
                    break;
                case WorkflowDataSourceType.Definition:
                    result = helper.GetAllDefinition().ToDictionary();
                    break;
                case WorkflowDataSourceType.Application:
                    result = helper.GetAllApplication().ToDictionary();
                    break;
                case WorkflowDataSourceType.Instance:
                    result = helper.GetAllInstance().ToDictionary();
                    break;
                default:
                    break;
            }
            return result;
        }

        public IEnumerable<TreeNode> SelectItems()
        {
            WorkflowHelper helper = new WorkflowHelper(Sec.User);
            IEnumerable<TreeNode> result = null;
            switch (SourceType)
            {
                case WorkflowDataSourceType.Definition:
                    result = helper.GetAllDefinition().Select(o => new TreeNode() { Text = o.Name, Value = o.Id });
                    break;
                case WorkflowDataSourceType.Application:
                    result = helper.GetAllApplication().Select(o => new TreeNode() { Text = o.Name, Value = o.Id });
                    break;
                default:
                    throw new FoxOneException("Not Support!");
            }
            return result;
        }

        public int Insert(IDictionary<string, object> data)
        {
            throw new NotImplementedException();
        }

        public int Update(string key, IDictionary<string, object> data)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, object> Get(string key)
        {
            throw new NotImplementedException();
        }

        public int Delete(string key)
        {
            WorkflowHelper helper = new WorkflowHelper(Sec.User);
            bool result = false;
            switch (SourceType)
            {
                case WorkflowDataSourceType.Definition:
                    if (DBContext<IWorkflowApplication>.Instance.Count(c => c.WorkflowId == key) > 0)
                    {
                        throw new FoxOneException("当前流程定义有关联的流程应用，不能删除！");
                    }
                    var components = DBContext<ComponentEntity>.Instance.Where(i => i.PageId.Equals(key, StringComparison.OrdinalIgnoreCase));
                    if (!components.IsNullOrEmpty())
                    {
                        components.ForEach(k =>
                        {
                            DBContext<ComponentEntity>.Delete(k);
                        });
                    }
                    result = DBContext<IWorkflowDefinition>.Delete(key);
                    break;
                case WorkflowDataSourceType.Application:
                    if (DBContext<IWorkflowInstance>.Instance.Count(c => c.ApplicationId == key) > 0)
                    {
                        throw new FoxOneException("当前流程应用有关联的实例，不能删除！");
                    }
                    result = DBContext<IWorkflowApplication>.Delete(key);
                    break;
                default:
                    throw new FoxOneException("Not Support!");
            }
            return result ? 1 : 0;
        }
    }

    public enum WorkflowDataSourceType
    {
        [Description("待办")]
        ToDo,

        [Description("已办")]
        Done,

        [Description("知会")]
        Read,

        [Description("流程定义")]
        Definition,

        [Description("流程应用")]
        Application,

        [Description("流程实例")]
        Instance,
    }
}
