/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/1/26 11:19:31
 * 描述说明：
 * *******************************************************/
using FoxOne.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FoxOne.Workflow.Kernel
{
    public interface IWorkflowContext
    {
        /// <summary>
        /// 当前用户信息
        /// </summary>
        IUser CurrentUser { get; set; }

        /// <summary>
        /// 当前流程实例信息
        /// </summary>
        IWorkflowInstance FlowInstance { get; set; }

        /// <summary>
        /// 当前工作项
        /// </summary>
        IWorkflowItem CurrentTask { get; set; }

        /// <summary>
        /// 用户选择
        /// </summary>
        IList<IWorkflowChoice> UserChoice { get; set; }

        /// <summary>
        /// 流程参数
        /// </summary>
        IDictionary<string, object> Parameter { get; set; }

        /// <summary>
        /// 并行分支标识
        /// </summary>
        string LevelCode { get; set; }

        /// <summary>
        /// 意见内容
        /// </summary>
        string OpinionContent { get; set; }

        /// <summary>
        /// 意见所属区域
        /// </summary>
        int OpinionArea { get; set; }
    }
}
