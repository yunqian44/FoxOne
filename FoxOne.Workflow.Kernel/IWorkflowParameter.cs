/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/6/7 12:11:22
 * 描述说明：
 * *******************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FoxOne.Workflow.Kernel
{
    /// <summary>
    /// 流程参数接口
    /// </summary>
    public interface IWorkflowParameter
    {
        string Id { get; set; }

        /// <summary>
        /// 流程实例号
        /// </summary>
        string InstanceId { get; set; }

        /// <summary>
        /// 参数名
        /// </summary>
        string Key { get; set; }

        /// <summary>
        /// 参数值
        /// </summary>
        string Value { get; set; }
    }
}
