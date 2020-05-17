using FoxOne.Core;
using FoxOne.Data.Mapping;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Linq;
namespace FoxOne.Data.Provider
{
    public class MySqlProvider : DaoProvider
    {
        private const string ProviderName = "MySql";
        private const string NameParamFormat = "@{0}";
        private const string MySqlClientDbProvider = "MySql.Data.MySqlClient";

        public MySqlProvider()
            : base(ProviderName, NameParamFormat)
        {

        }

        public override bool SupportsDbProvider(string dbProviderName)
        {
            return MySqlClientDbProvider.Equals(dbProviderName, StringComparison.OrdinalIgnoreCase);
        }

        public override string WrapPageSql(string sql, string orderClause, int startRowIndex, int rowCount, out IDictionary<string, object> pageParam)
        {
            sql = RemoveOrderByClause(sql);
            StringBuilder pagingSelect = new StringBuilder(sql.Length + 100);

            if (!String.IsNullOrEmpty(orderClause))
            {
                orderClause = "order by " + orderClause;
            }
            else
            {
                throw new FoxOneException("Paged query must set orderBy Clause");
            }

            pagingSelect.Append(sql).Append(" ").Append(orderClause).Append(" ").Append("limit ").Append("#").Append(PageParamNameBegin).Append("#").Append(",").Append("#").Append(PageParamNameEnd).Append("#");

            pageParam = new Dictionary<string, object>(2)
                            {
                                {PageParamNameBegin, startRowIndex - 1},
                                {PageParamNameEnd, rowCount}
                            };

            return pagingSelect.ToString();
        }

        public override string EscapeLikeParamValue(string value)
        {
            return value.Replace("_", "/_").Replace("%", "/%").Replace("\\", "\\\\");
        }

        public override DbCommand CreateDbCommand(string sql)
        {
            return new MySqlCommand(sql);
        }

        public override DbConnection CreateDbConnection(string connectionString)
        {
            return new MySqlConnection(connectionString);
        }


        protected override string GetTablesSql()
        {
            return @"SELECT TABLE_SCHEMA AS `Schema`,TABLE_NAME AS `Name`
                     FROM INFORMATION_SCHEMA.TABLES 
                     WHERE (TABLE_TYPE='BASE TABLE' OR TABLE_TYPE='VIEW') and TABLE_SCHEMA = SCHEMA()";
        }

        protected override string GetColumnsSql()
        {
            return @"SELECT  
                    TABLE_SCHEMA AS `Schema`,
                    TABLE_NAME   AS `Table`,
                    column_name `Name`,
                    (case when column_comment='' then column_name else column_comment end) Comment,
                    data_type `Type`,
                    (case when IS_NULLABLE='YES' then 1 else 0 end) IsNullable,
                    (case when extra='auto_increment' then 1 else 0 end) IsAutoIncrement, 
                    character_maximum_length `Length`,
                    (case when column_key ='PRI' then 1 else 0 end) IsKey,
                    ORDINAL_POSITION `Rank`
                    FROM  information_schema.COLUMNS
                    WHERE TABLE_SCHEMA = SCHEMA()
                    ORDER BY ORDINAL_POSITION ASC ";
        }

        protected override string GetKeysSql()
        {
            return @"SELECT
                        KCU.TABLE_SCHEMA AS `Schema`,
                        KCU.TABLE_NAME   AS `Table`,
                        KCU.COLUMN_NAME  AS `Column`
                    FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE KCU
                        JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS TC
                        ON (KCU.CONSTRAINT_NAME = TC.CONSTRAINT_NAME AND KCU.TABLE_NAME = TC.TABLE_NAME AND KCU.TABLE_SCHEMA = TC.TABLE_SCHEMA)
                    WHERE TC.CONSTRAINT_TYPE = 'PRIMARY KEY'
                        and TC.TABLE_SCHEMA = SCHEMA()
                    ORDER BY ORDINAL_POSITION ASC ";
        }

        public override string EscapeIdentifier(string name)
        {
            return string.Format("`{0}`", name);
        }

        public override string CreateTableCommand(Table mapping)
        {
            string createTableSQL = "CREATE TABLE {0}({1})ENGINE=MyISAM DEFAULT CHARSET=utf8;";
            var keys = new List<string>();
            mapping.Keys.Select(o => o.Name).ForEach(o =>
            {
                keys.Add(EscapeIdentifier(o));
            });
            string keyString = string.Join(",", keys.ToArray());
            keyString = string.Format("PRIMARY KEY({0})", keyString);
            var columns = new List<string>();
            var fields = mapping.Columns;
            foreach (var field in fields)
            {
                columns.Add(GetColumnsSQL(field));
            }
            columns.Add(keyString);
            return string.Format(createTableSQL, mapping.Name, string.Join(",", columns.ToArray()));
        }

        public virtual string GetColumnsSQL(Column field)
        {
            if (field.Type.Equals("uniqueidentifier", StringComparison.CurrentCultureIgnoreCase))
            {
                field.Type = "varchar";
                field.Length = "38";
            }
            if (field.Type.Equals("nvarchar", StringComparison.CurrentCultureIgnoreCase))
            {
                field.Type = "varchar";
                field.Length = (int.Parse(field.Length) * 2).ToString();
            }
            return string.Format("{0} {1}{2} {3} {4} {5}",
                EscapeIdentifier(field.Name),
                field.Type,
                string.IsNullOrEmpty(field.Length) ? "" : "(" + field.Length + ")",
                field.IsAutoIncrement ? "AUTO_INCREMENT" : "",
                field.IsNullable ? "NULL" : "NOT NULL",
                string.IsNullOrEmpty(field.Comment) ? string.Format("COMMENT {0}", field.Comment) : ""
                );
        }

        protected override string GetSearchCondition(TableMapping mapping)
        {
            var fields = mapping.Table.Columns;
            List<string> condition = new List<string>();
            string[] likeTypes = "char|varchar|tinytext|text|mediumtext|longtext".Split('|');
            string[] dateTypes = "datetime|date|time".Split('|');
            foreach (var field in fields)
            {
                string temp = string.Empty;
                if (likeTypes.Contains(field.Type))
                {
                    temp = string.Format("\n{{? AND `{0}` like '%${0}$%' }}", field.Name);
                }
                else if (dateTypes.Contains(field.Type))
                {
                    temp = string.Format("\n{{? AND DATE_FORMAT(`{0}`,'%Y-%m-%d') = #{0}# }}", field.Name);
                }
                else
                {
                    temp = string.Format("\n{{? AND `{0}`=#{0}# }}", field.Name);
                }
                condition.Add(temp);

            }
            return string.Join(" ", condition.ToArray());
        }

        public override string GetDropTableCommand(Table table)
        {
            return string.Format("DROP TABLE {0}", table.Name);
        }
    }
}