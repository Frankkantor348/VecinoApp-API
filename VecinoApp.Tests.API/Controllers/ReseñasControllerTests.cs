using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VecinoApp.API.Controllers;
using VecinoApp.Application.DTOs;
using VecinoApp.Domain.Entities;
using VecinoApp.Domain.Interfaces;

namespace VecinoApp.Tests.API.Controllers;

public class ReseñasControllerTests
{
    private readonly Mock<IReseñaRepository> _mockReseñaRepo;
    private readonly Mock<INegocioRepository> _mockNegocioRepo;
    private readonly Mock<UserManager<Usuario>> _mockUserManager;
    private readonly Mock<ILogger<ReseñasController>> _mockLogger;
    private readonly ReseñasController _controller;

    public ReseñasControllerTests()
    {
        _mockReseñaRepo = new Mock<IReseñaRepository>();
        _mockNegocioRepo = new Mock<INegocioRepository>();

        var store = new Mock<IUserStore<Usuario>>();
        _mockUserManager = new Mock<UserManager<Usuario>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _mockLogger = new Mock<ILogger<ReseñasController>>();

        _controller = new ReseñasController(
            _mockReseñaRepo.Object,
            _mockNegocioRepo.Object,
            _mockUserManager.Object,
            _mockLogger.Object);
    }

    // ============================================================
    // Pruebas para GetReseñas
    // ============================================================

