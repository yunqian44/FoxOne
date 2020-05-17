/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/1/26 12:01:49
 * 描述说明：
 * *******************************************************/
using FoxOne.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FoxOne.Workflow.Kernel
{
    public interface IWorkflowApplication:IEntity
    {
        string Name { get; set; }

        string WorkflowId { get; set; }

        string Type { get; set; }

        string FormType { get; set; }

        string InstanceTitleTemplate { get; set; }

        string Icon { get; set; }

        string DocUrl { get; set; }

        string Description { get; set; }

        bool NeedAttachement { get; set; }
    }
}
