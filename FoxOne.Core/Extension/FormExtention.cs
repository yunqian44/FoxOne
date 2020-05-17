using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace FoxOne.Core
{
    public static class FormExtention
    {
        public static T ToEntity<T>(this NameValueCollection source) where T : class, new()
        {
            var type = typeof(T);
            FastType fastType = FastType.Get(type);
            var instance = Activator.CreateInstance(type);
            foreach (var p in fastType.Setters)
            {
                if (p.Name.IsNullOrEmpty()) continue;
                if (source.AllKeys.Contains(p.Name))
                {
                    p.SetValue(instance, source[p.Name].ConvertToType(p.Type));
                }
                GetCompositeProperty(source, instance, p, false);
            }
            return instance as T;
        }

        private static void GetCompositeProperty(NameValueCollection source, object instance, FastProperty p, bool needKh, string prefix = "")
        {
            if (p.Type.IsGenericType)
            {
                var pType = p.Type.GetGenericArguments()[0];
                var t = typeof(List<>);
                t = t.MakeGenericType(pType);
                var listInstance = Activator.CreateInstance(t);
                int i = 0;
                string keyTemplate = needKh ? "{0}[{1}][{2}]" : "{0}{1}[{2}]";
                while (source.AllKeys.Any(o => o.StartsWith(keyTemplate.FormatTo(prefix, p.Name, i))))
                {
                    string key = keyTemplate.FormatTo(prefix, p.Name, i);
                    object pInstance = null;
                    if (pType.IsInterface)
                    {
                        pInstance = ObjectHelper.GetObject(pType);
                    }
                    else
                    {
                        pInstance = Activator.CreateInstance(pType);
                    }
                    var pInstanceType = FastType.Get(pInstance.GetType());
                    foreach (var pp in pInstanceType.Setters)
                    {
                        if (pp.Type == typeof(string) || pp.Type.IsValueType)
                        {
                            string tempKey = "{0}[{1}]".FormatTo(key, pp.Name);
                            if (source.AllKeys.Contains(tempKey))
                            {
                                pp.SetValue(pInstance, source[tempKey].ConvertToType(pp.Type));
                            }
                        }
                        else
                        {
                            GetCompositeProperty(source, pInstance, pp, true, key);
                        }
                    }
                    var add = listInstance.GetType().GetMethod("Add");
                    add.Invoke(listInstance, new object[] { pInstance });
                    i++;
                }
                p.SetValue(instance, listInstance.ConvertToType(p.Type));
            }
        }
    }
}
