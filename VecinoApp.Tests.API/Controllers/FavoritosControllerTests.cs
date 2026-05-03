using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using VecinoApp.API.Controllers;
using VecinoApp.Application.DTOs;
using VecinoApp.Domain.Entities;
using VecinoApp.Domain.Interfaces;

namespace VecinoApp.Tests.API.Controllers;

public class FavoritosControllerTests
{
    private readonly Mock<IFavoritoRepository> _mockFavoritoRepo;
    private readonly Mock<INegocioRepository> _mockNegocioRepo;
    private readonly Mock<UserManager<Usuario>> _mockUserManager;
    private readonly FavoritosController _controller;

    public FavoritosControllerTests()
    {
        _mockFavoritoRepo = new Mock<IFavoritoRepository>();
        _mockNegocioRepo = new Mock<INegocioRepository>();

        var store = new Mock<IUserStore<Usuario>>();
        _mockUserManager = new Mock<UserManager<Usuario>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _controller = new FavoritosController(
            _mockFavoritoRepo.Object,
            _mockNegocioRepo.Object,
            _mockUserManager.Object);
    }

    // ============================================================
    // Pruebas para GetFavoritos
    // ============================================================

    [Fact]
    public async Task GetFavoritos_DeberiaRetornarOkConLista()
    {
        // Arrange
        var favoritos = new List<Favorito>
        {
            new Favorito { Id = 1, UsuarioId = 1, NegocioId = 1 },
            new Favorito { Id = 2, UsuarioId = 1, NegocioId = 2 }
        };
        _mockFavoritoRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(favoritos);

        // Act
        var resultado = await _controller.GetFavoritos();

        // Assert
        resultado.Result.Should().BeOfType<OkObjectResult>();
    }

    // ============================================================
    // Pruebas para GetFavoritosPorUsuario
    // ============================================================

    [Fact]
    public async Task GetFavoritosPorUsuario_DeberiaRetornarOk()
    {
        // Arrange
        var usuarioId = 1;
        var favoritos = new List<Favorito>
        {
            new Favorito { Id = 1, UsuarioId = usuarioId, NegocioId = 1 }
        };
        _mockFavoritoRepo.Setup(r => r.GetByUsuarioAsync(usuarioId)).ReturnsAsync(favoritos);

        // Act
        var resultado = await _controller.GetFavoritosPorUsuario(usuarioId);

        // Assert
        resultado.Result.Should().BeOfType<OkObjectResult>();
    }

    // ============================================================
    // Pruebas para GetNegociosFavoritosPorUsuario
    // ============================================================

    [Fact]
    public async Task GetNegociosFavoritosPorUsuario_DeberiaRetornarOkConNegocios()
    {
        // Arrange
        var usuarioId = 1;
        var negocios = new List<Negocio>
        {
            new Negocio
            {
                Id = 1,
                Nombre = "Negocio 1",
                Reseñas = new List<Reseña>()
            }
        };
        _mockFavoritoRepo.Setup(r => r.GetNegociosFavoritosByUsuarioAsync(usuarioId))
            .ReturnsAsync(negocios);

        // Act
        var resultado = await _controller.GetNegociosFavoritosPorUsuario(usuarioId);

        // Assert
        resultado.Result.Should().BeOfType<OkObjectResult>();
    }

    // ============================================================
    // Pruebas para GetFavoritosPorNegocio
    // ============================================================

    [Fact]
    public async Task GetFavoritosPorNegocio_DeberiaRetornarOk()
    {
        // Arrange
        var negocioId = 1;
        var favoritos = new List<Favorito>
        {
            new Favorito { Id = 1, UsuarioId = 1, NegocioId = negocioId }
        };
        _mockFavoritoRepo.Setup(r => r.GetByNegocioAsync(negocioId)).ReturnsAsync(favoritos);

        // Act
        var resultado = await _controller.GetFavoritosPorNegocio(negocioId);

        // Assert
        resultado.Result.Should().BeOfType<OkObjectResult>();
    }

    // ============================================================
    // Pruebas para VerificarFavorito
    // ============================================================

    [Fact]
    public async Task VerificarFavorito_CuandoExiste_DeberiaRetornarTrue()
    {
        // Arrange
        var usuarioId = 1;
        var negocioId = 1;
        _mockFavoritoRepo.Setup(r => r.EsFavoritoAsync(usuarioId, negocioId))
            .ReturnsAsync(true);

        // Act
        var resultado = await _controller.VerificarFavorito(usuarioId, negocioId);

        // Assert
        resultado.Result.Should().BeOfType<OkObjectResult>();
        var okResult = resultado.Result as OkObjectResult;
        okResult?.Value.Should().Be(true);
    }

    [Fact]
    public async Task VerificarFavorito_CuandoNoExiste_DeberiaRetornarFalse()
    {
        // Arrange
        var usuarioId = 1;
        var negocioId = 1;
        _mockFavoritoRepo.Setup(r => r.EsFavoritoAsync(usuarioId, negocioId))
            .ReturnsAsync(false);

        // Act
        var resultado = await _controller.VerificarFavorito(usuarioId, negocioId);

        // Assert
        resultado.Result.Should().BeOfType<OkObjectResult>();
        var okResult = resultado.Result as OkObjectResult;
        okResult?.Value.Should().Be(false);
    }

