/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/1/26 8:21:09
 * 描述说明：
 * *******************************************************/
using FoxOne.Core;
using System;
using System.Collections.Generic;
namespace FoxOne.Workflow.Kernel
{
    /// <summary>
    /// 流程实例服务类接口
    /// </summary>
    public interface IWorkflowInstanceService:IService<IWorkflowInstance>
    {
        /// <summary>
        /// 根据业务主键获取流程实例
        /// </summary>
        /// <param name="datalocator">业务主键ID</param>
        /// <returns></returns>
        IWorkflowInstance GetInstanceByDataLocator(string datalocator);

        /// <summary>
        /// 流程回到指定步骤
        /// </summary>
        /// <param name="procID">流程实例号</param>
        /// <param name="taskID">工作项ID</param>
        /// <param name="taskSeq">工作项序号</param>
        /// <param name="currentActi">步骤名称</param>
        /// <returns></returns>
        bool BackToRunning(string procID, int taskID, int taskSeq, string currentActi);
    }
}
