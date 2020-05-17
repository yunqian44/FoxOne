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
    public abstract class BaseActivity : ControlBase, IActivity
    {
        [DisplayName("步骤名称")]
        [FormField(Editable =false)]
        public string Name
        {
            get;
            set;
        }

        [DisplayName("显示名称")]
        public string Alias
        {
            get;
            set;
        }

        [DisplayName("需要弹出选人框"), DefaultValue(true)]
        public virtual bool NeedChoice
        {
            get;
            set;
        }

        [Browsable(false)]
        public IWorkflow Owner
        {
            get;
            set;
        }

        [DefaultValue(typeof(UserSelectActor)), DisplayName("参与者")]
        public IActor Actor
        {
            get;
            set;
        }

        [Browsable(false)]
        public IList<ITransition> Transitions
        {
            get;
            set;
        }

        [DisplayName("位置X")]
        public int Left { get; set; }

        [DisplayName("位置Y")]
        public int Top { get; set; }

        [DisplayName("图标")]
        public string Icon { get; set; }

        public virtual bool AutoRun
        {
            get { return false; }
        }

        public virtual bool CanEnter(IWorkflowContext context)
        {
            return true;
        }

        public virtual bool CanExecute(IWorkflowContext context)
        {
            return true;
        }

        public virtual bool CanExit(IWorkflowContext context)
        {
            return true;
        }

        public virtual void Enter(IWorkflowContext context)
        {

        }

        public virtual void Execute(IWorkflowContext context)
        {

        }

        public virtual void Exit(IWorkflowContext context)
        {

        }

        protected void Log(string message)
        {
            Logger.GetLogger("Workflow").Info(message);
        }
    }
}
