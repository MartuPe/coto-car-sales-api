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
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddOpenApi();

var app = builder.Build();

// Primero en el pipeline: así mide el request entero y registra el código final, incluso en los errores.
app.UseMiddleware<RequestTimingMiddleware>();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    // Documento OpenAPI en /openapi/v1.json y Swagger UI en /swagger para probar los servicios.
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Car Sales API v1"));
    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
}

app.MapControllers();

await app.RunAsync();
