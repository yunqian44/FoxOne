using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using FoxOne.Core;
namespace FoxOne.Business
{
    [DisplayName("引用数据源")]
    public class ReferenceDataSource:ListDataSourceBase
    {
        public string DataSourcePageId { get; set; }

        public string DataSourceId { get; set; }

        private IListDataSource _dataSource;
        private IListDataSource DataSource
        {
            get
            {
                if (_dataSource == null)
                {
                    if (DataSourcePageId.IsNullOrEmpty())
                    {
                        throw new ArgumentNullException("DataSourcePageId");
                    }
                    if (DataSourceId.IsNullOrEmpty())
                    {
                        throw new ArgumentOutOfRangeException("DataSourceId");
                    }
                    var page = PageBuilder.BuildPage(DataSourcePageId);
                    if (page == null)
                    {
                        throw new ArgumentOutOfRangeException("DataSourcePageId");
                    }
                    _dataSource = page.FindControl(DataSourceId) as IListDataSource;
                    if (_dataSource == null)
                    {
                        throw new FoxOneException("指定的DataSourceId非数据源");
                    }
                    _dataSource.Parameter = Parameter;
                    _dataSource.SortExpression = SortExpression;
                }
                return _dataSource;
            }
        }

        public override IEnumerable<IDictionary<string, object>> GetList()
        {
            return DataSource.GetList();
        }

        public override IEnumerable<IDictionary<string, object>> GetList(int pageIndex, int pageSize, out int recordCount)
        {
            return DataSource.GetList(pageIndex, pageSize, out recordCount);
        }
    }
}
