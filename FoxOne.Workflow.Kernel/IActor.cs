using FoxOne.Business;
/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2014/12/29 12:39:26
 * 描述说明：
 * *******************************************************/
using FoxOne.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FoxOne.Workflow.Kernel
{
    /// <summary>
    /// 流程参与者接口
    /// </summary>
    public interface IActor : IControl
    {
        /// <summary>
        /// 参与者所属的流程步骤
        /// </summary>
        IActivity Owner { get; set; }

        /// <summary>
        /// 求解参与者
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        IList<IUser> Resolve(IWorkflowContext context);
    }
}
