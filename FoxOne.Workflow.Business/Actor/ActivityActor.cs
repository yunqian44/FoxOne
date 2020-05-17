/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/1/26 11:49:42
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
    [Serializable,DisplayName("流程步骤参与者")]
    public class ActivityActor:BaseActor
    {

        [DisplayName("步骤名称")]
        public string Activity { get; set; }

        [DisplayName("求解方式")]
        public ActorSelctMode ActorSelctMode { get; set; }

        public override IList<IUser> Resolve(IWorkflowContext context)
        {
            if (string.IsNullOrEmpty(Activity))
            {
                throw new ArgumentNullException(string.Format("{0}:在步骤【{1}】求解参与者发生异常，【流程步骤参与者】需要指定步骤名", context.FlowInstance.Id, Owner.Name));
            }
            var workitems = context.FlowInstance.WorkItems.Where(o => o.CurrentActivity == Activity);
            if (workitems == null || workitems.Count() == 0)
            {
                throw new ArgumentNullException(string.Format("{0}:在步骤【{1}】求解参与者发生异常，步骤【{2}】未有任何参与者", context.FlowInstance.Id, Owner.Name, Activity));
            }
            var userIds = new List<string>();
            switch (ActorSelctMode)
            {
                case ActorSelctMode.All:
                    foreach (var item in workitems)
                    {
                        userIds.Add(item.PartUserId);
                    }
                    break;
                case ActorSelctMode.LastOne:
                    userIds.Add(workitems.OrderByDescending(o => o.ItemId).First().PartUserId);
                    break;
                case ActorSelctMode.FirstOne:
                    userIds.Add(workitems.OrderBy(o => o.ItemId).First().PartUserId);
                    break;
                case ActorSelctMode.FirstSeq:
                    int firstSeq = workitems.OrderBy(o => o.ItemSeq).First().ItemSeq;
                    foreach (var item in workitems.Where(o => o.ItemSeq == firstSeq))
                    {
                        userIds.Add(item.PartUserId);
                    }
                    break;
                case ActorSelctMode.LastSeq: 
                    int lastSeq = workitems.OrderByDescending(o => o.ItemSeq).First().ItemSeq;
                    foreach (var item in workitems.Where(o => o.ItemSeq == lastSeq))
                    {
                        userIds.Add(item.PartUserId);
                    }
                    break;
            }
            if (userIds.Count == 0)
            {
                throw new ArgumentNullException(string.Format("{0}:在步骤【{1}】求解参与者发生异常", context.FlowInstance.Id, Owner.Name));
            }

            return DBContext<IUser>.Instance.Where(o => userIds.Contains(o.Id,StringComparer.OrdinalIgnoreCase)).ToList();
        }
    }

    public enum ActorSelctMode
    {
        /// <summary>
        /// 全部历史的该步骤的参与者，结果会去除重复项
        /// </summary>
        [Description("全部")]
        All = 0,

        /// <summary>
        ///  最后一个该步骤的参与者，结果会去除重复项
        /// </summary>
        [Description("最后一个")]
        LastOne = 1,

        /// <summary>
        /// 第一个该步骤的参与者，结果会去除重复项
        /// </summary>
        [Description("第一个")]
        FirstOne = 2,

        /// <summary>
        /// 最后一个批次的该步骤的参与者，结果会去除重复项
        /// </summary>
        [Description("最后一批")]
        LastSeq = 3,

        /// <summary>
        /// 第一个批次的该步骤的参与者，结果会去除重复项
        /// </summary>
        [Description("第一批")]
        FirstSeq = 4,
    }
}
