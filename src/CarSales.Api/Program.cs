using CarSales.Api.Diagnostics;
using CarSales.Api.ErrorHandling;
using CarSales.Application;
using CarSales.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Logs en una sola línea con la hora, para leer fácil en la consola los tiempos de ejecución.
builder.Logging.AddSimpleConsole(options =>
{
    options.SingleLine = true;
    options.TimestampFormat = "HH:mm:ss ";
});

// Cada capa registra sus propias dependencias; la API solo las compone.
builder.Services
    .AddApplication()
    .AddInfrastructure();

builder.Services.AddControllers();
builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = ProblemDetailsTitles.Apply);
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddHealthChecks();
builder.Services.AddOpenApi(options => options.AddDocumentTransformer((document, _, _) =>
{
    document.Info.Title = "Car Sales API";
    document.Info.Description = "Ventas de una fábrica de autos: registrar ventas y consultar el volumen total, " +
        "el volumen por centro de distribución y el porcentaje de unidades de cada modelo por centro.";
    return Task.CompletedTask;
}));

var app = builder.Build();

// Primero en el pipeline: así mide el request entero y registra el código final, incluso en los errores.
app.UseMiddleware<RequestTimingMiddleware>();
app.UseExceptionHandler();

// Las respuestas de error sin cuerpo (ruta inexistente, método no permitido) también salen como ProblemDetails.
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    // Documento OpenAPI en /openapi/v1.json y Swagger UI en /swagger para probar los servicios.
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Car Sales API v1"));
    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
}

app.MapControllers();

// Sonda de salud para el orquestador (liveness/readiness en Kubernetes): responde 200 "Healthy".
app.MapHealthChecks("/health");

await app.RunAsync();
