using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Globalization;
using gpslogcamara.Data;
using gpslogcamara.Interfaces;
using gpslogcamara.Models;
using Microsoft.Extensions.Configuration;

namespace gpslogcamara.Services
{
    public static class Program_Console_CameraExtensionsService
    {
        public static IHostBuilder Program_ConfigureConsoleServices(this IHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                var configuration = context.Configuration;

                // Leer valores de tiempo desde appsettings.json
                double tiempoUDPset = double.Parse(configuration["tiempoUDPset"]!, CultureInfo.InvariantCulture);
                double tiempoUDPAlertas = double.Parse(configuration["tiempoUDPAlertas"]!, CultureInfo.InvariantCulture);
                double tiempoGPSget = double.Parse(configuration["tiempoGPSget"]!, CultureInfo.InvariantCulture);

                services.AddScoped<SqlCnConfigMain>(provider =>
                {
                    string connectionString = configuration.GetConnectionString("conexion")!;
                    return new SqlCnConfigMain(connectionString);
                });

                // Para manejar la API de JIMI IOT
                services.AddScoped<_IModuloCamara, _ModuloCamaraService>();
                services.AddScoped<IImeis, ImeisService>();
                services.AddScoped<IDatosGPSCamara, DatosGPSCamaraService>();
                services.AddScoped<Console_CameraApiManagerServices>();

                // Para enviar las tramas en UDP
                services.AddScoped<ICamaraLogGPS, CamaraLogGPSService>();
                services.AddScoped<Console_CameraUDPCommManagerService>();

                // Registrar los tiempos leídos
                services.AddSingleton(new TimersConfig
                {
                    tiempoUDPset = tiempoUDPset,
                    tiempoUDPAlertas = tiempoUDPAlertas,
                    tiempoGPSget = tiempoGPSget
                });
            });
        }

        public static void Program_StartConsoleServices(this IHost host)
        {
            var timersConfig = host.Services.GetRequiredService<TimersConfig>();
            var Console_CameraApiManagerServices = host.Services.GetRequiredService<Console_CameraApiManagerServices>();
            var Console_CameraUDPCommManagerService = host.Services.GetRequiredService<Console_CameraUDPCommManagerService>();

            //1️ Obtiene datos de la API e inserta los datos de GPS
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        await Console_CameraApiManagerServices.getGpsData();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} / Error en getGpsData: {ex.Message}");
                    }
                    await Task.Delay(TimeSpan.FromMinutes(timersConfig.tiempoGPSget));
                }
            });

            //2️ Procesa datos GPS de la cámara para enviarlos a UDP
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        //Si es 0 es para ubicación GPS, si es 1 es alertas
                        await Console_CameraUDPCommManagerService.procesarTrama(0);
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} / Error en procesarTrama UDP GPS: {ex.Message}");
                        Console.ResetColor();
                    }
                    await Task.Delay(TimeSpan.FromMinutes(timersConfig.tiempoUDPset));
                }
            });

            //3️ Procesa datos de Alertas de la cámara para enviarlos a UDP
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        //Si es 0 es para ubicación GPS, si es 1 es alertas
                        await Console_CameraUDPCommManagerService.procesarTrama(1);
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} / Error en procesarTrama UDP Alarmas: {ex.Message}");
                        Console.ResetColor();
                    }
                    await Task.Delay(TimeSpan.FromMinutes(timersConfig.tiempoUDPAlertas));
                }
            });
        }
    }
}
