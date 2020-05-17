
using System.Collections.Generic;
using FoxOne.Data.Sql;
using System.Data;
using System.Data.Common;
using FoxOne.Data.Mapping;

namespace FoxOne.Data
{
    /// <summary>
    /// 数据访问提供者接口
    /// </summary>
    public interface IDaoProvider
    {
        /// <summary>
        /// Provider的名称，一般此属性直接映射ConnectionString中的ProviderName
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 返回此Provider是否支持某个DbProvider
        /// </summary>
        /// <param name="dbProviderName"></param>
        /// <returns></returns>
        bool SupportsDbProvider(string dbProviderName);

        /// <summary>
        /// 是否支持命名参数
        /// </summary>
        bool SupportsNamedParameter { get; }

        /// <summary>
        /// 命名参数中一个参数多次出现的情形，是否需要一一严格Push给CommandParameter
        /// </summary>
        bool NamedParameterMustOneByOne { get; }

        /// <summary>
        /// 数据访问提供者的命名参数格式，命名参数前缀 + {0}
        /// </summary>
        /// <remarks>
        /// 例如Sql Server对应的格式为 "@{0}"，Oracle对应的格式为:{0}
        /// </remarks>
        string NamedParameterFormat { get; }

        /// <summary>
        /// 把指定的SQL语句进行自动包装为分页查询的语句
        /// </summary>
        string WrapPageSql(string sql, string orderClause, int startRowIndex, int rowCount, out IDictionary<string, object> pageParam);

        /// <summary>
        /// 把指定的SQL语句进行自动包装为查询总数的语句
        /// </summary>
        string WrapCountSql(string sql);

        /// <summary>
        /// 对SQL文本进行转义处理
        /// </summary>
        string EscapeText(string text);

        /// <summary>
        /// 对SQL中的命名参数进行转义处理
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        string EscapeParam(string name);

        /// <summary>
        /// 对字段进行转义处理
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        string EscapeIdentifier(string name);

        /// <summary>
        /// 对SQL语句中Like后面%value%中的value进行转义处理
        /// </summary>
        /// <param name="value"></param>
        /// <returns>转义后的值</returns>
        string EscapeLikeParamValue(string value);

        /// <summary>
        /// 包装分页查询时Begin变量的名称
        /// </summary>
        /// <returns></returns>
        string PageParamNameBegin { get; }

        /// <summary>
        /// 包装分页查询是End变量的名称
        /// </summary>
        /// <returns></returns>
        string PageParamNameEnd { get; }

        DbCommand CreateDbCommand(string sql);

        DbConnection CreateDbConnection(string connectionString);

        IList<Table> ReadTables(Dao dao);

        string CreateTableCommand(Table mapping);

        string GetDropTableCommand(Table table);

        string CreateSelectStatement(TableMapping mapping);

        string CreateInsertStatement(TableMapping mapping);

        string CreateUpdateStatement(TableMapping mapping);

        string CreateDeleteStatement(TableMapping mapping);

        string CreateGetOneStatement(TableMapping mapping);

        ISqlCommand CreateSelectCommand(TableMapping mapping, object parameters);

        ISqlCommand CreateSelectCountCommand(TableMapping mapping, object parameters);

        ISqlCommand CreateSelectAllCommand(TableMapping mapping, object parameter);

        ISqlCommand CreateInsertCommand(TableMapping mapping, object parameters);

        ISqlCommand CreateUpdateCommand(TableMapping mapping, object parameters);

        ISqlCommand CreateUpdateCommand(TableMapping mapping, object parameters, string[] fields, bool inclusive);

        ISqlCommand CreateDeleteCommand(TableMapping mapping, object parameters);


        ISqlCommand CreateBatchUpdateCommand(TableMapping mapping, object parameters, object whereParameter);

        ISqlCommand CreateBatchDeleteCommand(TableMapping mapping, object whereParameter);
    }
}