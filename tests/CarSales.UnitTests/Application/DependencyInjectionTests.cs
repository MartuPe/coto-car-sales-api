using CarSales.Application;
using CarSales.Application.Abstractions;
using CarSales.Application.Abstractions.Persistence;
using CarSales.Application.Common;
using CarSales.Application.Diagnostics;
using CarSales.Application.Reports.GetCenterSalesVolume;
using CarSales.Application.Reports.GetModelShareByCenter;
using CarSales.Application.Reports.GetSalesVolumeByCenter;
using CarSales.Application.Reports.GetTotalSalesVolume;
using CarSales.Application.Sales.RegisterSale;
using Microsoft.Extensions.DependencyInjection;

namespace CarSales.UnitTests.Application;

public class DependencyInjectionTests
{
    [Fact]
    public void AddApplication_RegistersEveryUseCaseWrappedByTheTimingDecorator()
    {
        var services = new ServiceCollection();
        services.AddFakeLogging();
        services.AddSingleton<ISaleRepository>(new FakeSaleRepository());
        services.AddSingleton<ICarModelRepository>(new FakeCarModelRepository());
        services.AddSingleton<IDistributionCenterRepository>(new FakeDistributionCenterRepository());
        services.AddApplication();

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
        using var scope = provider.CreateScope();
        var resolver = scope.ServiceProvider;

        Assert.IsType<TimedCommandHandler<RegisterSaleCommand, SaleDto>>(
            resolver.GetRequiredService<ICommandHandler<RegisterSaleCommand, SaleDto>>());
        Assert.IsType<TimedQueryHandler<GetTotalSalesVolumeQuery, SalesVolumeDto>>(
            resolver.GetRequiredService<IQueryHandler<GetTotalSalesVolumeQuery, SalesVolumeDto>>());
        Assert.IsType<TimedQueryHandler<GetSalesVolumeByCenterQuery, IReadOnlyList<CenterSalesVolumeDto>>>(
            resolver.GetRequiredService<IQueryHandler<GetSalesVolumeByCenterQuery, IReadOnlyList<CenterSalesVolumeDto>>>());
        Assert.IsType<TimedQueryHandler<GetCenterSalesVolumeQuery, CenterSalesVolumeDto>>(
            resolver.GetRequiredService<IQueryHandler<GetCenterSalesVolumeQuery, CenterSalesVolumeDto>>());
        Assert.IsType<TimedQueryHandler<GetModelShareByCenterQuery, ModelShareReportDto>>(
            resolver.GetRequiredService<IQueryHandler<GetModelShareByCenterQuery, ModelShareReportDto>>());
    }
}
