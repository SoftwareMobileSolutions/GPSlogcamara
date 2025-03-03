using gpslogcamara.Data;
using gpslogcamara.Interfaces;
using gpslogcamara.Models;
using System.Data;
using Microsoft.Data.SqlClient;
using gpslogcamara.Util;

namespace gpslogcamara.Services
{
    public class _ModuloCamaraService : _IModuloCamara
    {
        private readonly SqlCnConfigMain _configuration;
        public _ModuloCamaraService(SqlCnConfigMain configuration)
        {
            _configuration = configuration;
        }

        public async Task<IEnumerable<camaraConfigDataModel>> ObtenerDataCamara(int op, string? accessToken = null, string? account = null, string? refreshToken = null, int? expiresIn = null, string? time = null, string? appKey = null)
        {
            IEnumerable<camaraConfigDataModel> data;
            using (var conn = new SqlConnection(_configuration.Value))
            {
                string query = @"EXEC fleet_core_getTokenCamara @accessToken, @account, @refreshToken, @expiresIn, @time, @appKey, @op";

                if (conn.State == ConnectionState.Closed)
                {
                    await conn.OpenAsync();
                }

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.Text;

                    // Crear configuración de parámetros
                    var parameterConfig = new SqlParameterConfigUtil(
                        new[] { "@accessToken", "@account", "@refreshToken", "@expiresIn", "@time", "@appKey", "@op" },
                        new object?[] { accessToken, account, refreshToken, expiresIn, time, appKey, op }
                    );

                    // Agregar parámetros al comando
                    parameterConfig.AddToCommand(cmd);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        data = reader.MapToList<camaraConfigDataModel>();
                    }
                }
            }
            return data;
        }
    }
}
