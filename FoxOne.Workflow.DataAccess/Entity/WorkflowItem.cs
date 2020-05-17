/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/6/9 13:31:23
 * 描述说明：
 * *******************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FoxOne.Workflow.Kernel;
using FoxOne.Data.Attributes;
using FoxOne.Core;
using System.ComponentModel;
using FoxOne.Business;
namespace FoxOne.Workflow.DataAccess
{
    [Category("工作流管理")]
    [DisplayName("流程工作项")]
    [Table("WFL_WorkItem")]
    public class WorkflowItem :EntityBase, IWorkflowItem,IAutoCreateTable
    {
        [PrimaryKey]
        [Column(DataType="int",IsAutoIncrement=true)]
        public override string Id { get; set; }

        [Column(DataType="varchar",Length="30")]
        public string InstanceId
        {
            get;
            set;
        }

        public int ItemId
        {
            get;
            set;
        }

        public int ItemSeq
        {
            get;
            set;
        }

        [Column(DataType = "varchar", Length = "10")]
        public WorkItemStatus Status
        {
            get;
            set;
        }

        [Column(DataType = "varchar", Length = "1000")]
        public string OpinionContent
        {
            get;
            set;
        }


        public int OpinionType
        {
            get;
            set;
        }

        [Column(DataType = "varchar", Length = "20")]
        public string AppCode
        {
            get;
            set;
        }

        public int PreItemId
        {
            get;
            set;
        }

        [Column(DataType = "varchar", Length = "50")]
        public string PartUserId
        {
            get;
            set;
        }

        public string PartUserName
        {
            get;
            set;
        }

        [Column(DataType = "varchar", Length = "50")]
        public string PartDepartmentId
        {
            get;
            set;
        }

        public string PartDepepartmentName
        {
            get;
            set;
        }

        [Column(DataType = "varchar", Length = "50")]
        public string AssigneeUserId
        {
            get;
            set;
        }

        public string AssigneeUserName
        {
            get;
            set;
        }

        [Column(DataType = "varchar", Length = "100")]
        public string LevelCode
        {
            get;
            set;
        }

        [Column(DataType = "varchar", Length = "50")]
        public string ParallelInfo
        {
            get;
            set;
        }

        public string CurrentActivity
        {
            get;
            set;
        }

        public string Alias
        {
            get;
            set;
        }

        public bool AutoFinish
        {
            get;
            set;
        }

        [Column(DataType = "varchar", Length = "50")]
        public string PasserUserId
        {
            get;
            set;
        }

        public string PasserUserName
        {
            get;
            set;
        }

        public DateTime? ReceiveTime
        {
            get;
            set;
        }

        public DateTime? ReadTime
        {
            get;
            set;
        }

        public DateTime? FinishTime
        {
            get;
            set;
        }

        public DateTime? ExpiredTime
        {
            get;
            set;
        }

        public string UserChoice { get; set; }

        public string DisplayPartName 
        {
            get 
            {
                if (string.IsNullOrEmpty(AssigneeUserName) || AssigneeUserName.Equals(PartUserName, StringComparison.CurrentCultureIgnoreCase) || Status == WorkItemStatus.AutoFinished)
                {
                    return PartUserName;
                }
                return string.Format("{0} 代 {1} 办", AssigneeUserName, PartUserName);
            } 
        }

        public string DisplayPartComment
        {
            get
            {
                if (FinishTime == null)
                {
                    return string.Empty;
                }
                return OpinionContent;
            }
        }

        public string StatusText
        {
            get
            {
                return Status.GetDescription();
            }
        }

        public override bool Equals(object obj)
        {
            var target = obj as WorkflowItem;
            return target.InstanceId.Equals(this.InstanceId) && target.ItemId.Equals(ItemId);
        }

        public override int GetHashCode()
        {
            return this.InstanceId.GetHashCode() + this.ItemId.GetHashCode();
        }
    }


    [DisplayName("流程知会信息")]
    [Table("WFL_WorkItemRead")]
    public class WorkflowItemRead : WorkflowItem, IAutoCreateTable
    {

    }
}
