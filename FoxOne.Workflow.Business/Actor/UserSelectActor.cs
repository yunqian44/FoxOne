/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/6/9 10:24:34
 * 描述说明：
 * *******************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FoxOne.Workflow.Kernel;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.ComponentModel;
using FoxOne.Core;

namespace FoxOne.Workflow.Business
{
    [Serializable, DisplayName("用户选择参与者")]
    public class UserSelectActor : BaseActor
    {
        private IActivity _owner;

        [Browsable(false)]
        public override IActivity Owner
        {
            get
            {
                return _owner;
            }
            set
            {
                _owner = value;
                if (InnerActor != null)
                {
                    InnerActor.Owner = value;
                }
            }
        }

        [DefaultValue(typeof(RoleActor)),DisplayName("供选择的参与者")]
        public IActor InnerActor
        {
            get;
            set;
        }

        [DisplayName("只允许单选")]
        public bool OnlySingleSelect
        {
            get;
            set;
        }

        [DisplayName("允许任意选人")]
        public bool AllowFree
        {
            get;
            set;
        }

        [DisplayName("自动选中全部")]
        public bool AutoSelectAll
        {
            get;
            set;
        }

        public override IList<IUser> Resolve(IWorkflowContext context)
        {
            if (context.UserChoice != null && context.UserChoice.Count > 0)
            {
                foreach (var u in context.UserChoice)
                {
                    if (u.Choice.Equals(Owner.Name))
                    {
                        return u.Participant;
                    }
                }
            }
            throw new ArgumentNullException(string.Format("没有选择步骤:{0} 的参与者", Owner.Name));
        }
    }
}