    // ============================================================
    // Pruebas para CreateFavorito
    // ============================================================

    [Fact]
    public async Task CreateFavorito_ConDatosValidos_DeberiaCrearYRetornarCreated()
    {
        // Arrange
        var createDto = new CreateFavoritoDto
        {
            UsuarioId = 1,
            NegocioId = 1
        };

        var usuario = new Usuario { Id = 1, Nombre = "Test User" };
        var negocio = new Negocio { Id = 1, Nombre = "Test Negocio", Direccion = "Calle 123" };

        _mockUserManager.Setup(u => u.FindByIdAsync("1")).ReturnsAsync(usuario);
        _mockNegocioRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(negocio);
        _mockFavoritoRepo.Setup(r => r.EsFavoritoAsync(1, 1)).ReturnsAsync(false);
        _mockFavoritoRepo.Setup(r => r.AddAsync(It.IsAny<Favorito>())).Returns(Task.CompletedTask);
        _mockFavoritoRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var resultado = await _controller.CreateFavorito(createDto);

        // Assert
        resultado.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task CreateFavorito_UsuarioNoExistente_DeberiaRetornarBadRequest()
    {
        // Arrange
        var createDto = new CreateFavoritoDto
        {
            UsuarioId = 999,
            NegocioId = 1
        };

        _mockUserManager.Setup(u => u.FindByIdAsync("999")).ReturnsAsync((Usuario?)null);

        // Act
        var resultado = await _controller.CreateFavorito(createDto);

        // Assert
        resultado.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateFavorito_NegocioNoExistente_DeberiaRetornarBadRequest()
    {
        // Arrange
        var createDto = new CreateFavoritoDto
        {
            UsuarioId = 1,
            NegocioId = 999
        };

        var usuario = new Usuario { Id = 1, Nombre = "Test User" };

        _mockUserManager.Setup(u => u.FindByIdAsync("1")).ReturnsAsync(usuario);
        _mockNegocioRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Negocio?)null);

        // Act
        var resultado = await _controller.CreateFavorito(createDto);

        // Assert
        resultado.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateFavorito_CuandoYaExiste_DeberiaRetornarBadRequest()
    {
        // Arrange
        var createDto = new CreateFavoritoDto
        {
            UsuarioId = 1,
            NegocioId = 1
        };

        var usuario = new Usuario { Id = 1, Nombre = "Test User" };
        var negocio = new Negocio { Id = 1, Nombre = "Test Negocio" };

        _mockUserManager.Setup(u => u.FindByIdAsync("1")).ReturnsAsync(usuario);
        _mockNegocioRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(negocio);
        _mockFavoritoRepo.Setup(r => r.EsFavoritoAsync(1, 1)).ReturnsAsync(true);

        // Act
        var resultado = await _controller.CreateFavorito(createDto);

        // Assert
        resultado.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ============================================================
    // Pruebas para DeleteFavorito (por ID)
    // ============================================================

    [Fact]
    public async Task DeleteFavorito_FavoritoExistente_DeberiaEliminarYRetornarNoContent()
    {
        // Arrange
        var favorito = new Favorito { Id = 1 };
        _mockFavoritoRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(favorito);
        _mockFavoritoRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var resultado = await _controller.DeleteFavorito(1);

        // Assert
        resultado.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteFavorito_FavoritoNoExistente_DeberiaRetornarNotFound()
    {
        // Arrange
        _mockFavoritoRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Favorito?)null);

        // Act
        var resultado = await _controller.DeleteFavorito(999);

        // Assert
        resultado.Should().BeOfType<NotFoundResult>();
    }

    // ============================================================
    // Pruebas para DeleteFavoritoPorUsuarioYNegocio
    // ============================================================

    [Fact]
    public async Task DeleteFavoritoPorUsuarioYNegocio_CuandoExiste_DeberiaEliminarYRetornarNoContent()
    {
        // Arrange
        var usuarioId = 1;
        var negocioId = 1;

        _mockFavoritoRepo.Setup(r => r.EsFavoritoAsync(usuarioId, negocioId))
            .ReturnsAsync(true);
        _mockFavoritoRepo.Setup(r => r.RemoveAsync(usuarioId, negocioId))
            .Returns(Task.CompletedTask);
        _mockFavoritoRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var resultado = await _controller.DeleteFavoritoPorUsuarioYNegocio(usuarioId, negocioId);

        // Assert
        resultado.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteFavoritoPorUsuarioYNegocio_CuandoNoExiste_DeberiaRetornarNotFound()
    {
        // Arrange
        var usuarioId = 1;
        var negocioId = 999;

        _mockFavoritoRepo.Setup(r => r.EsFavoritoAsync(usuarioId, negocioId))
            .ReturnsAsync(false);

        // Act
        var resultado = await _controller.DeleteFavoritoPorUsuarioYNegocio(usuarioId, negocioId);

        // Assert
        resultado.Should().BeOfType<NotFoundResult>();
    }
}