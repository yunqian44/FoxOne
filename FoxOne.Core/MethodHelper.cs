using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

namespace FoxOne.Core
{
    public static class MethodHelper
    {
        public static IDictionary<string, object> GetParams(HttpRequest _request)
        {
            var _params = new Dictionary<string, object>();
            foreach (var key in _request.Form.Keys)
            {
                if (null != key)
                {
                    string name = key.ToString();
                    _params.Add(name, _request.Form[name]);
                }
            }
            foreach (var key in _request.QueryString.Keys)
            {
                if (null != key)
                {
                    string name = key.ToString();
                    _params.Add(name, _request.QueryString[name]);
                }
            }

            if (_request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase)
                && (!string.IsNullOrEmpty(_request.ContentType)
                    && _request.ContentType.ToLower().StartsWith("application/json")))
            {
                using (var sr = new StreamReader(_request.InputStream,
                                                 _request.ContentEncoding))
                {
                    var jsonParams = JSONHelper.Deserialize<Dictionary<string, object>>(sr.ReadToEnd());

                    foreach (var item in jsonParams)
                    {
                        _params.Add(item.Key, item.Value);
                    }
                }
            }
            return _params;
        }

        public static object[] ParamsMatch(MethodInfo methodInfo, IDictionary<string, object> parameters)
        {
            ParameterInfo[] paramInfos = methodInfo.GetParameters();

            object[] paramsArray = new object[paramInfos.Length];

            foreach (ParameterInfo parameterInfo in paramInfos)
            {
                if (parameterInfo.IsOut || parameterInfo.IsRetval)
                {
                    throw new NotSupportedException(string.Format("Out param not supported in method {0}.", methodInfo.Name));
                }

                string paramName = parameterInfo.Name;
                Type paramType = parameterInfo.ParameterType;
                object paramValue;

                try
                {
                    if (TryMatch(paramName, paramType, parameters, out paramValue))
                    {
                        //名称匹配，进行对象转换
                        paramsArray[parameterInfo.Position] = paramValue;
                    }
                    else if (paramType.Equals(typeof(IDictionary<string, object>)))
                    {
                        //IDictionary类型的参数如果没有匹配的名称，那么使用整个rawParameters TODO : 是否忽略非简单类型的参数
                        paramsArray[parameterInfo.Position] = parameters;
                    }
                    else
                    {

                        if (CreateDefaultValue(paramType, out paramValue))
                        {
                            paramsArray[parameterInfo.Position] = paramValue;
                        }
                    }
                }
                catch
                {

                }
            }

            return paramsArray;
        }

        private static bool TryMatch(string name, Type type, IDictionary<string, object> parameters, out object value)
        {
            if (parameters.TryGetValue(name, out value))
            {
                //值为空或者类型匹配，直接绑定
                if (null == value || type.IsAssignableFrom(value.GetType()))
                {
                    return true;
                }
                else if (type.IsClass)
                {
                    value = parameters.ToEntity(type);
                }
                else if (type.IsArray)
                {
                    value = ConvertToArrayObject(name, type, value);
                }
                else
                {
                    value = value.ConvertToType(type);
                }
                return true;
            }
            return false;
        }

        private static object ConvertToArrayObject(string name, Type type, object value)
        {
            if (null == value)
            {
                return Array.CreateInstance(type.GetElementType(), 0);
            }
            else if (value.GetType().IsArray)
            {
                if (type.IsAssignableFrom(value.GetType().GetElementType()))
                {
                    return value;
                }
                else
                {
                    throw new Exception(string.Format("Array Type Expected '{0}' But '{1}' of Param '{2}'",
                                      type.FullName, value.GetType().GetElementType().FullName, name));
                }
            }
            else
            {
                object[] values = value is string ? ((string)value).Split(',') : new object[] { value.ToString() };

                Array array = Array.CreateInstance(type.GetElementType(), values.Length);

                for (int i = 0; i < values.Length; i++)
                {
                    object obj = values[i];
                    array.SetValue(obj.ConvertToType(type.GetElementType()), i);
                }

                return array;
            }
        }

        private static bool CreateDefaultValue(Type type, out object value)
        {
            if (type.IsArray)
            {
                value = Array.CreateInstance(type.GetElementType(), 0);

                return true;
            }
            value = null;
            return false;
        }
    }
}
