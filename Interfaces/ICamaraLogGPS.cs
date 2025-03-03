using gpslogcamara.Models;

namespace gpslogcamara.Interfaces
{
    public interface ICamaraLogGPS
    {
        Task<IEnumerable<CamaraLogGPSModel>> getCamaraGPSLog();
        Task<IEnumerable<mensajesModel>> UpdateGPSCamaraData(Int64 idlogcam);

        Task<IEnumerable<CamaraLogGPSModel>> getCamaraAlertasLog();
        Task<IEnumerable<mensajesModel>> UpdateGPSAlertasData(Int64 idlogcam);
    }
}

