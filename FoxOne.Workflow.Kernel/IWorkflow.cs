/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2014/12/29 12:40:58
 * 描述说明：
 * *******************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FoxOne.Workflow.Kernel
{
    /// <summary>
    /// 流程对象接口定义
    /// </summary>
    public interface IWorkflow
    {
        /// <summary>
        /// 根据步骤名称获取步骤对象
        /// </summary>
        /// <param name="ActivityName">步骤名称</param>
        /// <returns></returns>
        IActivity this[string ActivityName] { get; }

        /// <summary>
        /// 流程的开始步骤
        /// </summary>
        IActivity Root { get; set; }

        /// <summary>
        /// 流程的所有步骤的集合
        /// </summary>
        IList<IActivity> Activities { get; set; }

        /// <summary>
        /// 流程的所有迁移线的集合
        /// </summary>
        IList<ITransition> Transitions { get; set; }

        /// <summary>
        /// 流程名称
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 新增流程实例
        /// </summary>
        /// <param name="instance"></param>
        void AddInstance(IWorkflowInstance instance);

        /// <summary>
        /// 更新流程实例
        /// </summary>
        /// <param name="instance"></param>
        void UpdateInstance(IWorkflowInstance instance);

        /// <summary>
        /// 删除流程实例
        /// </summary>
        /// <param name="instance"></param>
        void DeleteInstance(IWorkflowInstance instance);

        /// <summary>
        /// 运行流程
        /// </summary>
        /// <param name="context"></param>
        void Run(IWorkflowContext context);

        /// <summary>
        /// 流程实例服务类
        /// </summary>
        IWorkflowInstanceService InstanceService { get; }
    }
}
