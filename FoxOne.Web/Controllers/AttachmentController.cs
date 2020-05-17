using FoxOne.Business;
using FoxOne.Business.Environment;
using FoxOne.Business.Security;
using FoxOne.Controls;
using FoxOne.Core;
using FoxOne.Data;
using FoxOne.Workflow.Business;
using FoxOne.Workflow.DataAccess;
using FoxOne.Workflow.Kernel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace FoxOne.Web.Controllers
{
    public class AttachmentController : BaseController
    {
        public ActionResult Index(string id)
        {

            if (id.IsNullOrEmpty())
            {
                throw new PageNotFoundException();
            }
            WorkflowHelper helper = new WorkflowHelper(Sec.User);
            helper.OpenWorkflow(id);
            bool canModify = false;
            if (helper.FlowInstance.FlowTag < FlowStatus.Finished && helper.FlowInstance.WorkItems.Any(o => o.Status < WorkItemStatus.Finished && o.PartUserId.Equals(Sec.User.Id, StringComparison.OrdinalIgnoreCase)))
            {
                canModify = true;
            }
            var workItemTable = new Table();
            workItemTable.AutoGenerateColum = false;
            workItemTable.AllowPaging = false;
            workItemTable.Columns.Add(new TableColumn() { ColumnName = "文件名", FieldName = "FileName", TextAlign = CellTextAlign.Center });
            workItemTable.Columns.Add(new TableColumn() { ColumnName = "上传者", FieldName = "CreatorId", TextAlign = CellTextAlign.Center, ColumnConverter = new EntityDataSource() { EntityType = typeof(User) } });
            workItemTable.Columns.Add(new TableColumn() { ColumnName = "上传时间", FieldName = "CreateTime", TextAlign = CellTextAlign.Center, DataFormatString = "{0:yyyy-MM-dd HH:mm}" });
            workItemTable.Columns.Add(new TableColumn() { ColumnName = "文件类型", FieldName = "FileType", TextAlign = CellTextAlign.Center });
            workItemTable.Columns.Add(new TableColumn() { ColumnName = "文件大小", FieldName = "FileSize", DataFormatString = "{0}（字节）", TextAlign = CellTextAlign.Center });
            workItemTable.Buttons.Add(new TableButton() { Id = "btnDownload", Href = "/Attachment/Download/{0}", Target = TableButtonTarget.Blank, CssClass = "btn btn-default btn-sm", Name = "下载", DataFields = "Id" });
            if (canModify)
            {
                workItemTable.Buttons.Add(new TableButton() { Id = "btnDeleteA", CssClass = "btn btn-danger btn-sm", Name = "删除", OnClick="return confirm('您确定要删除该附件吗？');", Href = "/Attachment/Delete/{0}", DataFields = "Id", TableButtonType = TableButtonType.TableRow, Filter = new StaticDataFilter() { ColumnName = "CreatorId", Operator = typeof(NotEqualOperation).FullName, Value = "$User.Id$" } });
            }
            workItemTable.DataSource = new EntityDataSource() { EntityType = typeof(AttachmentEntity), DataFilter = new StaticDataFilter() { ColumnName = "RelateId", Value = id, Operator = typeof(EqualsOperation).FullName } };
            ViewData["Table"] = workItemTable;
            ViewData["RelateId"] = id;
            ViewData["CanUpload"] = canModify;
            return View();
        }

        public ActionResult Delete(string id)
        {
            var attachment = DBContext<AttachmentEntity>.Instance.Get(id);
            if (attachment.CreatorId.Equals(Sec.User.Id, StringComparison.OrdinalIgnoreCase) || Sec.IsSuperAdmin)
            {
                DBContext<AttachmentEntity>.Delete(id);
            }
            else
            {
                throw new FoxOneException("您没有执行此操作的权限！");
            }
            return RedirectToAction("Index", new { id = attachment.RelateId });
        }

        public ActionResult Upload(HttpPostedFileBase file, string relateId)
        {
            if (file != null)
            {
                if (!relateId.IsNullOrEmpty())
                {
                    string fileName = file.FileName;
                    string ext = System.IO.Path.GetExtension(fileName).ToLower();
                    fileName = fileName.Replace(ext, "") + DateTime.Now.ToString("_yyyy_MM_dd_HH_mm_ss");
                    var filePath = UploadHelper.Upload(file, "uploadFiles", fileName, true);
                    DBContext<AttachmentEntity>.Insert(new AttachmentEntity()
                    {
                        Id = Guid.NewGuid().ToString(),
                        CreateTime = DateTime.Now,
                        CreatorId = Sec.User.Id,
                        FileName = file.FileName,
                        FilePath = filePath,
                        FileSize = file.ContentLength,
                        FileType = System.IO.Path.GetExtension(filePath),
                        RentId = 1,
                        RelateId = relateId
                    });
                }
            }
            return RedirectToAction("Index", new { id = relateId });
        }

        public FileResult Download(string id)
        {
            var attachment = DBContext<AttachmentEntity>.Get(id);
            if (attachment == null)
            {
                throw new PageNotFoundException();
            }
            return File(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, attachment.FilePath), "application/octet-stream", attachment.FileName);
        }


    }
}