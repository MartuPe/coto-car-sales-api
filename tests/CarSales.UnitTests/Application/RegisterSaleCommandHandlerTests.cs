using CarSales.Application.Sales.RegisterSale;
using CarSales.Domain.Common;

namespace CarSales.UnitTests.Application;

public class RegisterSaleCommandHandlerTests
{
    private readonly FakeSaleRepository _sales = new();
    private readonly RegisterSaleCommandHandler _handler;

    public RegisterSaleCommandHandlerTests()
    {
        _handler = new RegisterSaleCommandHandler(
            new FakeDistributionCenterRepository(),
            new FakeCarModelRepository(),
            _sales,
            new FixedTimeProvider(TestData.Now));
    }

    [Fact]
    public async Task HandleAsync_ValidSale_SavesItAndReturnsItsAmounts()
    {
        var result = await _handler.HandleAsync(new RegisterSaleCommand(1, "sport", 2), CancellationToken.None);

        var saved = Assert.Single(_sales.Saved);
        Assert.Equal(saved.Id, result.Id);
        Assert.Equal(1, result.DistributionCenterId);
        Assert.Equal("Centro Norte", result.DistributionCenterName);
        Assert.Equal("Sport", result.Model);
        Assert.Equal(2, result.Quantity);
        Assert.Equal(18_200m, result.UnitPrice);
        Assert.Equal(36_400m, result.NetAmount);
        Assert.Equal(2_548m, result.TaxAmount);
        Assert.Equal(38_948m, result.TotalAmount);
        Assert.Equal(TestData.Now, result.SoldAt);
    }

    [Fact]
    public async Task HandleAsync_UnknownCenter_ThrowsListingTheValidOnes()
    {
        var exception = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(new RegisterSaleCommand(9, "Sedan", 1), CancellationToken.None));

        Assert.Equal("No existe el centro de distribución 9. Centros válidos: 1, 2.", exception.Message);
        Assert.Empty(_sales.Saved);
    }

    [Fact]
    public async Task HandleAsync_UnknownModel_ThrowsListingTheValidOnes()
    {
        var exception = await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(new RegisterSaleCommand(1, "Coupe", 1), CancellationToken.None));

        Assert.Equal("No existe el modelo 'Coupe'. Modelos válidos: Sedan, Sport.", exception.Message);
        Assert.Empty(_sales.Saved);
    }

    [Fact]
    public async Task HandleAsync_InvalidQuantity_DoesNotSaveAnything()
    {
        await Assert.ThrowsAsync<DomainException>(
            () => _handler.HandleAsync(new RegisterSaleCommand(1, "Sedan", 0), CancellationToken.None));

        Assert.Empty(_sales.Saved);
    }

    [Fact]
    public async Task HandleAsync_NullCommand_Throws()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.HandleAsync(null!, CancellationToken.None));
    }
}
