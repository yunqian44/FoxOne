using FoxOne.Core;
using FoxOne.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FoxOne.Business
{
    public static class DBContext<TEntity> where TEntity : class, IEntity, new()
    {
        public static DaoQueryable<TEntity> Instance
    {
        get
        {
            return new DaoQueryable<TEntity>(Dao.Get());
        }
    }

    public static bool ClearCache()
    {
        var type = ObjectHelper.GetRegisterType<TEntity>();
        if (CacheHelper.Remove(type.FullName))
        {
            Logger.Info("清除缓存：{0}", type.FullName);
            return true;
        }
        return false;
    }

    private static IService<TEntity> _service = null;
    private static IService<TEntity> Service
    {
        get
        {
            return _service ?? (_service = ObjectHelper.GetObject<IService<TEntity>>());
        }
    }

    public static bool Insert(TEntity item)
    {
        var result = Service.Insert(item) > 0;
        if (result)
        {
            ClearCache();
        }
        return result;
    }

    public static bool Update(TEntity item)
    {
        var result = Service.Update(item) > 0;
        if (result)
        {
            ClearCache();
        }
        return result;
    }

    public static bool Delete(object item)
    {
        var result = Service.Delete(item) > 0;
        if (result)
        {
            ClearCache();
        }
        return result;
    }

    public static TEntity Get(string id)
    {
        return Service.Get(id);
    }
}


}
