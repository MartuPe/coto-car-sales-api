using System.Net;
using System.Net.Http.Json;
using CarSales.Api.ErrorHandling;
using CarSales.Application.Common;
using CarSales.Application.Reports.GetModelShareByCenter;
using CarSales.Application.Sales.RegisterSale;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace CarSales.IntegrationTests;

/// <summary>
/// Prueba los servicios REST de punta a punta: HTTP → controller → caso de uso → repositorio en memoria.
/// xUnit crea una instancia de la clase por test, así cada test levanta su propia API con los datos
/// mockeados originales y los POST de un test no afectan a los demás.
/// </summary>
public sealed class SalesApiTests : IDisposable
{
    // Totales de los datos mockeados: 50 unidades, 6 de ellas Sport (6 x 1.274 de impuesto).
    private static readonly SalesVolumeDto _mockedTotal = new(50, 542_200m, 7_644m, 549_844m);

    private readonly WebApplicationFactory<Program> _factory = new();
    private readonly HttpClient _client;

    public SalesApiTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task GetTotalVolume_ReturnsTheMockedSales()
    {
        var volume = await _client.GetFromJsonAsync<SalesVolumeDto>("/api/sales/volume");

        Assert.Equal(_mockedTotal, volume);
    }

    [Fact]
    public async Task RegisterSale_ReturnsCreatedAndTheReportsIncludeIt()
    {
        var response = await _client.PostAsJsonAsync("/api/sales", new { distributionCenterId = 1, model = "sport", quantity = 2 });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var sale = await response.Content.ReadFromJsonAsync<SaleDto>();
        Assert.NotNull(sale);
        Assert.Equal("Centro Buenos Aires", sale.DistributionCenterName);
        Assert.Equal("Sport", sale.Model);
        Assert.Equal(36_400m, sale.NetAmount);
        Assert.Equal(2_548m, sale.TaxAmount);
        Assert.Equal(38_948m, sale.TotalAmount);

        var volume = await _client.GetFromJsonAsync<SalesVolumeDto>("/api/sales/volume");
        Assert.Equal(new SalesVolumeDto(52, 578_600m, 10_192m, 588_792m), volume);
    }

