/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/6/6 23:35:46
 * 描述说明：
 * *******************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FoxOne.Workflow.Kernel;
using FoxOne.Data;
using FoxOne.Core;
using FoxOne.Data.Attributes;
using FoxOne.Business;

namespace FoxOne.Workflow.DataAccess
{
    [Table("WFL_Param")]
    public class WorkflowParam:EntityBase,IAutoCreateTable
    {
        [PrimaryKey]
        public override string Id
        {
            get;
            set;
        }

        public int Value { get; set; }
    }

    public class WorkflowInstanceService : IWorkflowInstanceService
    {
        private string GetNewInstanceId()
        {
            string id = DateTime.Now.ToString("yyyyMMdd").Substring(2);
            int value = 1;
            var entity = Dao.Get().Get<WorkflowParam>(id);
            if(entity==null)
            {
                var param = new WorkflowParam() { Id = id, Value = value };
                Dao.Get().Insert(param);
            }
            else
            {
                value = entity.Value + 1;
                Dao.Get().Update(new WorkflowParam() { Id = id, Value = value });
            }
            string procID = value.ToString();
            procID = id + procID.PadLeft(5, '0');
            return procID;
        }

        public IWorkflowInstance Get(object id)
        {
            //return CacheHelper.GetFromCache<IWorkflowInstance>(id.ToString(), () =>
            //{
                return Dao.Get().Get<WorkflowInstance>(id);
            //});
        }

        public IWorkflowInstance GetInstanceByDataLocator(string datalocator)
        {
            return Dao.Get().Query<WorkflowInstance>().FirstOrDefault(o => o.DataLocator == datalocator);
        }

        public bool BackToRunning(string procID,int itemId,int taskSeq,string currentActi)
        {
            var entity = Get(procID);
            entity.WorkItemNewSeq = taskSeq;
            entity.WorkItemNewTask = itemId;
            entity.CurrentActivityName = currentActi;
            entity.FlowTag = FlowStatus.Running;
            entity.EndTime = null;
            return Update(entity) > 0;
        }


        public IEnumerable<IWorkflowInstance> Select(object parameter=null)
        {
            return Dao.Get().Select<WorkflowInstance>(parameter);
        }

        public int Insert(IWorkflowInstance entity)
        {
            entity.Id = GetNewInstanceId();
            return DBContext<IWorkflowInstance>.Insert(entity) ? 1 : 0;
        }

        public int Update(IWorkflowInstance entity)
        {
            return DBContext<IWorkflowInstance>.Update(entity) ? 1 : 0;
        }

        public int Delete(object id)
        {
            return DBContext<IWorkflowInstance>.Delete(id) ? 1 : 0;
        }
    }
}
