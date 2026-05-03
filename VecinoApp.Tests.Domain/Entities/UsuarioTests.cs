using Xunit;
using FluentAssertions;
using VecinoApp.Domain.Entities;

namespace VecinoApp.Tests.Domain.Entities;

public class UsuarioTests
{
    [Fact]
    public void CrearUsuario_ConDatosValidos_DeberiaTenerPropiedadesCorrectas()
    {
        // Arrange
        var nombre = "Alan Brito";
        var email = "alan@vecinoapp.com";
        var telefono = "3001234567";
        var fotoPerfilUrl = "https://example.com/foto.jpg";

        // Act
        var usuario = new Usuario
        {
            Nombre = nombre,
            Email = email,
            Telefono = telefono,
            FotoPerfilUrl = fotoPerfilUrl,
            Activo = true
        };

        // Assert
        usuario.Nombre.Should().Be(nombre);
        usuario.Email.Should().Be(email);
        usuario.Telefono.Should().Be(telefono);
        usuario.FotoPerfilUrl.Should().Be(fotoPerfilUrl);
        usuario.Activo.Should().BeTrue();
    }

    [Fact]
    public void CrearUsuario_ValoresPorDefecto_DeberianSerCorrectos()
    {
        // Arrange & Act
        var usuario = new Usuario();

        // Assert
        usuario.Nombre.Should().Be(string.Empty);
        usuario.Activo.Should().BeTrue();
        usuario.Reseñas.Should().NotBeNull();
        usuario.Favoritos.Should().NotBeNull();
        usuario.Negocios.Should().NotBeNull();
    }

    [Fact]
    public void Usuario_FechaRegistro_DeberiaSerUtcNow()
    {
        // Arrange & Act
        var usuario = new Usuario();
        var ahora = DateTime.UtcNow;

        // Assert
        usuario.FechaRegistro.Should().BeCloseTo(ahora, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Usuario_PropiedadesNavegacion_DeberianInicializarseComoColeccionesVacias()
    {
        // Arrange & Act
        var usuario = new Usuario();

        // Assert
        usuario.Reseñas.Should().BeEmpty();
        usuario.Favoritos.Should().BeEmpty();
        usuario.Negocios.Should().BeEmpty();
    }

    [Fact]
    public void Usuario_TelefonoYFotoPerfil_DeberianSerNulosPorDefecto()
    {
        // Arrange & Act
        var usuario = new Usuario();

        // Assert
        usuario.Telefono.Should().BeNull();
        usuario.FotoPerfilUrl.Should().BeNull();
    }
}