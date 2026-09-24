using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace CarSales.IntegrationTests;

/// <summary>Verifica que cada request imprima el tiempo del caso de uso y el del request completo.</summary>
public sealed class ExecutionTimeLoggingTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory = new WebApplicationFactory<Program>()
        .WithWebHostBuilder(builder => builder.ConfigureServices(services => services.AddFakeLogging()));

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task SuccessfulRequest_PrintsTheUseCaseTimeAndTheRequestTime()
    {
        using var client = _factory.CreateClient();

        await client.GetAsync(new Uri("/api/sales/volume", UriKind.Relative));

        var messages = LoggedMessages();
        Assert.Contains(messages, m => m.StartsWith("GetTotalSalesVolumeQueryHandler.HandleAsync se ejecutó en", StringComparison.Ordinal));
        Assert.Contains(messages, m => m.StartsWith("HTTP GET /api/sales/volume respondió 200 en", StringComparison.Ordinal));
    }

    [Fact]
    public async Task FailedRequest_PrintsTheFinalStatusCode()
    {
        using var client = _factory.CreateClient();

        await client.GetAsync(new Uri("/api/sales/volume/by-center/9", UriKind.Relative));

        Assert.Contains(LoggedMessages(), m => m.StartsWith("HTTP GET /api/sales/volume/by-center/9 respondió 404 en", StringComparison.Ordinal));
    }

    private List<string> LoggedMessages() =>
        [.. _factory.Services.GetFakeLogCollector().GetSnapshot().Select(record => record.Message)];
}
