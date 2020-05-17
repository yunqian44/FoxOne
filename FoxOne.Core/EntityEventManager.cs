using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FoxOne.Core
{
    public static class EntityEventManager
    {
        private static IDictionary<string, Func<object, bool>> EntityEventList = new Dictionary<string, Func<object, bool>>();
        private const string KeyTemplate = "{0}_{1}_{2}";
        public static bool RaiseEvent<TEntity>(EventStep step, EventType type, TEntity o)
        {
            string key = KeyTemplate.FormatTo(step.ToString(), type.ToString(), o.GetType().FullName);
            Logger.Debug("Raise Event:{0}".FormatTo(key));
            if (EntityEventList.ContainsKey(key))
            {
                return EntityEventList[key](o);
            }
            return true;
        }

        public static void RegisterEvent<TEntity>(EventStep step, EventType type, Func<object, bool> predicate)
        {
            if (predicate != null)
            {
                var tfromType = ObjectHelper.GetRegisterType<TEntity>();
                string key = KeyTemplate.FormatTo(step.ToString(), type.ToString(), tfromType.FullName);
                Logger.Debug("Register Event:{0}".FormatTo(key));
                EntityEventList.Add(key, predicate);
            }
        }
    }

    public enum EventType
    {
        Insert,
        Update,
        Delete
    }

    public enum EventStep
    {
        Before,
        After
    }
}
