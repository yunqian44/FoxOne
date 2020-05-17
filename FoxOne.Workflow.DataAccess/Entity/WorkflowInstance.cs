/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/6/9 13:40:29
 * 描述说明：
 * *******************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FoxOne.Workflow.Kernel;
using FoxOne.Data.Attributes;
using FoxOne.Data;
using FoxOne.Core;
using System.ComponentModel;
using System.Threading;
using FoxOne.Business;

namespace FoxOne.Workflow.DataAccess
{
    [Category("工作流管理")]
    [DisplayName("流程实例信息")]
    [Table("WFL_Instance")]
    public partial class WorkflowInstance :EntityBase, IWorkflowInstance, IAutoCreateTable
    {

        public string ApplicationId
        {
            get;
            set;
        }

        public IWorkflowApplication Application
        {
            get
            {
                return DBContext<IWorkflowApplication>.Instance.Get(ApplicationId);
            }
        }

        public string InstanceName
        {
            get;
            set;
        }

        public int WorkItemNewTask
        {
            get;
            set;
        }

        public int WorkItemNewSeq
        {
            get;
            set;
        }

        public string CurrentActivityName
        {
            get;
            set;
        }

        public int ImportantLevel
        {
            get;
            set;
        }

        public int SecretLevel
        {
            get;
            set;
        }

        public string CreatorId
        {
            get;
            set;
        }

        public IUser Creator
        {
            get
            {
                return DBContext<IUser>.Instance.Get(CreatorId);
            }
        }

        public FlowStatus FlowTag
        {
            get;
            set;
        }

        public DateTime? StartTime
        {
            get;
            set;
        }

        public DateTime? EndTime
        {
            get;
            set;
        }

        public string DataLocator
        {
            get;
            set;
        }

        public string RelateItems
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

    }
}
