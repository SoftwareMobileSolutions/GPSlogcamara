using gpslogcamara.Models;


namespace gpslogcamara.Interfaces
{
    public interface IImeis
    {
        Task<IEnumerable<ImeisModel>> ObtenerImeis();
    }
}
