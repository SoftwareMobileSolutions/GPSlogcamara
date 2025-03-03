using gpslogcamara.Data;
using gpslogcamara.Interfaces;
using gpslogcamara.Models;
using gpslogcamara.Util;
using Microsoft.Data.SqlClient;
using System.Data;

namespace gpslogcamara.Services
{
    public class CamaraLogGPSService : ICamaraLogGPS
    {
        private readonly SqlCnConfigMain _configuration;
        public CamaraLogGPSService(SqlCnConfigMain configuration)
        {
            _configuration = configuration;
        }

        //GPS
        public async Task<IEnumerable<CamaraLogGPSModel>> getCamaraGPSLog()
        {
            IEnumerable<CamaraLogGPSModel> data;
            using (var conn = new SqlConnection(_configuration.Value))
            {
                // string query = @"EXEC Fleet_Core_ObtenerDataGPSLogCamara";
                string query = @"EXEC Fleet_Core_ObtenerDataGPSLogCamara_ServerIP";
                if (conn.State == ConnectionState.Closed)
                {
                    await conn.OpenAsync();
                }
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        data = reader.MapToList<CamaraLogGPSModel>();
                    }
                }
                if (conn.State == ConnectionState.Open) { await conn.CloseAsync(); }
            }
            return data;
        }
        public async Task<IEnumerable<mensajesModel>> UpdateGPSCamaraData(Int64 idlogcam)
        {
            IEnumerable<mensajesModel> result;
            using (var conn = new SqlConnection(_configuration.Value))
            {
                string query = @"EXEC Fleet_Core_ActualizarDataGPSLogCamaraAfterSendUDP @idlogcam";

                if (conn.State == ConnectionState.Closed) { await conn.OpenAsync(); }

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    var parameterConfig = new SqlParameterConfigUtil(
                        new[] { "@idlogcam" },
                        new object?[] { idlogcam }
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

        //Alertas
        public async Task<IEnumerable<CamaraLogGPSModel>> getCamaraAlertasLog()
        {
            IEnumerable<CamaraLogGPSModel> data;
            using (var conn = new SqlConnection(_configuration.Value))
            {
                //string query = @"EXEC Fleet_Core_ObtenerDataAlertasLogCamara";
                string query = @"EXEC Fleet_Core_ObtenerDataAlertasLogCamara_serverip";
                if (conn.State == ConnectionState.Closed)
                {
                    await conn.OpenAsync();
                }
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        data = reader.MapToList<CamaraLogGPSModel>();
                    }
                }
                if (conn.State == ConnectionState.Open) { await conn.CloseAsync(); }
            }
            return data;
        }

        public async Task<IEnumerable<mensajesModel>> UpdateGPSAlertasData(Int64 idlogcam)
        {
            IEnumerable<mensajesModel> result;
            using (var conn = new SqlConnection(_configuration.Value))
            {
                string query = @"EXEC Fleet_Core_ActualizarDataAlertasLogCamaraAfterSendUDP @idlogcam";

                if (conn.State == ConnectionState.Closed) { await conn.OpenAsync(); }

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    var parameterConfig = new SqlParameterConfigUtil(
                        new[] { "@idlogcam" },
                        new object?[] { idlogcam }
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
