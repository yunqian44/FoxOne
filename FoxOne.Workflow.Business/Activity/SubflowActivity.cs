/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/6/8 16:47:08
 * 描述说明：
 * *******************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FoxOne.Workflow.Kernel;
using System.ComponentModel;

namespace FoxOne.Workflow.Business
{
    [DisplayName("子流程步骤")]
    public class SubflowActivity:BaseActivity
    {

        public int WorkflowID
        {
            get;
            set;
        }

        public string WorkflowVersion
        {
            get;
            set;
        }

    }
}
