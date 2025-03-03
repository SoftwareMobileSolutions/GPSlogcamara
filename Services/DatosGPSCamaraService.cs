using gpslogcamara.Data;
using gpslogcamara.Interfaces;
using gpslogcamara.Models;
using gpslogcamara.Util;
using Microsoft.Data.SqlClient;
using System.Data;


namespace gpslogcamara.Services
{
    public class DatosGPSCamaraService : IDatosGPSCamara
    {
        private readonly SqlCnConfigMain _configuration;
        public DatosGPSCamaraService(SqlCnConfigMain configuration)
        {
            _configuration = configuration;
        }

        public async Task<IEnumerable<mensajesModel>> SetGPSCamaraData(string data)
        {
            IEnumerable<mensajesModel> result;
            using (var conn = new SqlConnection(_configuration.Value))
            {
                string query = @"EXEC Fleet_Core_InsertarDatosGPSCamara @data";

                if (conn.State == ConnectionState.Closed) { await conn.OpenAsync(); }

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    var parameterConfig = new SqlParameterConfigUtil(
                        new[] { "@data" },
                        new object?[] { data }
                    );
                    parameterConfig.AddToCommand(cmd);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        result = reader.MapToList<mensajesModel>();
                    }
                }

                if (conn.State == ConnectionState.Open) { await conn.CloseAsync(); }
            }
            return result;
        }
    }
}