    [Fact]
    public async Task GetReseñas_CuandoExistenReseñas_DeberiaRetornarOk()
    {
        // Arrange
        var reseñas = new List<Reseña>
        {
            new Reseña { Id = 1, Calificacion = 5, Comentario = "Excelente" },
            new Reseña { Id = 2, Calificacion = 4, Comentario = "Muy bueno" }
        };
        _mockReseñaRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(reseñas);

        // Act
        var resultado = await _controller.GetReseñas();

        // Assert
        resultado.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetReseñas_CuandoOcurreError_DeberiaRetornar500()
    {
        // Arrange
        _mockReseñaRepo.Setup(r => r.GetAllAsync()).ThrowsAsync(new Exception("Error de BD"));

        // Act
        var resultado = await _controller.GetReseñas();

        // Assert
        resultado.Result.Should().BeOfType<ObjectResult>();
        var objectResult = resultado.Result as ObjectResult;
        objectResult?.StatusCode.Should().Be(500);
    }

    // ============================================================
    // Pruebas para GetReseña
    // ============================================================

    [Fact]
    public async Task GetReseña_CuandoExiste_DeberiaRetornarOk()
    {
        // Arrange
        var reseña = new Reseña { Id = 1, Calificacion = 5, Comentario = "Excelente" };
        _mockReseñaRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(reseña);

        // Act
        var resultado = await _controller.GetReseña(1);

        // Assert
        resultado.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetReseña_CuandoNoExiste_DeberiaRetornarNotFound()
    {
        // Arrange
        _mockReseñaRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Reseña?)null);

        // Act
        var resultado = await _controller.GetReseña(999);

        // Assert
        resultado.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ============================================================
    // Pruebas para GetReseñasPorNegocio
    // ============================================================

    [Fact]
    public async Task GetReseñasPorNegocio_NegocioExistente_DeberiaRetornarOk()
    {
        // Arrange
        var negocioId = 1;
        var negocio = new Negocio { Id = negocioId, Nombre = "Test Negocio" };
        var reseñas = new List<Reseña>();

        _mockNegocioRepo.Setup(r => r.GetByIdAsync(negocioId)).ReturnsAsync(negocio);
        _mockReseñaRepo.Setup(r => r.GetByNegocioAsync(negocioId)).ReturnsAsync(reseñas);

        // Act
        var resultado = await _controller.GetReseñasPorNegocio(negocioId);

        // Assert
        resultado.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetReseñasPorNegocio_NegocioNoExistente_DeberiaRetornarBadRequest()
    {
        // Arrange
        var negocioId = 999;
        _mockNegocioRepo.Setup(r => r.GetByIdAsync(negocioId)).ReturnsAsync((Negocio?)null);

        // Act
        var resultado = await _controller.GetReseñasPorNegocio(negocioId);

        // Assert
        resultado.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ============================================================
    // Pruebas para GetReseñasPorUsuario
    // ============================================================

    [Fact]
    public async Task GetReseñasPorUsuario_UsuarioExistente_DeberiaRetornarOk()
    {
        // Arrange
        var usuarioId = 1;
        var usuario = new Usuario { Id = usuarioId, Nombre = "Test User" };
        var reseñas = new List<Reseña>();

        _mockUserManager.Setup(u => u.FindByIdAsync(usuarioId.ToString())).ReturnsAsync(usuario);
        _mockReseñaRepo.Setup(r => r.GetByUsuarioAsync(usuarioId)).ReturnsAsync(reseñas);

        // Act
        var resultado = await _controller.GetReseñasPorUsuario(usuarioId);

        // Assert
        resultado.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetReseñasPorUsuario_UsuarioNoExistente_DeberiaRetornarBadRequest()
    {
        // Arrange
        var usuarioId = 999;
        _mockUserManager.Setup(u => u.FindByIdAsync(usuarioId.ToString())).ReturnsAsync((Usuario?)null);

        // Act
        var resultado = await _controller.GetReseñasPorUsuario(usuarioId);

        // Assert
        resultado.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ============================================================
    // Pruebas para CreateReseña
    // ============================================================

    [Fact]
    public async Task CreateReseña_ConDatosValidos_DeberiaCrearYRetornarCreated()
    {
        // Arrange
        var createDto = new CreateReseñaDto
        {
            UsuarioId = 1,
            NegocioId = 1,
            Calificacion = 5,
            Comentario = "Excelente servicio!"
        };

        var usuario = new Usuario { Id = 1, Nombre = "Test User" };
        var negocio = new Negocio { Id = 1, Nombre = "Test Negocio" };

        _mockUserManager.Setup(u => u.FindByIdAsync("1")).ReturnsAsync(usuario);
        _mockNegocioRepo.Setup(n => n.GetByIdAsync(1)).ReturnsAsync(negocio);
        _mockReseñaRepo.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Reseña, bool>>>()))
            .ReturnsAsync(false);
        _mockReseñaRepo.Setup(r => r.AddAsync(It.IsAny<Reseña>())).Returns(Task.CompletedTask);
        _mockReseñaRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var resultado = await _controller.CreateReseña(createDto);

        // Assert
        resultado.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task CreateReseña_UsuarioIdInvalido_DeberiaRetornarBadRequest()
    {
        // Arrange
        var createDto = new CreateReseñaDto
        {
            UsuarioId = 0,
            NegocioId = 1,
            Calificacion = 5,
            Comentario = "Comentario"
        };

        // Act
        var resultado = await _controller.CreateReseña(createDto);

        // Assert
        resultado.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateReseña_NegocioIdInvalido_DeberiaRetornarBadRequest()
    {
        // Arrange
        var createDto = new CreateReseñaDto
        {
            UsuarioId = 1,
            NegocioId = 0,
            Calificacion = 5,
            Comentario = "Comentario"
        };

        // Act
        var resultado = await _controller.CreateReseña(createDto);

        // Assert
        resultado.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateReseña_CalificacionInvalida_DeberiaRetornarBadRequest()
    {
        // Arrange
        var createDto = new CreateReseñaDto
        {
            UsuarioId = 1,
            NegocioId = 1,
            Calificacion = 6,
            Comentario = "Comentario"
        };

        // Act
        var resultado = await _controller.CreateReseña(createDto);

        // Assert
        resultado.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateReseña_SinComentario_DeberiaRetornarBadRequest()
    {
        // Arrange
        var createDto = new CreateReseñaDto
        {
            UsuarioId = 1,
            NegocioId = 1,
            Calificacion = 5,
            Comentario = ""
        };

        // Act
        var resultado = await _controller.CreateReseña(createDto);

        // Assert
        resultado.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateReseña_ComentarioMuyLargo_DeberiaRetornarBadRequest()
    {
        // Arrange
        var createDto = new CreateReseñaDto
        {
            UsuarioId = 1,
            NegocioId = 1,
            Calificacion = 5,
            Comentario = new string('a', 1001)
        };

        // Act
        var resultado = await _controller.CreateReseña(createDto);

        // Assert
        resultado.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateReseña_UsuarioNoExistente_DeberiaRetornarBadRequest()
    {
        // Arrange
        var createDto = new CreateReseñaDto
        {
            UsuarioId = 999,
            NegocioId = 1,
            Calificacion = 5,
            Comentario = "Comentario"
        };

        _mockUserManager.Setup(u => u.FindByIdAsync("999")).ReturnsAsync((Usuario?)null);

        // Act
        var resultado = await _controller.CreateReseña(createDto);

        // Assert
        resultado.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateReseña_NegocioNoExistente_DeberiaRetornarBadRequest()
    {
        // Arrange
        var createDto = new CreateReseñaDto
        {
            UsuarioId = 1,
            NegocioId = 999,
            Calificacion = 5,
            Comentario = "Comentario"
        };

        var usuario = new Usuario { Id = 1, Nombre = "Test User" };

        _mockUserManager.Setup(u => u.FindByIdAsync("1")).ReturnsAsync(usuario);
        _mockNegocioRepo.Setup(n => n.GetByIdAsync(999)).ReturnsAsync((Negocio?)null);

        // Act
        var resultado = await _controller.CreateReseña(createDto);

        // Assert
        resultado.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateReseña_ReseñaDuplicada_DeberiaRetornarConflict()
    {
        // Arrange
        var createDto = new CreateReseñaDto
        {
            UsuarioId = 1,
            NegocioId = 1,
            Calificacion = 5,
            Comentario = "Comentario"
        };

        var usuario = new Usuario { Id = 1, Nombre = "Test User" };
        var negocio = new Negocio { Id = 1, Nombre = "Test Negocio" };

        _mockUserManager.Setup(u => u.FindByIdAsync("1")).ReturnsAsync(usuario);
        _mockNegocioRepo.Setup(n => n.GetByIdAsync(1)).ReturnsAsync(negocio);
        _mockReseñaRepo.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Reseña, bool>>>()))
            .ReturnsAsync(true);

        // Act
        var resultado = await _controller.CreateReseña(createDto);

        // Assert
        resultado.Result.Should().BeOfType<ConflictObjectResult>();
    }

    // ============================================================
    // Pruebas para UpdateReseña
    // ============================================================

    [Fact]
    public async Task UpdateReseña_ConDatosValidos_DeberiaActualizarYRetornarNoContent()
    {
        // Arrange
        var reseña = new Reseña
        {
            Id = 1,
            Calificacion = 3,
            Comentario = "Comentario antiguo"
        };
        var updateDto = new UpdateReseñaDto
        {
            Calificacion = 5,
            Comentario = "Comentario nuevo"
        };

        _mockReseñaRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(reseña);
        _mockReseñaRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var resultado = await _controller.UpdateReseña(1, updateDto);

        // Assert
        resultado.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task UpdateReseña_ReseñaNoExistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var updateDto = new UpdateReseñaDto
        {
            Calificacion = 5,
            Comentario = "Comentario"
        };

        _mockReseñaRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Reseña?)null);

        // Act
        var resultado = await _controller.UpdateReseña(999, updateDto);

        // Assert
        resultado.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateReseña_CalificacionInvalida_DeberiaRetornarBadRequest()
    {
        // Arrange
        var updateDto = new UpdateReseñaDto
        {
            Calificacion = 6,
            Comentario = "Comentario"
        };

        // Act
        var resultado = await _controller.UpdateReseña(1, updateDto);

        // Assert
        resultado.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateReseña_SinComentario_DeberiaRetornarBadRequest()
    {
        // Arrange
        var updateDto = new UpdateReseñaDto
        {
            Calificacion = 5,
            Comentario = ""
        };

        // Act
        var resultado = await _controller.UpdateReseña(1, updateDto);

        // Assert
        resultado.Should().BeOfType<BadRequestObjectResult>();
    }

    // ============================================================
    // Pruebas para DeleteReseña
    // ============================================================

    [Fact]
    public async Task DeleteReseña_ReseñaExistente_DeberiaEliminarYRetornarNoContent()
    {
        // Arrange
        var reseña = new Reseña { Id = 1 };
        _mockReseñaRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(reseña);
        _mockReseñaRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var resultado = await _controller.DeleteReseña(1);

        // Assert
        resultado.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteReseña_ReseñaNoExistente_DeberiaRetornarNotFound()
    {
        // Arrange
        _mockReseñaRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Reseña?)null);

        // Act
        var resultado = await _controller.DeleteReseña(999);

        // Assert
        resultado.Should().BeOfType<NotFoundObjectResult>();
    }
}