using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using System.Security.Claims;
using VecinoApp.API.Controllers;
using VecinoApp.Application.DTOs;
using VecinoApp.Domain.Entities;
using VecinoApp.Domain.Interfaces;
using VecinoApp.Infrastructure.Services;
using Xunit;

namespace VecinoApp.Tests.API.Controllers;

public class NegociosControllerTests
{
    private readonly Mock<INegocioRepository> _mockNegocioRepo;
    private readonly Mock<UserManager<Usuario>> _mockUserManager;
    private readonly Mock<IWebHostEnvironment> _mockWebHostEnvironment;
    private readonly Mock<IFileService> _mockFileService;
    private readonly NegociosController _controller;
    private readonly int _usuarioId = 1;

    public NegociosControllerTests()
    {
        _mockNegocioRepo = new Mock<INegocioRepository>();

        var store = new Mock<IUserStore<Usuario>>();
        _mockUserManager = new Mock<UserManager<Usuario>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _mockWebHostEnvironment = new Mock<IWebHostEnvironment>();
        _mockFileService = new Mock<IFileService>();

        _controller = new NegociosController(
            _mockNegocioRepo.Object,
            _mockUserManager.Object,
            _mockWebHostEnvironment.Object,
            _mockFileService.Object);

        // Configurar usuario autenticado
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _usuarioId.ToString()),
            new Claim(ClaimTypes.Role, "User")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    // ============================================================
    // Pruebas para GetCategorias
    // ============================================================

    [Fact]
    public async Task GetCategorias_DeberiaRetornarOkConListaDeCategorias()
    {
        // Arrange
        var categorias = new List<string> { "Panadería", "Peluquería", "Tienda" };
        _mockNegocioRepo.Setup(r => r.GetCategoriasAsync()).ReturnsAsync(categorias);

        // Act
        var resultado = await _controller.GetCategorias();

        // Assert
        resultado.Result.Should().BeOfType<OkObjectResult>();
    }

    // ============================================================
    // Pruebas para GetNegociosPorTipo
    // ============================================================

    [Fact]
    public async Task GetNegociosPorTipo_DeberiaRetornarOkConNegocios()
    {
        // Arrange
        var tipo = "Panadería";
        var negocios = new List<Negocio>
        {
            new Negocio { Id = 1, Nombre = "Panadería 1", Tipo = "Panadería" }
        };
        _mockNegocioRepo.Setup(r => r.GetByTipoAsync(tipo)).ReturnsAsync(negocios);

        // Act
        var resultado = await _controller.GetNegociosPorTipo(tipo);

        // Assert
        resultado.Result.Should().BeOfType<OkObjectResult>();
    }

    // ============================================================
    // Pruebas para GetNegocios
    // ============================================================

    [Fact]
    public async Task GetNegocios_DeberiaRetornarOkConLista()
    {
        // Arrange
        var negocios = new List<Negocio> { new Negocio { Id = 1, Nombre = "Negocio 1" } };
        _mockNegocioRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(negocios);

        // Act
        var resultado = await _controller.GetNegocios();

        // Assert
        resultado.Result.Should().BeOfType<OkObjectResult>();
    }

    // ============================================================
    // Pruebas para GetNegociosCercanos
    // ============================================================

    [Fact]
    public async Task GetNegociosCercanos_DeberiaRetornarOk()
    {
        // Arrange
        var latitud = 4.7110;
        var longitud = -74.0721;
        var negocios = new List<Negocio>();
        _mockNegocioRepo.Setup(r => r.GetNegociosCercanosAsync(latitud, longitud, 1000))
            .ReturnsAsync(negocios);

        // Act
        var resultado = await _controller.GetNegociosCercanos(latitud, longitud, 1000, null);

        // Assert
        resultado.Result.Should().BeOfType<OkObjectResult>();
    }

    // ============================================================
    // Pruebas para GetNegocio
    // ============================================================

