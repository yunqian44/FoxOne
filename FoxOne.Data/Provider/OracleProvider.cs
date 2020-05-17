using FoxOne.Data.Mapping;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using FoxOne.Core;
using System.Linq;
namespace FoxOne.Data.Provider
{
    internal class OracleProvider : DaoProvider
    {
        private const string ProviderName = "Oracle";
        private const string NameParamFormat = ":{0}";
        private const string DefaultOracleDbProvider = "System.Data.OracleClient";
        private const string OdpNetOracleDbProvider = "Oracle.DataAccess.Client";
        private string _oracleDriverName;

        public OracleProvider()
            : base(ProviderName, NameParamFormat)
        {

        }

        public override bool SupportsDbProvider(string dbProviderName)
        {
            if (DefaultOracleDbProvider.Equals(dbProviderName, StringComparison.OrdinalIgnoreCase))
            {
                _oracleDriverName = DefaultOracleDbProvider;
                return true;
            }
            if (OdpNetOracleDbProvider.Equals(dbProviderName, StringComparison.OrdinalIgnoreCase))
            {
                _oracleDriverName = OdpNetOracleDbProvider;
                return true;
            }
            return false;
        }

        public override string WrapPageSql(string sql, string orderClause, int startRowIndex, int rowCount, out IDictionary<string, object> pageParam)
        {
            sql = RemoveOrderByClause(sql);
            StringBuilder pagingSelect = new StringBuilder(sql.Length + 100);

            pagingSelect.Append("select * from (select * from (select row_.*, rownum rownum_ from ( ");
            pagingSelect.Append(sql);
            if (!string.IsNullOrEmpty(orderClause))
            {
                pagingSelect.Append(" Order By ").Append(orderClause);
            }
            pagingSelect.Append("\n ) row_) where rownum_ <= #").Append(PageParamNameEnd).Append("#) where rownum_ >= #")
                .Append(PageParamNameBegin).Append("#");

            pageParam = new Dictionary<string, object>(2)
                            {
                                {PageParamNameBegin, startRowIndex},
                                {PageParamNameEnd, startRowIndex + rowCount - 1}
                            };

            return pagingSelect.ToString();
        }

        public override string EscapeLikeParamValue(string value)
        {
            return value.Replace("?", @"\?").Replace("_", @"\_").Replace("%", @"\%");
        }

        public override bool NamedParameterMustOneByOne
        {
            get { return OdpNetOracleDbProvider.Equals(_oracleDriverName); }
        }

        public override DbCommand CreateDbCommand(string sql)
        {
            return new OracleCommand(sql);
        }

        public override DbConnection CreateDbConnection(string connectionString)
        {
            return new OracleConnection(connectionString);
        }


        protected override string GetTablesSql()
        {
            return @"Select TABLE_NAME AS ""Name"" From USER_TABLES
                     Union
                     Select VIEW_NAME AS ""Name"" from USER_VIEWS";
        }

        protected override string GetColumnsSql()
        {
            return @"select table_name AS ""Table"",
                            column_name AS ""Name""
                     from USER_TAB_COLS
                     order by column_id";
        }

        protected override string GetKeysSql()
        {
            return @"select uc.TABLE_NAME AS ""Table"", 
                            column_name AS ""Column""
                      from USER_CONSTRAINTS uc
                     inner join USER_CONS_COLUMNS ucc
                        on (uc.constraint_name = ucc.constraint_name and uc.table_name = ucc.table_name and uc.owner = ucc.owner)
                     where uc.constraint_type = 'P'
                       and ucc.position = 1";
        }

        public override string EscapeIdentifier(string name)
        {
            return string.Format("\"{0}\"", name);
        }

        public override string CreateTableCommand(Table mapping)
        {
            string createTableSQL = "CREATE TABLE {0}({1})";
            var keys = new List<string>();
            mapping.Keys.Select(o => o.Name).ForEach(o =>
            {
                keys.Add(EscapeIdentifier("{0}"));
            });
            var columns = new List<string>();
            var fields = mapping.Columns;
            foreach (var field in fields)
            {
                columns.Add(GetColumnsSQL(field));
            }
            string keyString = string.Join(",", keys.ToArray());
            return string.Format(createTableSQL, mapping.Name, string.Join(",", columns.ToArray()));
        }

        private string GetColumnsSQL(Column field)
        {
            if (field.Type.Equals("int", StringComparison.OrdinalIgnoreCase)
                || field.Type.Equals("decimal", StringComparison.OrdinalIgnoreCase)
                || field.Type.Equals("bit", StringComparison.OrdinalIgnoreCase))
            {
                field.Type = "number";
                field.Length = string.Empty;
            }
            if (field.Type.Equals("datetime", StringComparison.OrdinalIgnoreCase))
            {
                field.Type = "date";
                field.Length = string.Empty;
            }
            if (field.Type.Equals("text", StringComparison.OrdinalIgnoreCase))
            {
                field.Type = "varchar2";
                field.Length = "4000";
            }
            return string.Format("{0} {1}{2} {3} {4}",
                EscapeIdentifier(field.Name),
                field.Type,
                string.IsNullOrEmpty(field.Length) ? "" : "(" + field.Length + ")",
                //field.IsAutoIncrement ? "IDENTITY(1,1)" : "",
                field.IsNullable ? "NULL" : "NOT NULL",
                field.IsKey ? "PRIMARY KEY" : ""
                );
        }

        public override string GetDropTableCommand(Table table)
        {
            return "DROP TABLE {0}".FormatTo(table.Name);
        }

        protected override string GetSearchCondition(TableMapping mapping)
        {
            var fields = mapping.Table.Columns;
            List<string> condition = new List<string>();
            string[] likeTypes = "char|varchar|tinytext|text|mediumtext|longtext".Split('|');
            foreach (var field in fields)
            {
                string temp = string.Empty;
                if (likeTypes.Contains(field.Type))
                {
                    temp = string.Format("\n{{? AND \"{0}\" like '%${0}$%' }}", field.Name);
                }
                else
                {
                    temp = string.Format("\n{{? AND \"{0}\"=#{0}# }}", field.Name);
                }
                condition.Add(temp);

            }
            return string.Join(" ", condition.ToArray());
        }
    }
}