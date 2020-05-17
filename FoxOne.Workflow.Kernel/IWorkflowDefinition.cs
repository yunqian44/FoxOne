/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/1/26 12:01:34
 * 描述说明：
 * *******************************************************/
using FoxOne.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FoxOne.Workflow.Kernel
{
    public interface IWorkflowDefinition:IEntity
    {
        string Name { get; set; }

        string Definition { get; set; }

        string Description { get; set; }

        int Width { get; set; }

        int Height { get; set; }
    }
}
