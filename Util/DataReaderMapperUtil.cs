using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace gpslogcamara.Util
{
    public static class DataReaderMapperUtil
    {
        public static List<T> MapToList<T>(this IDataReader reader) where T : new()
        {
            var results = new List<T>();
            var properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);

            while (reader.Read())
            {
                var obj = new T();

                foreach (var property in properties)
                {
                    if (reader.HasColumn(property.Name) && !reader.IsDBNull(reader.GetOrdinal(property.Name)))
                    {
                        var value = reader[property.Name];
                        property.SetValue(obj, value);
                    }
                }

                results.Add(obj);
            }

            return results;
        }

        // Método de extensión para verificar si una columna existe en el DataReader
        public static bool HasColumn(this IDataRecord reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
