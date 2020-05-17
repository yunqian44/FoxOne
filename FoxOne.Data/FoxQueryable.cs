using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using FoxOne.Core;
using FoxOne.Data.Mapping;
using System.Collections;
namespace FoxOne.Data
{
    public class DaoQueryable<T> : DaoQueryableBase<T> where T : class, new()
    {
        public DaoQueryable(Dao dao) : base(dao)
        {
        }

        public string OrderByStr { get; set; }

        public T Single()
        {
            var mapping = TableMapper.GetTableMapping(_dao, typeof(T));
            var command = _dao.Provider.CreateSelectAllCommand(mapping, null);
            return _dao.QueryEntity<T>(command.CommandText + WhereString + OrderByStr, Parameter);
        }

        public IList<TResult> Select<TResult>(Expression<Func<T, TResult>> func)
        {
            string fieldName = new ExpressionTool() { Parameter = Parameter, Provider = _dao.Provider, TargetType = typeof(T) }.GetSqlByExpression(func).ToString();
            fieldName = _dao.Provider.EscapeIdentifier(fieldName);
            var mapping = TableMapper.GetTableMapping(_dao, typeof(T));
            var command = _dao.Provider.CreateSelectAllCommand(mapping, null);
            return _dao.QueryScalarList<TResult>(command.CommandText.Replace("*", fieldName) + WhereString + OrderByStr, Parameter);
        }

        public int Count(Expression<Func<T, bool>> func)
        {
            Where(func);
            var mapping = TableMapper.GetTableMapping(_dao, typeof(T));
            var command = _dao.Provider.CreateSelectAllCommand(mapping, null);
            return _dao.QueryScalar<int>(_dao.Provider.WrapCountSql(command.CommandText + WhereString), Parameter);
        }

        public T FirstOrDefault(Expression<Func<T, bool>> func)
        {
            Where(func);
            return Single();
        }

        public DaoQueryable<T> Where(Expression<Func<T, bool>> func)
        {
            WhereInner(func);
            return this;
        }

        public IList<T> ToList()
        {
            var mapping = TableMapper.GetTableMapping(_dao, typeof(T));
            var command = _dao.Provider.CreateSelectAllCommand(mapping, null);
            return _dao.QueryEntities<T>(command.CommandText + WhereString + OrderByStr, Parameter);
        }

        public IList<T> ToPageList(int pageIndex, int pageSize, out int totalRowCount)
        {
            var mapping = TableMapper.GetTableMapping(_dao, typeof(T));
            var command = _dao.Provider.CreateSelectAllCommand(mapping, null);
            return _dao.QueryEntitiesByPage<T>(command.CommandText + WhereString, pageIndex, pageSize, OrderByStr, out totalRowCount, Parameter);
        }

        /// <summary>
        /// 按照DESC排序
        /// </summary>
        /// <param name="func"></param>
        public DaoQueryable<T> OrderByDescending(Expression<Func<T, object>> func)
        {
            AppendOrderStr(func.Body, "DESC");
            return this;
        }

        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="func"></param>
        public DaoQueryable<T> OrderBy(Expression<Func<T, object>> func)
        {
            AppendOrderStr(func.Body, "ASC");
            return this;
        }

        private void AppendOrderStr(Expression func, string exp)
        {
            string descFieldName = new ExpressionTool() { Parameter = Parameter, Provider = _dao.Provider, TargetType = typeof(T) }.GetSqlByExpression(func).ToString();
            descFieldName = _dao.Provider.EscapeIdentifier(descFieldName);
            if (OrderByStr.IsNullOrEmpty())
            {
                OrderByStr = " ORDER BY ";
            }
            else if (OrderByStr.IndexOf("ORDER BY", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                OrderByStr += ",";
            }

            OrderByStr += descFieldName + " " + exp;
        }
    }

    public class DaoDeleteable<T> : DaoQueryableBase<T> where T : class, new()
    {
        public DaoDeleteable(Dao dao) : base(dao)
        {

        }
        public int Execute()
        {
            var mapping = TableMapper.GetTableMapping(_dao, typeof(T));
            var command = _dao.Provider.CreateBatchDeleteCommand(mapping, null);
            return _dao.ExecuteNonQuery(command.CommandText + WhereString, Parameter);
        }

        public DaoDeleteable<T> Where(Expression<Func<T, bool>> func)
        {
            WhereInner(func);
            return this;
        }
    }

    public class DaoUpdateable<T> : DaoQueryableBase<T> where T : class, new()
    {
        private object _updateParameter;
        public DaoUpdateable(Dao dao, object parameter) : base(dao)
        {
            _updateParameter = parameter;
        }

        public int Execute()
        {
            var mapping = TableMapper.GetTableMapping(_dao, typeof(T));
            var command = _dao.Provider.CreateBatchUpdateCommand(mapping, _updateParameter, null);
            return _dao.ExecuteNonQuery(command.CommandText + WhereString, Parameter);
        }

        public DaoUpdateable<T> Where(Expression<Func<T, bool>> func)
        {
            WhereInner(func);
            return this;
        }
    }

    public class DaoQueryableBase<T> where T : class, new()
    {
        protected Dao _dao;
        public DaoQueryableBase(Dao dao)
        {
            _dao = dao;
            Parameter = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        protected string WhereString { get; set; }

        public IDictionary<string, object> Parameter { get; set; }

        protected void WhereInner(Expression<Func<T, bool>> func)
        {
            WhereString = " WHERE " + new ExpressionTool() { Parameter = Parameter, Provider = _dao.Provider, TargetType = typeof(T) }.GetSqlByExpression(func.Body);
        }
    }
}
