using FoxOne.Business;
/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2014/12/29 12:39:40
 * 描述说明：
 * *******************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FoxOne.Workflow.Kernel
{
    /// <summary>
    /// 流程迁移条件接口
    /// </summary>
    public interface ICondition : IControl
    {
        /// <summary>
        /// 条件所属的迁移
        /// </summary>
        ITransition Owner { get; set; }

        /// <summary>
        /// 求解条件是否成立
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        bool Resolve(IWorkflowContext context);

        /// <summary>
        /// 预求解条件是否成立（通常用于选人界面显示哪些迁移供选择）
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        bool PreResolve(IWorkflowContext context);
    }
}
