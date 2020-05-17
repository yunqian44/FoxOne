/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/6/25 16:30:31
 * 描述说明：
 * *******************************************************/
using FoxOne.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FoxOne.Workflow.Kernel
{
    public interface IWorkDay:IEntity
    {
        DateTime Day { set; get; }

        bool IsWork { set; get; }

        string DepartmentId { set; get; }

        string Description { set; get; }
    }
}
