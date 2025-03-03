using gpslogcamara.Models;

namespace gpslogcamara.Interfaces
{
    public interface IDatosGPSCamara
    {
        Task<IEnumerable<mensajesModel>> SetGPSCamaraData(string data);
    }
}
