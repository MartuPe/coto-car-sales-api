using CarSales.Application.Abstractions;
using CarSales.Application.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace CarSales.UnitTests.Application;

public class ExecutionTimingTests
{
    private readonly FakeLogger<ExecutionTimer> _logger = new();
    private readonly ExecutionTimer _timer;

    public ExecutionTimingTests()
    {
        _timer = new ExecutionTimer(_logger);
    }

    [Fact]
    public async Task TimedQueryHandler_ReturnsTheInnerResultAndPrintsTheElapsedTime()
    {
        var decorated = new TimedQueryHandler<string, int>(new LengthQueryHandler(), _timer);

        var result = await decorated.HandleAsync("sport", CancellationToken.None);

        Assert.Equal(5, result);
        var log = Assert.Single(_logger.Collector.GetSnapshot());
        Assert.Equal(LogLevel.Information, log.Level);
        Assert.Matches(@"^LengthQueryHandler\.HandleAsync se ejecutó en \d+\.\d{3} ms$", log.Message);
    }

    [Fact]
    public async Task TimedCommandHandler_ReturnsTheInnerResultAndPrintsTheElapsedTime()
    {
        var decorated = new TimedCommandHandler<string, int>(new LengthCommandHandler(), _timer);

        var result = await decorated.HandleAsync("suv", CancellationToken.None);

        Assert.Equal(3, result);
        var log = Assert.Single(_logger.Collector.GetSnapshot());
        Assert.StartsWith("LengthCommandHandler.HandleAsync se ejecutó en", log.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task MeasureAsync_WhenTheOperationFails_PrintsTheTimeAndRethrows()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _timer.MeasureAsync<int>("Falla", () => throw new InvalidOperationException("error")));

        Assert.StartsWith("Falla se ejecutó en", Assert.Single(_logger.Collector.GetSnapshot()).Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task MeasureAsync_NullAction_Throws()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _timer.MeasureAsync<int>("Nada", null!));
    }

    private sealed class LengthQueryHandler : IQueryHandler<string, int>
    {
        public Task<int> HandleAsync(string query, CancellationToken cancellationToken) => Task.FromResult(query.Length);
    }

    private sealed class LengthCommandHandler : ICommandHandler<string, int>
    {
        public Task<int> HandleAsync(string command, CancellationToken cancellationToken) => Task.FromResult(command.Length);
    }
}
