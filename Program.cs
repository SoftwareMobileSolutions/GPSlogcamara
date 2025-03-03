using gpslogcamara.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
// ===============================================================
//A. ENDPOINTS Web
var webBuilder = WebApplication.CreateBuilder(args).Program_ConfigureWebServices();
var jsonOptions = Program_Web_QueriesEndPointsExtensionService.ConfigureJsonOptions();
var app = webBuilder.Build();

// Habilitar CORS antes de definir los endpoints
app.UseCors("AllowAll");

//1. Registrar Endpoints
app.Program_RegisterEndpoints(jsonOptions);

var urlQueries = webBuilder.Configuration["queriesweb"];
_ = Task.Run(() => app.Run(urlQueries));

// ===============================================================
//B. SERVICIOS DE CONSOLA
var host = Host.CreateDefaultBuilder(args)
    .Program_ConfigureConsoleServices()  //Se configura en un archivo externo
    .Build();

//7️. Iniciar tareas de consola
host.Program_StartConsoleServices();

//8. Mantener el programa en ejecución
await Task.Delay(Timeout.Infinite);

