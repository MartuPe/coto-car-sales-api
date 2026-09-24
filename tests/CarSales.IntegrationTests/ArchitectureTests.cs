using System.Reflection;

namespace CarSales.IntegrationTests;

/// <summary>
/// Verifica la regla de dependencias de Clean Architecture: cada capa solo conoce a las de adentro.
/// Las referencias entre proyectos ya lo impiden al compilar; este test deja la regla escrita y falla
/// si alguien agrega una referencia que la rompa.
/// </summary>
public class ArchitectureTests
{
    public static TheoryData<string, string[]> DependencyRules => new()
    {
        { "CarSales.Domain", ["CarSales.Application", "CarSales.Infrastructure", "CarSales.Api"] },
        { "CarSales.Application", ["CarSales.Infrastructure", "CarSales.Api"] },
        { "CarSales.Infrastructure", ["CarSales.Api"] },
    };

    [Theory]
    [MemberData(nameof(DependencyRules))]
    public void Layer_DoesNotDependOnOuterLayers(string layer, string[] outerLayers)
    {
        var references = ReferencedAssemblies(layer);

        Assert.Empty(references.Intersect(outerLayers));
    }

    [Fact]
    public void Domain_DependsOnlyOnTheBaseLibrary()
    {
        // netstandard aparece cuando coverlet instrumenta el ensamblado para medir cobertura; también es librería base.
        Assert.All(
            ReferencedAssemblies("CarSales.Domain"),
            name => Assert.True(
                name == "netstandard" || name?.StartsWith("System", StringComparison.Ordinal) == true,
                $"El dominio no debería depender de {name}."));
    }

    private static IEnumerable<string?> ReferencedAssemblies(string assemblyName) =>
        Assembly.Load(assemblyName).GetReferencedAssemblies().Select(reference => reference.Name);
}
