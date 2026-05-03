using Xunit;
using FluentAssertions;
using VecinoApp.Domain.Entities;

namespace VecinoApp.Tests.Domain.Entities;

public class FavoritoTests
{
    [Fact]
    public void CrearFavorito_ConDatosValidos_DeberiaTenerPropiedadesCorrectas()
    {
        // Arrange
        var usuarioId = 1;
        var negocioId = 1;

        // Act
        var favorito = new Favorito
        {
            UsuarioId = usuarioId,
            NegocioId = negocioId
        };

        // Assert
        favorito.UsuarioId.Should().Be(usuarioId);
        favorito.NegocioId.Should().Be(negocioId);
    }

    [Fact]
    public void CrearFavorito_ValoresPorDefecto_DeberianSerCero()
    {
        // Arrange & Act
        var favorito = new Favorito();

        // Assert
        favorito.Id.Should().Be(0);
        favorito.UsuarioId.Should().Be(0);
        favorito.NegocioId.Should().Be(0);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(5, 10)]
    [InlineData(100, 50)]
    [InlineData(999, 888)]
    public void Favorito_DiferentesCombinaciones_DeberiaAsignarCorrectamente(int usuarioId, int negocioId)
    {
        // Arrange & Act
        var favorito = new Favorito
        {
            UsuarioId = usuarioId,
            NegocioId = negocioId
        };

        // Assert
        favorito.UsuarioId.Should().Be(usuarioId);
        favorito.NegocioId.Should().Be(negocioId);
    }

    [Fact]
    public void Favorito_PropiedadesNavegacion_DeberianSerNulasPorDefecto()
    {
        // Arrange & Act
        var favorito = new Favorito();

        // Assert
        favorito.Usuario.Should().BeNull();
        favorito.Negocio.Should().BeNull();
    }
}