/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/6/8 16:45:14
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
    [DisplayName("并行流程开始步骤")]
    public class ParallelStartActivity : BaseActivity
    {

        public override bool AutoRun
        {
            get
            {
                return true;
            }
        }

        public override void Enter(IWorkflowContext context)
        {
            //增加一条工作项，系统自动完成，LevelCode取CurrentTask的
            string levelCode = context.CurrentTask.LevelCode;
            var workitem = ObjectHelper.GetObject<IWorkflowItem>();

            string instanceId = context.FlowInstance.Id;
            var instance = context.FlowInstance.WorkItems.OrderByDescending(o => o.ItemId).First();
            workitem.AutoFinish = true;
            workitem.Status = WorkItemStatus.AutoFinished;
            workitem.ItemId = instance.ItemId + 1;
            workitem.ItemSeq = instance.ItemSeq + 1;
            workitem.AppCode = context.CurrentTask.AppCode;
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
            context.LevelCode = string.Format("{0}-{1}:", levelCode, workitem.ItemId);
            Log(string.Format("{0}:改写了levelCode为：{1}", context.FlowInstance.Id, context.LevelCode));
        }
    }
}