    [Fact]
    public async Task GetNegocio_CuandoExiste_DeberiaRetornarOk()
    {
        // Arrange
        var negocio = new Negocio { Id = 1, Nombre = "Negocio Test" };
        _mockNegocioRepo.Setup(r => r.GetByIdWithReseñasAsync(1)).ReturnsAsync(negocio);

        // Act
        var resultado = await _controller.GetNegocio(1);

        // Assert
        resultado.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetNegocio_CuandoNoExiste_DeberiaRetornarNotFound()
    {
        // Arrange
        _mockNegocioRepo.Setup(r => r.GetByIdWithReseñasAsync(999)).ReturnsAsync((Negocio?)null);

        // Act
        var resultado = await _controller.GetNegocio(999);

        // Assert
        resultado.Result.Should().BeOfType<NotFoundResult>();
    }

    // ============================================================
    // Pruebas para GetNegociosPendientes (Admin)
    // ============================================================

    [Fact]
    public async Task GetNegociosPendientes_ComoAdmin_DeberiaRetornarOk()
    {
        // Arrange
        var negocios = new List<Negocio>
        {
            new Negocio { Id = 1, Aprobado = false },
            new Negocio { Id = 2, Aprobado = true }
        };
        _mockNegocioRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(negocios);

        // Act
        var resultado = await _controller.GetNegociosPendientes();

        // Assert
        resultado.Result.Should().BeOfType<OkObjectResult>();
    }

    // ============================================================
    // Pruebas para AprobarNegocio (Admin)
    // ============================================================

    [Fact]
    public async Task AprobarNegocio_NegocioExistente_DeberiaAprobarYRetornarOk()
    {
        // Arrange
        var negocio = new Negocio { Id = 1, Aprobado = false };
        _mockNegocioRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(negocio);
        _mockNegocioRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var resultado = await _controller.AprobarNegocio(1);

        // Assert
        resultado.Should().BeOfType<OkObjectResult>();
        negocio.Aprobado.Should().BeTrue();
    }

    [Fact]
    public async Task AprobarNegocio_NegocioNoExistente_DeberiaRetornarNotFound()
    {
        // Arrange
        _mockNegocioRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Negocio?)null);

        // Act
        var resultado = await _controller.AprobarNegocio(999);

