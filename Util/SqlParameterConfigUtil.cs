using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gpslogcamara.Util
{
    internal class SqlParameterConfigUtil
    {
        public string[] ParameterNames { get; }
        public object?[] Values { get; }

        public SqlParameterConfigUtil(string[] parameterNames, object?[] values)
        {
            ParameterNames = parameterNames;
            Values = values;
        }

        public void AddToCommand(SqlCommand command)
        {
            for (int i = 0; i < ParameterNames.Length; i++)
            {
                command.Parameters.AddWithValue(ParameterNames[i], Values[i] ?? DBNull.Value);
            }
        }
    }
}
