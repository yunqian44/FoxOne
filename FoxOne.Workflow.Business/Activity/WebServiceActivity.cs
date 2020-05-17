using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FoxOne.Workflow.Kernel;
using System.ComponentModel;

namespace FoxOne.Workflow.Business
{
    [DisplayName("接口调用步骤")]
    public class WebServiceActivity:BaseActivity
    {
        public override bool AutoRun
        {
            get
            {
                return true;
            }
        }

        [DisplayName("是否异步调用")]
        public bool IsAsync { get; set; }

        public string ExceptionHandlerType { get; set; }


        [DisplayName("服务地址 ")]
        public string ServiceUrl { get; set; }

        public override void Execute(IWorkflowContext context)
        {

        }
    }
}
