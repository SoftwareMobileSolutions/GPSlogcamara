using gpslogcamara.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace gpslogcamara.Util
{
    internal class _ModuloCamaraUtil
    {
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1); // Permite solo 1 solicitud a la vez
        private static int _pendingRequests = 0;   // Número de solicitudes en espera
        private static int _dispatchedRequests = 0; // Número de solicitudes procesadas
        private static int _processNumber = 0; // Número total de procesos lanzados

        public class _ModuloCamara_AUTH
        {
            public async Task<_ModuloCamaraModel> getRefreshToken(camaraConfigDataModel d)
            {
                _ModuloCamara_FIRMA mf = new _ModuloCamara_FIRMA();
                _ModuloCamaraModel datos = new _ModuloCamaraModel();
                var parameters = new Dictionary<string, string>
                {
                    { "method", d.method! },
                    { "access_token", d.accessToken! },
                    { "refresh_token", d.refreshToken! },
                    { "expires_in", d.expires_in! },
                    { "timestamp", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") },
                    { "app_key", d.app_key!},
                    { "sign_method", "md5"},
                    { "v", "1.0"},
                    { "format", "json"},
                };
                string firma = mf.SignTopRequest(parameters, d.appsecret!, d.sign_method!);//appsecret = "a3b937b6df064c799d1871c1e0df4751";

                if (!string.IsNullOrEmpty(firma))
                {
                    parameters["sign"] = firma; //añadiendo a los parámetros el campo firma
                    datos = await SendPostRequestAsync<_ModuloCamaraModel>(d.urlrequest!, parameters); // Realizar la solicitud POST
                }
                else
                {
                    datos.message = "No se pudo obtener la firma.";
                }
                return datos;
            }
            public async Task<_ModuloCamaraModel> getAccessToken(camaraConfigDataModel d)
            {
                _ModuloCamara_FIRMA mf = new _ModuloCamara_FIRMA();
                _ModuloCamaraModel datos = new _ModuloCamaraModel();
                // Parámetros para la solicitud
                var parameters = new Dictionary<string, string>
                {
                    { "method", d.method! },
                    { "timestamp",   DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") },/////TOMAR EN CUENTA ESTO PARA LA DIFERENCIA EN EL CONTROLLER DE 7200
                    { "app_key",  d.app_key! }, //"8FB345B8693CCD00A78D4AB901AD685E339A22A4105B6558" ;
                    { "sign_method", d.sign_method! }, //md5
                    { "v", "1.0" },
                    { "format", "json" },
                    { "user_id",  d.user_id! },//"dada_dada";
                    { "user_pwd_md5", d.passwordMD5! }, //mf.ByteToHex(mf.EncryptMD5("smsdada$25")).ToLower();
                    { "expires_in", d.expires_in! }
                };

                // Obtener la firma en MD5
                string firma = mf.SignTopRequest(parameters, d.appsecret!, d.sign_method!);//appsecret = "a3b937b6df064c799d1871c1e0df4751";

                if (!string.IsNullOrEmpty(firma))
                {
                    parameters["sign"] = firma; //añadiendo a los parámetros el campo firma
                    datos = await SendPostRequestAsync<_ModuloCamaraModel>(d.urlrequest!, parameters); // Realizar la solicitud POST
                }
                else
                {
                    datos.message = "No se pudo obtener la firma.";
                }
                return datos;
            }

            //******************TODAS LAS SOLICITUDES LAS COLOCA EN COLA PARA QUE SE EJECUTEN DE 1 EN 1 Y CONTINUA CON LA SIGUIENTE HASTA QUE SE HAYA DESPACHADO
            public async Task<T> SendPostRequestAsync<T>(string url, Dictionary<string, string> parameters)
            {
                int currentProcessNumber = Interlocked.Increment(ref _processNumber);

                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine(DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss] ") +
                                  $"PROCESO WEB {currentProcessNumber} INICIADO. EN COLA: {_pendingRequests}, DESPACHADOS: {_dispatchedRequests}");
                Console.ResetColor();

                await _semaphore.WaitAsync().ConfigureAwait(false); // Bloquea hasta que se libere

                Interlocked.Increment(ref _pendingRequests);

                try
                {
                    T datos = default!;
                    using (var client = new HttpClient())
                    {
                        var requestContent = new FormUrlEncodedContent(parameters!);
                        HttpResponseMessage response = await client.PostAsync(url, requestContent);

                        if (response.IsSuccessStatusCode)
                        {
                            string responseContent = await response.Content.ReadAsStringAsync();
                            datos = JsonConvert.DeserializeObject<T>(responseContent)!;
                        }

                        
                    }

                    return datos;
                }
                finally
                {
                    Interlocked.Decrement(ref _pendingRequests);
                    Interlocked.Increment(ref _dispatchedRequests);

                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.WriteLine(DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss] ") +
                                      $"PROCESO WEB FINALIZADO. EN COLA: {_pendingRequests}, DESPACHADOS: {_dispatchedRequests}");
                    Console.ResetColor();

                    await Task.Delay(100); // Espera 100ms antes de liberar el semáforo

                    _semaphore.Release(); // Libera el semáforo
                }
            }
        }

        public class _ModuloCamara_FIRMA
        {
            public string SignTopRequest(Dictionary<string, string> parameters, string seccode, string signMethod)
            {
                // 1: Ordenar las claves de los parámetros
                var keys = parameters.Keys.ToArray();
                Array.Sort(keys);

                // 2: Combinar todos los nombres de parámetros y valores de parámetros
                StringBuilder query = new StringBuilder();
                if (signMethod == "md5")  // Ajuste según tu constante en C#
                {
                    query.Append(seccode);
                }

                foreach (var key in keys)
                {
                    string value = parameters[key];
                    if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                    {
                        query.Append(key).Append(value);
                    }
                }

                // 3: Usar MD5/HMAC para cifrar
                byte[] bytes;
                if (signMethod == "hmac")  // Ajuste según tu constante en C#
                {
                    bytes = EncryptHMAC(query.ToString(), seccode);
                }
                else
                {
                    query.Append(seccode);
                    bytes = EncryptMD5(query.ToString());
                }

                // 4: Convertir binario a hexadecimal en mayúsculas
                return ByteToHex(bytes);
            }

            public byte[] EncryptHMAC(string data, string seccode)
            {
                try
                {
                    using (var hmac = new HMACMD5(Encoding.UTF8.GetBytes(seccode)))
                    {
                        return hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                    }
                }
                catch (Exception ex)
                {
                    throw new IOException(ex.ToString());
                }
            }

            public byte[] EncryptMD5(string data)
            {
                using (var md5 = MD5.Create())
                {
                    return md5.ComputeHash(Encoding.UTF8.GetBytes(data));
                }
            }

            public string ByteToHex(byte[] bytes)
            {
                StringBuilder hex = new StringBuilder(bytes.Length * 2);
                foreach (byte b in bytes)
                {
                    string hexValue = b.ToString("X2");
                    hex.Append(hexValue);
                }
                return hex.ToString();
            }
        }

    }
}