        // Assert
        resultado.Should().BeOfType<NotFoundObjectResult>();
    }

    // ============================================================
    // Pruebas para RechazarNegocio (Admin)
    // ============================================================

    [Fact]
    public async Task RechazarNegocio_NegocioExistente_DeberiaEliminarYRetornarOk()
    {
        // Arrange
        var negocio = new Negocio { Id = 1 };
        _mockNegocioRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(negocio);
        _mockNegocioRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var resultado = await _controller.RechazarNegocio(1);

        // Assert
        resultado.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task RechazarNegocio_NegocioNoExistente_DeberiaRetornarNotFound()
    {
        // Arrange
        _mockNegocioRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Negocio?)null);

        // Act
        var resultado = await _controller.RechazarNegocio(999);

        // Assert
        resultado.Should().BeOfType<NotFoundObjectResult>();
    }

    // ============================================================
    // Pruebas para CreateNegocio
    // ============================================================

    [Fact]
    public async Task CreateNegocio_ConDatosValidos_DeberiaCrearYRetornarCreated()
    {
        // Arrange
        var createDto = new CreateNegocioDto
        {
            Nombre = "Nuevo Negocio",
            Descripcion = "Descripción",
            Tipo = "Tienda",
            Direccion = "Calle 123",
            Telefono = "3001234567",
            Horario = "Lun-Vie 9-6",
            PropietarioId = 1,
            Latitud = 4.7110,
            Longitud = -74.0721
        };

        var propietario = new Usuario { Id = 1, Nombre = "Propietario" };
        var negocioCreado = new Negocio { Id = 1 };

        _mockUserManager.Setup(u => u.FindByIdAsync("1")).ReturnsAsync(propietario);
        _mockNegocioRepo.Setup(r => r.AddAsync(It.IsAny<Negocio>())).Returns(Task.CompletedTask);
        _mockNegocioRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var resultado = await _controller.CreateNegocio(createDto);

        // Assert
        resultado.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task CreateNegocio_PropietarioNoExistente_DeberiaRetornarBadRequest()
    {
        // Arrange
        var createDto = new CreateNegocioDto
        {
            Nombre = "Negocio",
            Descripcion = "Desc",
            Tipo = "Tipo",
            Direccion = "Dir",
            PropietarioId = 999
        };

        _mockUserManager.Setup(u => u.FindByIdAsync("999")).ReturnsAsync((Usuario?)null);

        // Act
        var resultado = await _controller.CreateNegocio(createDto);

        // Assert
        resultado.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ============================================================
    // Pruebas para UpdateNegocio
    // ============================================================

    [Fact]
    public async Task UpdateNegocio_NegocioExistente_DeberiaActualizarYRetornarNoContent()
    {
        // Arrange
        var negocio = new Negocio { Id = 1, Nombre = "Nombre Antiguo" };
        var updateDto = new UpdateNegocioDto
        {
            Nombre = "Nombre Nuevo",
            Descripcion = "Desc",
            Tipo = "Tipo",
            Direccion = "Dir",
            Activo = true,
            PropietarioId = 1
        };

        _mockNegocioRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(negocio);
        _mockNegocioRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var resultado = await _controller.UpdateNegocio(1, updateDto);

        // Assert
        resultado.Should().BeOfType<NoContentResult>();
        negocio.Nombre.Should().Be("Nombre Nuevo");
    }

    [Fact]
    public async Task UpdateNegocio_NegocioNoExistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var updateDto = new UpdateNegocioDto();
        _mockNegocioRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Negocio?)null);

        // Act
        var resultado = await _controller.UpdateNegocio(999, updateDto);

        // Assert
        resultado.Should().BeOfType<NotFoundResult>();
    }

    // ============================================================
    // Pruebas para DeleteNegocio
    // ============================================================

    [Fact]
    public async Task DeleteNegocio_NegocioExistente_DeberiaEliminarYRetornarNoContent()
    {
        // Arrange
        var negocio = new Negocio { Id = 1, ImagenUrl = null };
        _mockNegocioRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(negocio);
        _mockNegocioRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var resultado = await _controller.DeleteNegocio(1);

        // Assert
        resultado.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteNegocio_NegocioNoExistente_DeberiaRetornarNotFound()
    {
        // Arrange
        _mockNegocioRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Negocio?)null);

        // Act
        var resultado = await _controller.DeleteNegocio(999);

        // Assert
        resultado.Should().BeOfType<NotFoundResult>();
    }

    // ============================================================
    // Pruebas para SubirImagen
    // ============================================================

    [Fact]
    public async Task SubirImagen_ConPermiso_DeberiaSubirYRetornarOk()
    {
        // Arrange
        var negocio = new Negocio { Id = 1, PropietarioId = _usuarioId, ImagenUrl = null };
        var fotoMock = new Mock<IFormFile>();
        fotoMock.Setup(f => f.Length).Returns(1024);
        fotoMock.Setup(f => f.FileName).Returns("foto.jpg");

        _mockNegocioRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(negocio);
        _mockFileService.Setup(f => f.GuardarImagen(It.IsAny<IFormFile>(), "negocios"))
            .ReturnsAsync("/uploads/negocios/foto.jpg");
        _mockNegocioRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var resultado = await _controller.SubirImagen(1, fotoMock.Object);

        // Assert
        resultado.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task SubirImagen_NegocioNoExistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var fotoMock = new Mock<IFormFile>();
        _mockNegocioRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Negocio?)null);

        // Act
        var resultado = await _controller.SubirImagen(999, fotoMock.Object);

        // Assert
        resultado.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task SubirImagen_SinPermiso_DeberiaRetornar403()
    {
        // Arrange
        var otroUsuarioId = 999;
        var negocio = new Negocio { Id = 1, PropietarioId = otroUsuarioId };
        var fotoMock = new Mock<IFormFile>();
        fotoMock.Setup(f => f.Length).Returns(1024);

        _mockNegocioRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(negocio);

        // Act
        var resultado = await _controller.SubirImagen(1, fotoMock.Object);

        // Assert
        resultado.Should().BeOfType<ObjectResult>();
        var objectResult = resultado as ObjectResult;
        objectResult?.StatusCode.Should().Be(403);
    }

    // ============================================================
    // Pruebas para EliminarFotoNegocio
    // ============================================================

    [Fact]
    public async Task EliminarFotoNegocio_ConPermiso_DeberiaEliminarYRetornarOk()
    {
        // Arrange
        var negocio = new Negocio { Id = 1, PropietarioId = _usuarioId, ImagenUrl = "/uploads/negocios/foto.jpg" };
        _mockNegocioRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(negocio);
        _mockFileService.Setup(f => f.EliminarImagen(It.IsAny<string>())).Returns(Task.CompletedTask);
        _mockNegocioRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var resultado = await _controller.EliminarFotoNegocio(1);

        // Assert
        resultado.Should().BeOfType<OkObjectResult>();
        negocio.ImagenUrl.Should().BeNull();
    }

    [Fact]
    public async Task EliminarFotoNegocio_SinFoto_DeberiaRetornarBadRequest()
    {
        // Arrange
        var negocio = new Negocio { Id = 1, PropietarioId = _usuarioId, ImagenUrl = null };
        _mockNegocioRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(negocio);

        // Act
        var resultado = await _controller.EliminarFotoNegocio(1);

        // Assert
        resultado.Should().BeOfType<BadRequestObjectResult>();
    }
}