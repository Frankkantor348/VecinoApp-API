using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using VecinoApp.API.Controllers;
using VecinoApp.Application.DTOs;
using VecinoApp.Domain.Entities;
using Xunit;

namespace VecinoApp.Tests.API.Controllers;

public class UsuariosControllerTests
{
    private readonly Mock<UserManager<Usuario>> _mockUserManager;
    private readonly Mock<IWebHostEnvironment> _mockWebHostEnvironment;
    private readonly UsuariosController _controller;
    private readonly int _usuarioId = 1;

    public UsuariosControllerTests()
    {
        var userStore = Mock.Of<IUserStore<Usuario>>();
        _mockUserManager = new Mock<UserManager<Usuario>>(
            userStore, null!, null!, null!, null!, null!, null!, null!, null!);

        _mockWebHostEnvironment = new Mock<IWebHostEnvironment>();

        // Pasamos null para ApplicationDbContext porque las pruebas que usan
        // GetPerfil serán omitidas (requieren contexto real)
        _controller = new UsuariosController(
            _mockUserManager.Object,
            null!,  // ApplicationDbContext
            _mockWebHostEnvironment.Object);

        // Configurar usuario autenticado
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _usuarioId.ToString())
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    // ============================================================
    // Pruebas para UpdatePerfil (NO dependen de _context)
    // ============================================================

    [Fact]
    public async Task UpdatePerfil_UsuarioExistente_DeberiaActualizarYRetornarOk()
    {
        // Arrange
        var usuario = new Usuario
        {
            Id = _usuarioId,
            Nombre = "Nombre Antiguo",
            Telefono = "123456789"
        };

        var updateDto = new UpdatePerfilDto
        {
            Nombre = "Nombre Nuevo",
            Telefono = "987654321"
        };

        _mockUserManager.Setup(x => x.FindByIdAsync(_usuarioId.ToString()))
            .ReturnsAsync(usuario);
        _mockUserManager.Setup(x => x.UpdateAsync(usuario))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var resultado = await _controller.UpdatePerfil(updateDto);

        // Assert
        resultado.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UpdatePerfil_UsuarioNoExistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var updateDto = new UpdatePerfilDto
        {
            Nombre = "Nombre Nuevo",
            Telefono = "987654321"
        };

        _mockUserManager.Setup(x => x.FindByIdAsync(_usuarioId.ToString()))
            .ReturnsAsync((Usuario?)null);

        // Act
        var resultado = await _controller.UpdatePerfil(updateDto);

        // Assert
        resultado.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task UpdatePerfil_ErrorAlActualizar_DeberiaRetornarBadRequest()
    {
        // Arrange
        var usuario = new Usuario { Id = _usuarioId, Nombre = "Nombre" };
        var updateDto = new UpdatePerfilDto { Nombre = "Nombre Nuevo", Telefono = "123" };

        _mockUserManager.Setup(x => x.FindByIdAsync(_usuarioId.ToString()))
            .ReturnsAsync(usuario);
        _mockUserManager.Setup(x => x.UpdateAsync(usuario))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error de actualización" }));

        // Act
        var resultado = await _controller.UpdatePerfil(updateDto);

        // Assert
        resultado.Should().BeOfType<BadRequestObjectResult>();
    }

    // ============================================================
    // Pruebas para UpdateFotoPerfil (NO dependen de _context)
    // ============================================================

    [Fact]
    public async Task UpdateFotoPerfil_ConFotoValida_DeberiaGuardarYRetornarOk()
    {
        // Arrange
        var usuario = new Usuario { Id = _usuarioId, FotoPerfilUrl = null };
        var fotoMock = new Mock<IFormFile>();
        var fileName = "foto.jpg";
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write("imagen_falsa");
        writer.Flush();
        stream.Position = 0;

        fotoMock.Setup(x => x.OpenReadStream()).Returns(stream);
        fotoMock.Setup(x => x.FileName).Returns(fileName);
        fotoMock.Setup(x => x.Length).Returns(stream.Length);

        _mockUserManager.Setup(x => x.FindByIdAsync(_usuarioId.ToString()))
            .ReturnsAsync(usuario);
        _mockUserManager.Setup(x => x.UpdateAsync(usuario))
            .ReturnsAsync(IdentityResult.Success);

        _mockWebHostEnvironment.Setup(x => x.WebRootPath).Returns(Path.GetTempPath());

        // Act
        var resultado = await _controller.UpdateFotoPerfil(fotoMock.Object);

        // Assert
        resultado.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UpdateFotoPerfil_SinFoto_DeberiaRetornarBadRequest()
    {
        // Arrange
        var usuario = new Usuario { Id = _usuarioId };
        _mockUserManager.Setup(x => x.FindByIdAsync(_usuarioId.ToString()))
            .ReturnsAsync(usuario);

        // Act
        var resultado = await _controller.UpdateFotoPerfil(null!);

        // Assert
        resultado.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateFotoPerfil_FormatoInvalido_DeberiaRetornarBadRequest()
    {
        // Arrange
        var usuario = new Usuario { Id = _usuarioId };
        var fotoMock = new Mock<IFormFile>();
        fotoMock.Setup(x => x.FileName).Returns("foto.gif");
        fotoMock.Setup(x => x.Length).Returns(1024);

        _mockUserManager.Setup(x => x.FindByIdAsync(_usuarioId.ToString()))
            .ReturnsAsync(usuario);

        // Act
        var resultado = await _controller.UpdateFotoPerfil(fotoMock.Object);

        // Assert
        resultado.Should().BeOfType<BadRequestObjectResult>();
    }

    // ============================================================
    // Pruebas para DeleteFotoPerfil (NO dependen de _context)
    // ============================================================

    [Fact]
    public async Task DeleteFotoPerfil_UsuarioConFoto_DeberiaEliminarFotoYRetornarOk()
    {
        // Arrange
        var usuario = new Usuario
        {
            Id = _usuarioId,
            FotoPerfilUrl = "/uploads/perfiles/foto.jpg"
        };

        _mockUserManager.Setup(x => x.FindByIdAsync(_usuarioId.ToString()))
            .ReturnsAsync(usuario);
        _mockUserManager.Setup(x => x.UpdateAsync(usuario))
            .ReturnsAsync(IdentityResult.Success);

        _mockWebHostEnvironment.Setup(x => x.WebRootPath).Returns(Path.GetTempPath());

        // Act
        var resultado = await _controller.DeleteFotoPerfil();

        // Assert
        resultado.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeleteFotoPerfil_UsuarioSinFoto_DeberiaRetornarOk()
    {
        // Arrange
        var usuario = new Usuario { Id = _usuarioId, FotoPerfilUrl = null };

        _mockUserManager.Setup(x => x.FindByIdAsync(_usuarioId.ToString()))
            .ReturnsAsync(usuario);

        // Act
        var resultado = await _controller.DeleteFotoPerfil();

        // Assert
        resultado.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeleteFotoPerfil_UsuarioNoExistente_DeberiaRetornarNotFound()
    {
        // Arrange
        _mockUserManager.Setup(x => x.FindByIdAsync(_usuarioId.ToString()))
            .ReturnsAsync((Usuario?)null);

        // Act
        var resultado = await _controller.DeleteFotoPerfil();

        // Assert
        resultado.Should().BeOfType<NotFoundResult>();
    }
}