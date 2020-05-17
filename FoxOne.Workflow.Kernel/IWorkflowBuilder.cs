/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/6/17 13:13:46
 * 描述说明：
 * *******************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FoxOne.Workflow.Kernel
{
    public interface IWorkflowBuilder
    {
        IWorkflow Build(string workflowId);

        bool ClearCache(string workflowId);
    }
}
