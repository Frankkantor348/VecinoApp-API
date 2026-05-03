using Xunit;
using FluentAssertions;
using VecinoApp.Domain.Entities;

namespace VecinoApp.Tests.Domain.Entities;

public class PromocionTests
{
    [Fact]
    public void CrearPromocion_ConDatosValidos_DeberiaTenerPropiedadesCorrectas()
    {
        // Arrange
        var negocioId = 1;
        var titulo = "Descuento de Verano";
        var descuento = 20;
        var descripcion = "20% de descuento en todos los productos";
        var fechaInicio = DateTime.UtcNow.Date;
        var fechaFin = DateTime.UtcNow.Date.AddDays(30);

        // Act
        var promocion = new Promocion
        {
            NegocioId = negocioId,
            Titulo = titulo,
            Descuento = descuento,
            Descripcion = descripcion,
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            Activa = true
        };

        // Assert
        promocion.NegocioId.Should().Be(negocioId);
        promocion.Titulo.Should().Be(titulo);
        promocion.Descuento.Should().Be(descuento);
        promocion.Descripcion.Should().Be(descripcion);
        promocion.FechaInicio.Should().Be(fechaInicio);
        promocion.FechaFin.Should().Be(fechaFin);
        promocion.Activa.Should().BeTrue();
    }

    [Fact]
    public void CrearPromocion_ValoresPorDefecto_DeberianSerCorrectos()
    {
        // Arrange & Act
        var promocion = new Promocion();

        // Assert
        promocion.Titulo.Should().Be(string.Empty);
        promocion.Descripcion.Should().Be(string.Empty);
        promocion.Descuento.Should().Be(0);
        promocion.Activa.Should().BeTrue();
    }

    [Fact]
    public void Promocion_FechaPorDefecto_DeberiaSerDateTimeMinValue()
    {
        // Arrange & Act
        var promocion = new Promocion();

        // Assert
        promocion.FechaInicio.Should().Be(DateTime.MinValue);
        promocion.FechaFin.Should().Be(DateTime.MinValue);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(100)]
    public void Promocion_Descuento_DeberiaPermitirDiferentesValores(int descuento)
    {
        // Arrange & Act
        var promocion = new Promocion { Descuento = descuento };

        // Assert
        promocion.Descuento.Should().Be(descuento);
    }

    [Fact]
    public void Promocion_ActivaPorDefecto_DeberiaSerTrue()
    {
        // Arrange & Act
        var promocion = new Promocion();

        // Assert
        promocion.Activa.Should().BeTrue();
    }
}