using Xunit;
using FluentAssertions;
using VecinoApp.Domain.Entities;

namespace VecinoApp.Tests.Domain.Entities;

public class ProductoTests
{
    [Fact]
    public void CrearProducto_ConDatosValidos_DeberiaTenerPropiedadesCorrectas()
    {
        // Arrange
        var negocioId = 1;
        var nombre = "Arepa Paisa";
        var descripcion = "Deliciosa arepa con queso y mantequilla";
        decimal? precio = 3500m;

        // Act
        var producto = new Producto
        {
            NegocioId = negocioId,
            Nombre = nombre,
            Descripcion = descripcion,
            Precio = precio,
            Destacado = true
        };

        // Assert
        producto.NegocioId.Should().Be(negocioId);
        producto.Nombre.Should().Be(nombre);
        producto.Descripcion.Should().Be(descripcion);
        producto.Precio.Should().Be(precio);
        producto.Destacado.Should().BeTrue();
    }

    [Fact]
    public void CrearProducto_ValoresPorDefecto_DeberianSerCorrectos()
    {
        // Arrange & Act
        var producto = new Producto();

        // Assert
        producto.Nombre.Should().Be(string.Empty);
        producto.Descripcion.Should().BeNull();
        producto.Precio.Should().BeNull();
        producto.Destacado.Should().BeFalse();
    }

    [Theory]
    [InlineData(1000)]
    [InlineData(2500.50)]
    [InlineData(9999.99)]
    [InlineData(0)]
    public void Producto_Precio_DeberiaPermitirDiferentesValores(decimal precio)
    {
        // Arrange & Act
        var producto = new Producto { Precio = precio };

        // Assert
        producto.Precio.Should().Be(precio);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Producto_Destacado_DeberiaPermitirValores(bool destacado)
    {
        // Arrange & Act
        var producto = new Producto { Destacado = destacado };

        // Assert
        producto.Destacado.Should().Be(destacado);
    }

    [Fact]
    public void Producto_DescripcionOpcional_DeberiaSerNulaPorDefecto()
    {
        // Arrange & Act
        var producto = new Producto();

        // Assert
        producto.Descripcion.Should().BeNull();
    }

    [Fact]
    public void Producto_PropiedadNavegacion_DeberiaSerNulaPorDefecto()
    {
        // Arrange & Act
        var producto = new Producto();

        // Assert
        producto.Negocio.Should().BeNull();
    }
}