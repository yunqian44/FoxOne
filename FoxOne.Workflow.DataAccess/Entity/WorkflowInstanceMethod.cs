/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/1/26 8:21:22
 * 描述说明：
 * *******************************************************/
using FoxOne.Core;
using FoxOne.Data;
using FoxOne.Workflow.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FoxOne.Workflow.DataAccess
{
    public partial class WorkflowInstance
    {
        private IList<IWorkflowItem> _WorkItems = null;
        private IList<IWorkflowItem> _WorkItemsRead = null;
        private IDictionary<string, object> _parameter = null;

        private T LockExecute<T>(T item, Func<T> func)
        {
            if (item == null)
            {
                lock (this)
                {
                    if (item == null)
                    {
                        item = func();
                    }
                }
            }
            return item;
        }

        private T LockExecute<T>(Func<T> func)
        {
            var result = default(T);
            lock (this)
            {
                result = func();
            }
            return result;
        }


        public IList<IWorkflowItem> WorkItems
        {
            get
            {
                return LockExecute<IList<IWorkflowItem>>(_WorkItems, () =>
                {
                    var parameters = new { InstanceId = Id };
                    _WorkItems = Dao.Get().Query<WorkflowItem>().Where(o=>o.InstanceId==Id).ToList().ToList<IWorkflowItem>();
                    return _WorkItems;
                });
            }
        }

        public IList<IWorkflowItem> WorkItemsRead
        {
            get
            {
                return LockExecute<IList<IWorkflowItem>>(_WorkItemsRead, () =>
                {
                    var parameters = new { InstanceId = Id };
                    _WorkItemsRead = Dao.Get().Query<WorkflowItemRead>().Where(o=>o.InstanceId==Id).ToList().ToList<IWorkflowItem>();
                    return _WorkItemsRead;
                });
            }
        }

        public IDictionary<string, object> Parameters
        {
            get
            {
                if (_parameter == null)
                {
                    _parameter = new Dictionary<string, object>();
                }
                return _parameter;
                /*return LockExecute<IDictionary<string, object>>(_parameter, () =>
                {
                    _parameter = new Dictionary<string, object>();
                    var workflowParameter = Dao.Get().Select<WorkflowParameter>(new { InstanceId = Id });
                    if (workflowParameter != null && workflowParameter.Count > 0)
                    {
                        foreach (var p in workflowParameter)
                        {
                            _parameter[p.Key] = p.Value;
                        }
                    }
                    return _parameter;
                });*/
            }
        }

        public bool SetParameter(string key, string value)
        {
            /*IWorkflowParameter entity = ObjectHelper.GetObject<IWorkflowParameter>();
            entity.InstanceId = Id;
            entity.Key = key;
            entity.Value = value;
            entity.Id = Guid.NewGuid().ToString();
            int effectCount = Dao.Get().Update(entity);
            if (effectCount == 0)
            {
                effectCount = Dao.Get().Insert(entity);
            }
            if (effectCount > 0)
            {
                Parameters[key] = value;
                return true;
            }
            return false;
            */
            Parameters[key] = value;
            return true;
        }

        public int DeleteParameter()
        {
            _parameter = null;
            return 1;
            //return Dao.Get().BatchDelete<WorkflowParameter>(new { InstanceId = Id });
        }

        public List<string> GetLevelCode(string levelCodePrefix, int count)
        {
            return LockExecute<List<string>>(() =>
            {
                List<string> returnValue = new List<string>();
                var allItem = WorkItems.OrderByDescending(o => o.ItemId); //这里获取的结果是对TASKID从大到小排
                int result = 0;
                foreach (var item in allItem)
                {
                    if (item.LevelCode.StartsWith(levelCodePrefix))
                    {
                        result = int.Parse(item.LevelCode.Substring(levelCodePrefix.Length));
                        break;
                    }
                }

                for (int i = 1; i <= count; i++)
                {
                    returnValue.Add(levelCodePrefix + (result + i).ToString().PadLeft(2, '0'));
                }
                return returnValue;
            });
        }

        public bool IsAllParallelItemFinished(string levelCode)
        {
            //例如 传入的levelCode为 00-1:03，则需要在数据库中查找00-1:开头的所有工作项，判断是否全部结束
            //如果有未结束，则表明并行分支还有工作项未结束。
            return LockExecute<bool>(() =>
            {
                string startWithString = levelCode.Substring(0, levelCode.Length - 2);
                foreach (var item in WorkItems)
                {
                    if (item.LevelCode.StartsWith(startWithString))
                    {
                        if (item.Status < WorkItemStatus.Finished)
                        {
                            return false;
                        }
                    }
                }
                return true;
            });
        }

        public int GetMaxReadTaskID()
        {
            if (WorkItemsRead.IsNullOrEmpty())
            {
                return 10000;
            }
            return WorkItemsRead.OrderByDescending(o => o.ItemId).First().ItemId;
        }

        public int InsertWorkItem(IWorkflowItem entity)
        {
            return LockExecute<int>(() =>
            {
                if (!string.IsNullOrEmpty(entity.Alias) && entity.Alias.IndexOf('.') > 0)
                {
                    entity.Alias = entity.Alias.Substring(entity.Alias.LastIndexOf('.') + 1);
                }
                if (entity.ItemId >= 10000)
                {
                    WorkItemsRead.Add(entity);
                    return Dao.Get().Insert<WorkflowItemRead>(entity);
                }
                else
                {
                    WorkflowEventManager.RaiseWorkItemEvent(EventStep.Before, ItemActionType.Insert, this, entity);
                    WorkItems.Add(entity);
                    int result = Dao.Get().Insert<WorkflowItem>(entity);
                    if (result > 0)
                    {
                        WorkflowEventManager.RaiseWorkItemEvent(EventStep.After, ItemActionType.Insert, this, entity);
                    }
                    return result;
                }
            });
        }

        public int UpdateWorkItem(IWorkflowItem entity)
        {
            return LockExecute<int>(() =>
            {
                if (entity.ItemId >= 10000)
                {
                    WorkItemsRead.Remove(entity);
                    WorkItemsRead.Add(entity);
                    return Dao.Get().Update<WorkflowItemRead>(entity);
                }
                else
                {
                    WorkItems.Remove(entity);
                    WorkItems.Add(entity);
                    return Dao.Get().Update<WorkflowItem>(entity);
                }
            });
        }

        public int UpdateWorkItem(IList<IWorkflowItem> entities)
        {
            return LockExecute<int>(() =>
            {
                int effectCount = 0;
                foreach (var entity in entities)
                {
                    if (entity.ItemId >= 10000)
                    {
                        WorkItemsRead.Remove(entity);
                        WorkItemsRead.Add(entity);
                        effectCount += Dao.Get().Update<WorkflowItemRead>(entity);
                    }
                    else
                    {
                        WorkItems.Remove(entity);
                        WorkItems.Add(entity);
                        effectCount += Dao.Get().Update<WorkflowItem>(entity);
                    }
                }
                return effectCount;
            });
        }

        public int DeleteWorkItem()
        {
            return LockExecute<int>(() =>
            {
                int effectCount = Dao.Get().Delete<WorkflowItem>().Where(o => o.InstanceId==Id).Execute();
                effectCount += Dao.Get().Delete<WorkflowItemRead>().Where(o => o.InstanceId == Id).Execute();
                _WorkItems = null;
                return effectCount;
            });
        }

        public bool BackToTask(int taskId)
        {
            return LockExecute<bool>(() =>
            {
                var items = Dao.Get().Delete<WorkflowItem>().Where(o => o.ItemId > taskId).Execute();// WorkItems.Where(o => o.ItemId > taskId).ToList();
                if (items == 0)
                {
                    return false;
                }
                var currentItem = WorkItems.FirstOrDefault(o => o.ItemId == taskId);
                currentItem.OpinionContent = string.Empty;
                currentItem.FinishTime = null;
                currentItem.Status = WorkItemStatus.Sent;
                return UpdateWorkItem(currentItem) == 1;
            });
        }
    }
}
