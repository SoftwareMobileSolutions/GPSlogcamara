using gpslogcamara.Data;
using gpslogcamara.Interfaces;
using gpslogcamara.Models;
using System.Data;
using Microsoft.Data.SqlClient;
using gpslogcamara.Util;
namespace gpslogcamara.Services
{
    public class ImeisService : IImeis
    {
        private readonly SqlCnConfigMain _configuration;
        public ImeisService(SqlCnConfigMain configuration)
        {
            _configuration = configuration;
        }

        public async Task<IEnumerable<ImeisModel>> ObtenerImeis()
        {
            IEnumerable<ImeisModel> data;
            using (var conn = new SqlConnection(_configuration.Value))
            {
                string query = @"EXEC Fleet_Core_obtenerImeisCamaraGPSLog";
                if (conn.State == ConnectionState.Closed)
                {
                    await conn.OpenAsync();
                }
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        data = reader.MapToList<ImeisModel>();// Utilizamos la función auxiliar para mapear los datos
                    }// Aquí se cierra el SqlDataReader
                }// Aquí se cierra el SqlCommand
                if (conn.State == ConnectionState.Open) { await conn.CloseAsync(); }
            }// Aquí se cierra el SqlConnection
           
            return data;
        }
    }
}
