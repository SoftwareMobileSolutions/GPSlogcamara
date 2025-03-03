using gpslogcamara.Models;

namespace gpslogcamara.Interfaces
{
    public interface _IModuloCamara
    {
        Task<IEnumerable<camaraConfigDataModel>> ObtenerDataCamara(int op, string? accessToken = null, string? account = null, string? refreshToken = null, int? expiresIn = null, string? time = null, string? appKey = null);
    }
}
