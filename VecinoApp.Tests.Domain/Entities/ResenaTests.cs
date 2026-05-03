using Xunit;
using FluentAssertions;
using VecinoApp.Domain.Entities;

namespace VecinoApp.Tests.Domain.Entities;

public class ResenaTests
{
    [Fact]
    public void CrearResena_ConDatosValidos_DeberiaTenerPropiedadesCorrectas()
    {
        // Arrange
        var usuarioId = 1;
        var negocioId = 1;
        var calificacion = 5;
        var comentario = "Excelente servicio!";

        // Act
        var reseña = new Reseña
        {
            UsuarioId = usuarioId,
            NegocioId = negocioId,
            Calificacion = calificacion,
            Comentario = comentario
        };

        // Assert
        reseña.UsuarioId.Should().Be(usuarioId);
        reseña.NegocioId.Should().Be(negocioId);
        reseña.Calificacion.Should().Be(calificacion);
        reseña.Comentario.Should().Be(comentario);
    }

    [Fact]
    public void CrearResena_ComentarioPorDefecto_DeberiaSerStringVacio()
    {
        // Arrange & Act
        var reseña = new Reseña();

        // Assert
        reseña.Comentario.Should().Be(string.Empty);
    }

    [Fact]
    public void CrearResena_FechaPorDefecto_DeberiaSerUtcNow()
    {
        // Arrange & Act
        var reseña = new Reseña();
        var ahora = DateTime.UtcNow;

        // Assert
        reseña.Fecha.Should().BeCloseTo(ahora, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CrearResena_CalificacionInvalida_DeberiaPermitirCualquierValor()
    {
        // Arrange & Act
        var reseña = new Reseña { Calificacion = 6 };

        // Assert
        reseña.Calificacion.Should().Be(6);
    }
}