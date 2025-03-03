using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using gpslogcamara.Data;
using gpslogcamara.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text.Json.Serialization.Metadata;

//Esto es llamado desde program.cs para no estar digitando a cada momento map, mientras que webQueriesServices contiene las funciones que se llaman desde acá
namespace gpslogcamara.Services
{
    public static class Program_Web_QueriesEndPointsExtensionService
    {

        public static WebApplicationBuilder Program_ConfigureWebServices(this WebApplicationBuilder webBuilder)
        {
            webBuilder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            //1️ Registrar SqlCnConfigMain para las conexiones web
            webBuilder.Services.AddScoped<SqlCnConfigMain>(provider =>
            {
                string connectionString = provider.GetRequiredService<IConfiguration>().GetConnectionString("conexion")!;
                return new SqlCnConfigMain(connectionString);
            });

            //2 Registrar las interfaces antes que los servicios que las usan
            webBuilder.Services.AddScoped<_IModuloCamara, _ModuloCamaraService>();
            webBuilder.Services.AddScoped<IImeis, ImeisService>();
            webBuilder.Services.AddScoped<IDatosGPSCamara, DatosGPSCamaraService>();

            //3️Registrar los servicios que dependen de las interfaces
            webBuilder.Services.AddScoped<Console_CameraApiManagerServices>();
            webBuilder.Services.AddScoped<Web_QueriesEndPointsService>();

            //4️ Configurar CORS
            webBuilder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder => builder.AllowAnyOrigin()
                                      .AllowAnyMethod()
                                      .AllowAnyHeader());
            });

            return webBuilder;
        }

        public static JsonSerializerOptions ConfigureJsonOptions()
        {
            return new JsonSerializerOptions
            {
                TypeInfoResolver = JsonTypeInfoResolver.Combine(
                    new DefaultJsonTypeInfoResolver())
            };
        }

        public static void Program_RegisterEndpoints(this WebApplication app, JsonSerializerOptions jsonOptions)
        {
            var service = app.Services.GetRequiredService<Web_QueriesEndPointsService>();

            //1️ Obtener video en vivo**
            app.MapGet("/getliveurl", async (HttpContext context) =>
            {
                var imei = context.Request.Query["imei"].ToString();
                if (string.IsNullOrEmpty(imei))
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("El parámetro 'imei' es requerido.");
                    return;
                }

                var result = await service.GetLiveUrl(imei);
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(result, jsonOptions));
            });

            //2️ Obtener fotos o videos en un rango de tiempo**
            app.MapGet("/getvideofoto", async (HttpContext context) =>
            {
                var imei = context.Request.Query["imei"].ToString();
                var media_type = context.Request.Query["media_type"].ToString();
                var camera = context.Request.Query["camera"].ToString();
                var start_time = context.Request.Query["start_time"].ToString();
                var end_time = context.Request.Query["end_time"].ToString();
                var page_size = context.Request.Query["page_size"].ToString();

                if (string.IsNullOrEmpty(imei) || string.IsNullOrEmpty(media_type) || string.IsNullOrEmpty(camera) ||
                    string.IsNullOrEmpty(start_time) || string.IsNullOrEmpty(end_time))
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Faltan parámetros obligatorios.");
                    return;
                }

                var result = await service.getVideoFoto(imei, media_type, camera, start_time, end_time, page_size);
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(result, jsonOptions));
            });

            //3️ Obtener lista de eventos
            app.MapGet("/getlistadeeventos", async (HttpContext context) =>
            {
                var imei = context.Request.Query["imei"].ToString();
                var begin_time = context.Request.Query["begin_time"].ToString();
                var end_time = context.Request.Query["end_time"].ToString();
                var alertTypeId = context.Request.Query["alertTypeId"].ToString();

                if (string.IsNullOrEmpty(imei))
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("El parámetro 'imei' es requerido.");
                    return;
                }

                var result = await service.getListaEventos(imei, begin_time, end_time, alertTypeId);
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(result, jsonOptions));
            });
        }
    }
}
