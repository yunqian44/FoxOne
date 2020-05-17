using FoxOne.Business;
/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2014/12/29 12:40:44
 * 描述说明：
 * *******************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FoxOne.Workflow.Kernel
{
    /// <summary>
    /// 流程迁移接口
    /// </summary>
    public interface ITransition : IControl
    {
        /// <summary>
        /// 迁移所属流程对象
        /// </summary>
        IWorkflow Owner { get; set; }

        /// <summary>
        /// 迁移条件
        /// </summary>
        ICondition Condition { get; set; }

        /// <summary>
        /// 迁移显示文本
        /// </summary>
        string Label { get; set; }

        /// <summary>
        /// 迁移描述
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// 迁移的目标步骤
        /// </summary>
        IActivity To { get; set; }

        /// <summary>
        /// 迁移的源步骤
        /// </summary>
        IActivity From { get; set; }

        /// <summary>
        /// 迁移排序值
        /// </summary>
        int Rank { get; set; }

        /// <summary>
        /// 能否退回
        /// </summary>
        bool CanPushBack { get; set; }

        /// <summary>
        /// 求解迁移是否成立
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        bool Resolve(IWorkflowContext context);

        /// <summary>
        /// 迁移预求解
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        bool PreResolve(IWorkflowContext context);
    }
}
