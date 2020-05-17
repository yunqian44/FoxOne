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
    public abstract class BaseActor:ControlBase,IActor
    {
        [Browsable(false)]
        public virtual IActivity Owner
        {
            get;
            set;
        }

        public abstract IList<IUser> Resolve(IWorkflowContext context);
    }
}
