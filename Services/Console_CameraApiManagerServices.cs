using gpslogcamara.Interfaces;
using gpslogcamara.Models;
using gpslogcamara.Util;
using Newtonsoft.Json;

namespace gpslogcamara.Services
{
    public class Console_CameraApiManagerServices
    {
        private readonly _IModuloCamara _IModuloCamara;
        private readonly IImeis IImeis;
        private readonly IDatosGPSCamara IDatosGPSCamara;

        public Console_CameraApiManagerServices(_IModuloCamara _IModuloCamara_, IImeis _IImeis, IDatosGPSCamara _IDatosGPSCamara)
        {
            _IModuloCamara = _IModuloCamara_;
            IImeis = _IImeis;
            IDatosGPSCamara = _IDatosGPSCamara;
        }

        public async Task<camaraConfigDataModel>? getACCtoken()
        {
            _ModuloCamaraUtil._ModuloCamara_AUTH datoscam = new _ModuloCamaraUtil._ModuloCamara_AUTH();
            _ModuloCamaraModel ds = new _ModuloCamaraModel();
            var datos = await _IModuloCamara.ObtenerDataCamara(1);//Obteniendo la data actual
            var d = datos.FirstOrDefault(); //Obtener los valores por defecto para obtener la key
            
            if (d != null)
            {
                //Fecha en la que se calcula que expirará el accesstoken
                DateTime? expiredDateToken =
                                            d.expiredDateToken != null
                                            ? DateTime.ParseExact(d.expiredDateToken, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture)
                                            : null;

                DateTime? fechaactual =
                                        d.fechaactual != null
                                        ? DateTime.ParseExact(d.fechaactual, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture)
                                        : null;

                if (d.time == null)
                {
                    ds = await datoscam.getAccessToken(d);
                    if (ds.code != 1006)
                    {
                        datos = await _IModuloCamara.ObtenerDataCamara(2, ds.result.accessToken, ds.result.account, ds.result.refreshToken, ds.result.expiresIn, ds.result.time, ds.result.appKey);
                        d = datos.FirstOrDefault();
                        d.accessToken = ds.result.accessToken;
                        d.account = ds.result.account;
                        d.refreshToken = ds.result.refreshToken;
                        d.expiresIn = ds.result.expiresIn;
                        d.time = ds.result.time;
                        d.appKey = ds.result.appKey;
                    }
                }

                if (fechaactual > expiredDateToken)
                {
                    d.method = "jimi.oauth.token.refresh";
                    ds = await datoscam.getRefreshToken(d);
                    if (ds != null)
                    {
                        if (ds.code != 1006)
                        {
                            datos = await _IModuloCamara.ObtenerDataCamara(2, ds.result.accessToken, ds.result.account, ds.result.refreshToken, ds.result.expiresIn, ds.result.time, ds.result.appKey);
                            d = datos.FirstOrDefault();
                            d.accessToken = ds.result.accessToken;
                            d.account = ds.result.account;
                            d.refreshToken = ds.result.refreshToken;
                            d.expiresIn = ds.result.expiresIn;
                            d.time = ds.result.time;
                            d.appKey = ds.result.appKey;
                        }
                    }
                }
                return d;


            } else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss] ") + "No se puede obtener la configuración de la tabla camaraConfigData en la base de datos");
                Console.ResetColor();
                return null;

            }
            
        }
        public async Task<T> processCamRequest<T>(Dictionary<string, string> parameters)
        {
            _ModuloCamaraUtil._ModuloCamara_FIRMA mf = new _ModuloCamaraUtil._ModuloCamara_FIRMA();
            _ModuloCamaraUtil._ModuloCamara_AUTH datoscam = new _ModuloCamaraUtil._ModuloCamara_AUTH();
            dynamic datos = null;
            var d = await getACCtoken();

            //Inicio Parámetros obligatorios
            parameters["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            parameters["app_key"] = d.app_key!;
            parameters["sign_method"] = "md5";
            parameters["v"] = "1.0";
            parameters["format"] = "json";
            parameters["access_token"] = d.accessToken; //Necesario para que vaya la firma de los parámetros
            //Fin Parámetros obligatorios

            string firma = mf.SignTopRequest(parameters, d.appsecret!, "md5");
            if (!string.IsNullOrEmpty(firma))
            {

                parameters["sign"] = firma; //añadiendo a los parámetros el campo firma
                datos = await datoscam.SendPostRequestAsync<T>(d.urlrequest!, parameters); // Realizar la solicitud POST
            }
            else
            {
                datos.message = DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss] ") + "No se pudo obtener la firma.";
            }
            return datos;
        }

        //GPS Log de la cámara donde obtiene la longitud y latitud
        public async Task getGpsData()
        {
            try
            {
                var imeislist = await IImeis.ObtenerImeis();
                if (imeislist.Count() > 0)
                {
                    var parameters = new Dictionary<string, string> {
                        { "imeis" ,  imeislist!.FirstOrDefault()!.imeis! },
                        { "method" , "jimi.device.location.get" }
                    };
                    var d = await processCamRequest<_ModuloCamaraModelArr>(parameters);
                    string dresult = JsonConvert.SerializeObject(d.result);
                    var data = (await IDatosGPSCamara.SetGPSCamaraData(dresult)).FirstOrDefault();

                    Console.ForegroundColor = ConsoleColor.Green;
                    if (data == null)
                    {
                        Console.WriteLine(DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss] ") + "Insertado sin respuesta del servidor...");
                    } else {
                        Console.WriteLine(DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss] ") + "CÓDIGO: " + data.code.ToString() + "|" + data.msg);
                    }
                    
                }  else {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss] ") + "No hay Imeis del tipo: decivetype = 36");
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss] ") + "Ha ocurrido un error: " + ex.Message);
            }
            Console.ResetColor();
        }

       
    }
}
