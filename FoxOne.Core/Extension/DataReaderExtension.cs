using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FoxOne.Core
{
    public static class DataReaderExtension
    {

        public static T Read<T>(this IDataReader reader) where T : class, new()
        {
            return Read<T>(reader, typeof(T));
        }

        public static T Read<T>(this IDataReader reader, Type instanceType)
        {
            if (reader.Read())
            {
                return Read<T>(reader, instanceType, GetSetterMappings(instanceType, reader));
            }
            else
            {
                return default(T);
            }
        }

        public static IList<T> ReadList<T>(this IDataReader reader) where T : class, new()
        {
            return ReadList<T>(reader, typeof(T));
        }

        public static IList<T> ReadList<T>(this IDataReader reader, Type instanceType)
            where T : class
        {
            IList<T> list = new List<T>();

            if (reader.Read())
            {
                PropertyMapping[] mappings = GetSetterMappings(instanceType, reader);

                do
                {
                    list.Add(Read<T>(reader, instanceType, mappings));
                }
                while (reader.Read());
            }
            return list;
        }

        private static T Read<T>(IDataReader reader, Type type, PropertyMapping[] mappings)
        {
            object instance = Activator.CreateInstance(type);

            foreach (PropertyMapping mapping in mappings)
            {
                FastProperty prop = mapping.Prop;

                prop.SetValue(instance, reader.GetValue(mapping.Index).ConvertToType(prop.Type));
            }
            return (T)instance;
        }

        private static PropertyMapping[] GetSetterMappings(Type type, IDataReader reader, string mappingKey = null)
        {
            PropertyMapping[] mappings = null;
            FastType reflection = FastType.Get(type);
            List<PropertyMapping> list = new List<PropertyMapping>();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                string columnName = reader.GetName(i);
                FastProperty prop = reflection.Setters.SingleOrDefault(m => MatchColumnName(m.Name, columnName));
                if (prop != null)
                {
                    list.Add(new PropertyMapping() { Prop = prop, Index = i });
                }
            }
            mappings = list.ToArray();
            return mappings;
        }

        private static bool MatchColumnName(string name, string columnName)
        {
            return columnName.Equals(name, StringComparison.OrdinalIgnoreCase) ||
                   columnName.Replace(" ", "").Replace("_", "").Equals(name, StringComparison.OrdinalIgnoreCase);
        }

        internal struct PropertyMapping
        {
            public FastProperty Prop;
            public int Index;
        }
    }
}
