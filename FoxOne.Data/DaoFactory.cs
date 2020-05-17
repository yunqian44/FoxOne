using System;
using System.Collections.Generic;
using System.Linq;
using FoxOne.Data.Mapping;
using FoxOne.Data.Sql;
using FoxOne.Core;
using System.Collections.Concurrent;

namespace FoxOne.Data
{
    public sealed class DaoFactory
    {


        private static ISqlSource _sqlSource;
        private static IDictionary<string, Dao> _daos;
        private static IList<IDaoProvider> _providers;
        private static IEnumerable<ISqlParameters> _parameters;
        private static IEnumerable<ISqlActionExecutor> _actionExecutors;

        static DaoFactory()
        {
            _actionExecutors = TypeHelper.GetAllImplInstance<ISqlActionExecutor>();
            _parameters = ObjectHelper.GetAllObjects<ISqlParameters>();
            _providers = TypeHelper.GetAllImplInstance<IDaoProvider>();
            _daos = new ConcurrentDictionary<string, Dao>();
            _sqlSource = new SqlSource().LoadSqls();
        }

        public static IEnumerable<ISqlParameters> Parameters
        {
            get { return _parameters; }
        }

        public static Dao GetDao(String name)
        {
            Dao dao;
            if (!_daos.TryGetValue(name, out dao))
            {
                dao = new Dao(name);
                _daos.Add(name, dao);
            }
            return dao;
        }

        public static ISqlSource GetSqlSource()
        {
            return _sqlSource;
        }

        public static IEnumerable<ISqlActionExecutor> ActionExecutors
        {
            get { return _actionExecutors; }
        }

        internal static IDaoProvider GetDaoProvider(string dbProviderName)
        {
            IDaoProvider provider = _providers.SingleOrDefault(p => p.Name.Equals(dbProviderName, StringComparison.OrdinalIgnoreCase) || p.SupportsDbProvider(dbProviderName));
            return provider;
        }
    }
}