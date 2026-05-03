using Xunit;
using FluentAssertions;
using VecinoApp.Domain.Entities;

namespace VecinoApp.Tests.Domain.Entities;

public class NegocioTests
{
    [Fact]
    public void CrearNegocio_ConDatosValidos_DeberiaTenerPropiedadesCorrectas()
    {
        // Arrange
        var nombre = "Peluquería Test";
        var descripcion = "La mejor peluquería del barrio";
        var tipo = "Peluquería";
        var direccion = "Calle 123";
        var telefono = "3001234567";
        var horario = "Lun-Vie 9am-6pm";

        // Act
        var negocio = new Negocio
        {
            Nombre = nombre,
            Descripcion = descripcion,
            Tipo = tipo,
            Direccion = direccion,
            Telefono = telefono,
            Horario = horario,
            Latitud = 4.7110,
            Longitud = -74.0721,
            Activo = true,
            Aprobado = false,
            PropietarioId = 1
        };

        // Assert
        negocio.Nombre.Should().Be(nombre);
        negocio.Descripcion.Should().Be(descripcion);
        negocio.Tipo.Should().Be(tipo);
        negocio.Direccion.Should().Be(direccion);
        negocio.Telefono.Should().Be(telefono);
        negocio.Horario.Should().Be(horario);
        negocio.Latitud.Should().Be(4.7110);
        negocio.Longitud.Should().Be(-74.0721);
        negocio.Activo.Should().BeTrue();
        negocio.Aprobado.Should().BeFalse();
        negocio.PropietarioId.Should().Be(1);
    }

    [Fact]
    public void CrearNegocio_ValoresPorDefecto_DeberianSerCorrectos()
    {
        // Arrange & Act
        var negocio = new Negocio();

        // Assert
        negocio.Nombre.Should().Be(string.Empty);
        negocio.Descripcion.Should().Be(string.Empty);
        negocio.Tipo.Should().Be(string.Empty);
        negocio.Direccion.Should().Be(string.Empty);
        negocio.Activo.Should().BeTrue();
        negocio.Aprobado.Should().BeFalse();
        negocio.Reseñas.Should().NotBeNull();
        negocio.Favoritos.Should().NotBeNull();
        negocio.Productos.Should().NotBeNull();
        negocio.Promociones.Should().NotBeNull();
    }

    [Fact]
    public void Negocio_PropiedadesNavegacion_DeberianInicializarseComoColeccionesVacias()
    {
        // Arrange & Act
        var negocio = new Negocio();

        // Assert
        negocio.Reseñas.Should().BeEmpty();
        negocio.Favoritos.Should().BeEmpty();
        negocio.Productos.Should().BeEmpty();
        negocio.Promociones.Should().BeEmpty();
    }

    [Fact]
    public void Negocio_Coordenadas_DeberianSerNulasPorDefecto()
    {
        // Arrange & Act
        var negocio = new Negocio();

        // Assert
        negocio.Latitud.Should().BeNull();
        negocio.Longitud.Should().BeNull();
        negocio.Ubicacion.Should().BeNull();
    }
}