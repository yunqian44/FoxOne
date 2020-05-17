/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/1/26 11:19:10
 * 描述说明：
 * *******************************************************/
using FoxOne.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FoxOne.Workflow.Kernel
{
    public interface IWorkflowChoice
    {
        /// <summary>
        /// 选择的步骤，多个步骤用逗号隔开
        /// </summary>
        string Choice { get; set; }

        /// <summary>
        /// 选择的参与者
        /// </summary>
        IList<IUser> Participant { get; set; }
    }
}
