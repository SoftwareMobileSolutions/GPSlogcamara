using gpslogcamara.Interfaces;
using gpslogcamara.Models;
using gpslogcamara.Services;

public class Web_QueriesEndPointsService
{
    private readonly Console_CameraApiManagerServices _cameraApiManagerServices;

    // Constructor con inyección de dependencias
    public Web_QueriesEndPointsService(_IModuloCamara moduloCamara, IImeis imeis, IDatosGPSCamara datosGPS)
    {
        _cameraApiManagerServices = new Console_CameraApiManagerServices(moduloCamara, imeis, datosGPS);
    }

    //1- VIDEO EN VIVO
    public async Task<object> GetLiveUrl(string imei)
    {
        var parameters = new Dictionary<string, string> {
            { "imei", imei },
            { "method", "jimi.device.live.page.url" }
        };

        var datos = await _cameraApiManagerServices.processCamRequest<_ModuloCamaraModel>(parameters);
        return datos;
    }


    //2- OBTENER RANGO DE FOTOS O VIDEOS || media_type = 1 -photo 2-video 3 both  camera = 1 camara frontal 2  camera trasera 3 both
    public async Task<object> getVideoFoto(string imei, string media_type, string camera, string start_time, string end_time, string page_size)
    {
        var parameters = new Dictionary<string, string> {
                { "imei" , imei},
                { "method" , "jimi.device.media.URL"},
                //{ "method" , "jimi.device.jimi.media.URL"},
                { "camera", camera},
                { "media_type", media_type},
                { "start_time", start_time},
                { "end_time", end_time}
            };
        if (page_size != null)
        {
            // parameters["start_row"] = "1";
            parameters["page_size"] = page_size;
            //parameters["page_no"] = "0";
        }

        var datos = await _cameraApiManagerServices.processCamRequest<_ModuloCamaraModelArr>(parameters);
        return datos;
    }

    //3- OBTENER LISTA DE EVENTOS
    public async Task<object> getListaEventos(string imei, string? begin_time = null, string? end_time = null, string? alertTypeId = null)
    {
        var parameters = new Dictionary<string, string> {
            { "imei" , imei},
            { "method" , "jimi.device.alarm.list" },
            { "begin_time", begin_time},
            { "end_time", end_time}
        };

        if (alertTypeId != null) { parameters["alertTypeId"] = alertTypeId; }
        if (begin_time != null) { parameters["begin_time"] = begin_time; }
        if (end_time != null) { parameters["end_time"] = end_time; }

        var datos = await _cameraApiManagerServices.processCamRequest<_ModuloCamaraModelArr>(parameters);
        return datos;
    }

}
