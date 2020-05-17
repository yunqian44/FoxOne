/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/1/26 8:21:09
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
    /// 流程实例接口
    /// </summary>
    public interface IWorkflowInstance:IEntity
    {
        /// <summary>
        /// 流程实例名称
        /// </summary>
        string InstanceName { get; set; }

        /// <summary>
        /// 流程应用ID
        /// </summary>
        string ApplicationId { get; set; }

        /// <summary>
        /// 流程应用名称
        /// </summary>
        IWorkflowApplication Application { get; }

        /// <summary>
        /// 当前最新工作项号
        /// </summary>
        int WorkItemNewTask { get; set; }

        /// <summary>
        /// 当前最新工作项批次
        /// </summary>
        int WorkItemNewSeq { get; set; }

        /// <summary>
        /// 当前步骤
        /// </summary>
        string CurrentActivityName { get; set; }

        /// <summary>
        /// 缓急
        /// </summary>
        int ImportantLevel { get; set; }

        /// <summary>
        /// 密级
        /// </summary>
        int SecretLevel { get; set; }

        /// <summary>
        /// 创建者ID
        /// </summary>
        string CreatorId { get; set; }

        /// <summary>
        /// 创建者名称
        /// </summary>
        IUser Creator { get; }

        /// <summary>
        /// 实例状态
        /// </summary>
        FlowStatus FlowTag { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        DateTime? StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        DateTime? EndTime { get; set; }

        /// <summary>
        /// 业务主键
        /// </summary>
        string DataLocator { get; set; }

        /// <summary>
        /// 关联实例
        /// </summary>
        string RelateItems { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// 所有工作项集合
        /// </summary>
        IList<IWorkflowItem> WorkItems { get; }

        /// <summary>
        /// 所有传阅集合
        /// </summary>
        IList<IWorkflowItem> WorkItemsRead { get; }

        /// <summary>
        /// 所有参数集合
        /// </summary>
        IDictionary<string,object> Parameters { get; }

        /// <summary>
        /// 获取并行信息LevelCode
        /// </summary>
        /// <param name="levelCodePrefix">并行信息前辍，如【00-5:】</param>
        /// <param name="count">数量</param>
        /// <returns></returns>
        List<string> GetLevelCode(string levelCodePrefix, int count);

        /// <summary>
        /// 判断并行工作项是否全部结束
        /// </summary>
        /// <param name="levelCode">并行信息</param>
        /// <returns></returns>
        bool IsAllParallelItemFinished(string levelCode);

        /// <summary>
        /// 获取当前传阅项的最大任务号
        /// </summary>
        /// <returns></returns>
        int GetMaxReadTaskID();

        int InsertWorkItem(IWorkflowItem entity);

        int UpdateWorkItem(IWorkflowItem entity);

        int UpdateWorkItem(IList<IWorkflowItem> entities);

        int DeleteWorkItem();

        bool BackToTask(int taskId);

        bool SetParameter(string key,string value);

        int DeleteParameter();
    }

    public enum FlowStatus
    {
        /// <summary>
        /// 拟稿
        /// </summary>
        [Description("拟稿")]
        Begin,

        /// <summary>
        /// 运行中
        /// </summary>
        [Description("运行中")]
        Running,

        /// <summary>
        /// 完成
        /// </summary>
        [Description("结束")]
        Finished
    }
}
