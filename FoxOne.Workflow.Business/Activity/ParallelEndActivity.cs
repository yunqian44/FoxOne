/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/6/8 16:45:42
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
    [DisplayName("并行流程结束步骤")]
    public class ParallelEndActivity : BaseActivity
    {
        public override bool AutoRun
        {
            get
            {
                return true;
            }
        }

        public override bool CanEnter(IWorkflowContext context)
        {

            //判断并行分支是否已经全部完成
            var levelCode = context.CurrentTask.LevelCode;
            bool result =context.FlowInstance.IsAllParallelItemFinished(levelCode);
            Log(string.Format("{0}:与levelCode为:{1}的工作项并行的工作项是否全部结束：{2}", context.FlowInstance.Id, levelCode, result));
            return result;

        }


        public override void Enter(IWorkflowContext context)
        {
            //增加一条工作项，系统自动完成
            //如果当前工作项levelCode为00-4:01 则结果为00
            //如果当前工作项levelCode为00-4:01-10:01 则结果为00-4:01
            string levelCode = context.CurrentTask.LevelCode.Substring(0, context.CurrentTask.LevelCode.LastIndexOf('-'));
            var workitem = ObjectHelper.GetObject<IWorkflowItem>();
            var instanceHelper = ObjectHelper.GetObject<IWorkflowInstanceService>();

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
            context.LevelCode = levelCode;
        }
    }
}
