using FoxOne.Business;
using FoxOne.Core;
using FoxOne.Workflow.Kernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace FoxOne.Workflow.Business
{
    public abstract class BaseCondition:ControlBase,ICondition
    {
        [Browsable(false)]
        public virtual ITransition Owner
        {
            get;
            set;
        }

        public abstract bool Resolve(IWorkflowContext context);

        public virtual bool PreResolve(IWorkflowContext context)
        {
            return Resolve(context);
        }

        protected void Log(string message)
        {
            Logger.GetLogger("Workflow").Info(message);
        }
    }
}
