using System.Collections.Generic;
using System.Text;

namespace FoxOne.Data.Sql
{
    public class SqlCommandBuilder
    {
        private readonly StringBuilder _sql = new StringBuilder();
        private readonly IDictionary<string, object> _params = new Dictionary<string, object>();

        internal SqlCommandBuilder()
        {
            
        }

        public StringBuilder CommandText
        {
            get { return _sql; }
        }

        public IDictionary<string, object> Params
        {
            get { return _params; }
        }

        public SqlCommandBuilder AppendCommandText(string sql)
        {
            _sql.Append(sql);
            return this;
        }

        public SqlCommandBuilder AddCommandParameter(string name, object value)
        {
            if (!_params.Keys.Contains(name))
            {
                _params.Add(name, value);
            }
            return this;
        }

        public SqlCommandBuilder AddCommandParameter(KeyValuePair<string,object> param)
        {
            AddCommandParameter(param.Key, param.Value);
            return this;
        }

        public SqlCommand ToCommand()
        {
            return new SqlCommand( _sql.ToString().Trim(), _params);
        }
    }
}