    [Fact]
    public async Task RegisterSale_InvalidFields_Returns400WithAnErrorPerField()
    {
        var response = await _client.PostAsJsonAsync("/api/sales", new { distributionCenterId = 0, model = "", quantity = 0 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal("Datos inválidos", problem.Title);
        Assert.Equal(["DistributionCenterId", "Model", "Quantity"], problem.Errors.Keys.Order());
        Assert.Equal("La cantidad tiene que estar entre 1 y 1000.", Assert.Single(problem.Errors["Quantity"]));
    }

    [Fact]
    public async Task RegisterSale_MissingField_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/sales", new { distributionCenterId = 1, model = "Sedan" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(9, "Sedan", "No existe el centro de distribución 9. Centros válidos: 1, 2, 3, 4.")]
    [InlineData(1, "Coupe", "No existe el modelo 'Coupe'. Modelos válidos: Sedan, SUV, Offroad, Sport.")]
    public async Task RegisterSale_UnknownCenterOrModel_Returns400ListingTheValidOnes(
        int distributionCenterId,
        string model,
        string expectedDetail)
    {
        var response = await _client.PostAsJsonAsync("/api/sales", new { distributionCenterId, model, quantity = 1 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("Datos inválidos", problem?.Title);
        Assert.Equal(expectedDetail, problem?.Detail);

        Assert.Equal(_mockedTotal, await _client.GetFromJsonAsync<SalesVolumeDto>("/api/sales/volume"));
    }

    [Theory]
    [InlineData("GET", "/api/sales/no-existe", HttpStatusCode.NotFound, "Recurso no encontrado")]
    [InlineData("GET", "/api/sales/volume/by-center/abc", HttpStatusCode.NotFound, "Recurso no encontrado")]
    [InlineData("DELETE", "/api/sales/volume", HttpStatusCode.MethodNotAllowed, "Método no permitido")]
    public async Task ErrorsWithoutBody_ReturnProblemDetailsInSpanish(
        string method,
        string url,
        HttpStatusCode expectedStatus,
        string expectedTitle)
    {
        using var request = new HttpRequestMessage(new HttpMethod(method), new Uri(url, UriKind.Relative));

        var response = await _client.SendAsync(request);

        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(expectedTitle, (await response.Content.ReadFromJsonAsync<ProblemDetails>())?.Title);
    }

    [Fact]
    public async Task RegisterSale_UnsupportedContentType_Returns415InSpanish()
    {
        using var content = new StringContent("hola");

        var response = await _client.PostAsync(new Uri("/api/sales", UriKind.Relative), content);

        Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
        Assert.Equal("Tipo de contenido no soportado", (await response.Content.ReadFromJsonAsync<ProblemDetails>())?.Title);
    }

    [Fact]
    public async Task GetVolumeByCenter_ReturnsTheFourCenters()
    {
        var centers = await _client.GetFromJsonAsync<List<CenterSalesVolumeDto>>("/api/sales/volume/by-center");

        Assert.Equal(
            [
                new CenterSalesVolumeDto(1, "Centro Buenos Aires", new(15, 145_200m, 1_274m, 146_474m)),
                new CenterSalesVolumeDto(2, "Centro Córdoba", new(12, 137_400m, 2_548m, 139_948m)),
                new CenterSalesVolumeDto(3, "Centro Rosario", new(11, 121_900m, 2_548m, 124_448m)),
                new CenterSalesVolumeDto(4, "Centro Mendoza", new(12, 137_700m, 1_274m, 138_974m)),
            ],
            centers);
    }

    [Fact]
    public async Task GetCenterVolume_ExistingCenter_ReturnsItsVolume()
    {
        var center = await _client.GetFromJsonAsync<CenterSalesVolumeDto>("/api/sales/volume/by-center/3");

        Assert.Equal(new CenterSalesVolumeDto(3, "Centro Rosario", new(11, 121_900m, 2_548m, 124_448m)), center);
    }

    [Fact]
    public async Task GetCenterVolume_UnknownCenter_Returns404()
    {
        var response = await _client.GetAsync(new Uri("/api/sales/volume/by-center/9", UriKind.Relative));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("No existe el centro de distribución 9.", problem?.Detail);
    }

    [Fact]
    public async Task GetModelShareByCenter_ReturnsEachModelOverTheCompanyTotal()
    {
        var report = await _client.GetFromJsonAsync<ModelShareReportDto>("/api/sales/model-share-by-center");

        Assert.NotNull(report);
        Assert.Equal(50, report.TotalUnits);
        Assert.Equal(4, report.Centers.Count);
        Assert.Equal(
            [new("Sedan", 8, 16m), new("SUV", 4, 8m), new("Offroad", 2, 4m), new("Sport", 1, 2m)],
            report.Centers[0].Models);
        Assert.Equal(100m, report.Centers.SelectMany(c => c.Models).Sum(m => m.Percentage));
    }

    [Fact]
    public async Task OpenApi_DocumentsTheServicesAndTheRootRedirectsToSwagger()
    {
        var document = await _client.GetStringAsync(new Uri("/openapi/v1.json", UriKind.Relative));
        Assert.Contains("/api/sales/model-share-by-center", document, StringComparison.Ordinal);

        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var response = await client.GetAsync(new Uri("/", UriKind.Relative));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/swagger", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Health_ReturnsHealthy()
    {
        var response = await _client.GetAsync(new Uri("/health", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task ExceptionHandler_LeavesUnknownErrorsToTheDefaultHandler()
    {
        var handler = new ApiExceptionHandler(_factory.Services.GetRequiredService<IProblemDetailsService>());

        var handled = await handler.TryHandleAsync(
            new DefaultHttpContext(),
            new InvalidOperationException("error inesperado"),
            CancellationToken.None);

        Assert.False(handled);
    }
}
