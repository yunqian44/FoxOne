/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/1/26 8:21:22
 * 描述说明：
 * *******************************************************/
using FoxOne.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace FoxOne.Workflow.Kernel
{
    /// <summary>
    /// 流程工作项接口
    /// </summary>
    public interface IWorkflowItem:IEntity
    {
        /// <summary>
        /// 流程实例号
        /// </summary>
        string InstanceId { get; set; }

        /// <summary>
        /// 流程任务号
        /// </summary>
        int ItemId { get; set; }

        /// <summary>
        /// 批次号
        /// </summary>
        int ItemSeq { get; set; }

        /// <summary>
        /// 工作项状态
        /// </summary>
        WorkItemStatus Status { get; set; }

        /// <summary>
        /// 审批意见
        /// </summary>
        string OpinionContent { get; set; }

        /// <summary>
        /// 审批意见显示区域
        /// </summary>
        int OpinionType { get; set; }

        /// <summary>
        /// 流程应用号
        /// </summary>
        string AppCode { get; set; }

        /// <summary>
        /// 上一任务号
        /// </summary>
        int PreItemId { get; set; }

        /// <summary>
        /// 工作项经办人ID
        /// </summary>
        string PartUserId { get; set; }

        /// <summary>
        /// 工作项经办人名称
        /// </summary>
        string PartUserName { get; set; }

        /// <summary>
        /// 工作项经办人所在部门ID
        /// </summary>
        string PartDepartmentId { get; set; }

        /// <summary>
        /// 工作项经办人所在部门名称
        /// </summary>
        string PartDepepartmentName { get; set; }

        /// <summary>
        /// 工作项代办人ID
        /// </summary>
        string AssigneeUserId { get; set; }

        /// <summary>
        /// 工作项代办人名称
        /// </summary>
        string AssigneeUserName { get; set; }

        /// <summary>
        /// 并行信息
        /// </summary>
        string LevelCode { get; set; }

        /// <summary>
        /// 并行KEY
        /// </summary>
        string ParallelInfo { get; set; }

        /// <summary>
        /// 工作项所属步骤名称
        /// </summary>
        string CurrentActivity { get; set; }

        /// <summary>
        /// 工作项所属步骤显示名称
        /// </summary>
        string Alias { get; set; }

        /// <summary>
        /// 是否自动结束
        /// </summary>
        bool AutoFinish { get; set; }

        /// <summary>
        /// 传递者ID
        /// </summary>
        string PasserUserId { get; set; }

        /// <summary>
        /// 传递者名称
        /// </summary>
        string PasserUserName { get; set; }

        /// <summary>
        /// 接收时间
        /// </summary>
        DateTime? ReceiveTime { get; set; }

        /// <summary>
        /// 阅读时间
        /// </summary>
        DateTime? ReadTime { get; set; }

        /// <summary>
        /// 完成时间
        /// </summary>
        DateTime? FinishTime { get; set; }

        /// <summary>
        /// 过期时间
        /// </summary>
        DateTime? ExpiredTime { get; set; }

        /// <summary>
        /// 用户选择
        /// </summary>
        string UserChoice { get; set; }

        /// <summary>
        /// 任务状态文本
        /// </summary>
        string StatusText { get; }
    }

    public enum WorkItemStatus
    {
        /// <summary>
        /// 送达
        /// </summary>
        [Description("送达")]
        Sent=0,

        /// <summary>
        /// 接收
        /// </summary>
        [Description("已读")]
        Received=1,

        /// <summary>
        /// 已阅
        /// </summary>
        [Description("已读")]
        Readed=2,

        /// <summary>
        /// 完成
        /// </summary>
        [Description("完成")]
        Finished=3,

        /// <summary>
        /// 自动完成
        /// </summary>
        [Description("自动完成")]
        AutoFinished,

        /// <summary>
        /// 暂停
        /// </summary>
        [Description("暂停")]
        Pause,

        /// <summary>
        /// 警告
        /// </summary>
        [Description("警告")]
        Alert,

        /// <summary>
        /// 过期
        /// </summary>
        [Description("过期")]
        Expire,

        /// <summary>
        /// 撤回
        /// </summary>
        [Description("撤回")]
        RollBack,

        /// <summary>
        /// 被撤回
        /// </summary>
        [Description("被撤回")]
        BeRollBack,

        /// <summary>
        /// 转签
        /// </summary>
        [Description("转签")]
        Assigned,

        /// <summary>
        /// 被跳转
        /// </summary>
        [Description("被跳转")]
        BeSwitched
    }
}
