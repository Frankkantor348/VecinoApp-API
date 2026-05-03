using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using VecinoApp.API.Controllers;
using VecinoApp.Application.DTOs;
using VecinoApp.Domain.Entities;
using VecinoApp.Domain.Interfaces;

namespace VecinoApp.Tests.API.Controllers;

public class PromocionesControllerTests
{
    private readonly Mock<IPromocionRepository> _mockPromocionRepo;
    private readonly Mock<INegocioRepository> _mockNegocioRepo;
    private readonly PromocionesController _controller;

    public PromocionesControllerTests()
    {
        _mockPromocionRepo = new Mock<IPromocionRepository>();
        _mockNegocioRepo = new Mock<INegocioRepository>();
        _controller = new PromocionesController(
            _mockPromocionRepo.Object,
            _mockNegocioRepo.Object);
    }

    // ============================================================
    // Pruebas para GetPromociones
    // ============================================================

    [Fact]
    public async Task GetPromociones_DeberiaRetornarOkConListaDePromociones()
    {
        // Arrange
        var promociones = new List<Promocion>
        {
            new Promocion { Id = 1, Titulo = "Promo 1", Descuento = 10, Negocio = new Negocio { Nombre = "Negocio 1" } },
            new Promocion { Id = 2, Titulo = "Promo 2", Descuento = 20, Negocio = new Negocio { Nombre = "Negocio 2" } }
        };
        _mockPromocionRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(promociones);

        // Act
        var resultado = await _controller.GetPromociones();

        // Assert
        resultado.Result.Should().BeOfType<OkObjectResult>();
    }

    // ============================================================
    // Pruebas para GetPromocion
    // ============================================================

    [Fact]
    public async Task GetPromocion_CuandoExiste_DeberiaRetornarOk()
    {
        // Arrange
        var promocion = new Promocion
        {
            Id = 1,
            Titulo = "Promo Test",
            Descuento = 15,
            Negocio = new Negocio { Nombre = "Negocio Test" }
        };
        _mockPromocionRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(promocion);

        // Act
        var resultado = await _controller.GetPromocion(1);

        // Assert
        resultado.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetPromocion_CuandoNoExiste_DeberiaRetornarNotFound()
    {
        // Arrange
        _mockPromocionRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Promocion?)null);

        // Act
        var resultado = await _controller.GetPromocion(999);

        // Assert
        resultado.Result.Should().BeOfType<NotFoundResult>();
    }

    // ============================================================
    // Pruebas para GetPromocionesPorNegocio
    // ============================================================

    [Fact]
    public async Task GetPromocionesPorNegocio_DeberiaRetornarOkConLista()
    {
        // Arrange
        var negocioId = 1;
        var promociones = new List<Promocion>
        {
            new Promocion { Id = 1, NegocioId = negocioId, Titulo = "Promo 1" }
        };
        _mockPromocionRepo.Setup(r => r.GetByNegocioAsync(negocioId)).ReturnsAsync(promociones);

        // Act
        var resultado = await _controller.GetPromocionesPorNegocio(negocioId);

        // Assert
        resultado.Result.Should().BeOfType<OkObjectResult>();
    }

    // ============================================================
    // Pruebas para GetPromocionesActivas
    // ============================================================

    [Fact]
    public async Task GetPromocionesActivas_DeberiaRetornarSoloPromocionesActivas()
    {
        // Arrange
        var promociones = new List<Promocion>
        {
            new Promocion { Id = 1, Activa = true, Negocio = new Negocio { Nombre = "Negocio 1" } },
            new Promocion { Id = 2, Activa = false, Negocio = new Negocio { Nombre = "Negocio 2" } }
        };
        _mockPromocionRepo.Setup(r => r.GetActivasAsync()).ReturnsAsync(promociones.Where(p => p.Activa));

        // Act
        var resultado = await _controller.GetPromocionesActivas();

        // Assert
        resultado.Result.Should().BeOfType<OkObjectResult>();
    }

    // ============================================================
    // Pruebas para CreatePromocion
    // ============================================================

    [Fact]
    public async Task CreatePromocion_ConDatosValidos_DeberiaCrearYRetornarCreated()
    {
        // Arrange
        var createDto = new CreatePromocionDto
        {
            NegocioId = 1,
            Titulo = "Descuento de Verano",
            Descripcion = "20% de descuento",
            Descuento = 20,
            FechaInicio = DateTime.UtcNow.Date,
            FechaFin = DateTime.UtcNow.Date.AddDays(30),
            Activa = true
        };

        var negocio = new Negocio { Id = 1, Nombre = "Test Negocio" };
        var promocionCreada = new Promocion { Id = 1 };

        _mockNegocioRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(negocio);
        _mockPromocionRepo.Setup(r => r.AddAsync(It.IsAny<Promocion>())).Returns(Task.CompletedTask);
        _mockPromocionRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var resultado = await _controller.CreatePromocion(createDto);

        // Assert
        resultado.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task CreatePromocion_NegocioNoExistente_DeberiaRetornarBadRequest()
    {
        // Arrange
        var createDto = new CreatePromocionDto
        {
            NegocioId = 999,
            Titulo = "Promo",
            Descripcion = "Descripción",
            Descuento = 10,
            FechaInicio = DateTime.UtcNow.Date,
            FechaFin = DateTime.UtcNow.Date.AddDays(30),
            Activa = true
        };

        _mockNegocioRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Negocio?)null);

        // Act
        var resultado = await _controller.CreatePromocion(createDto);

        // Assert
        resultado.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreatePromocion_FechaInicioMayorQueFechaFin_DeberiaRetornarBadRequest()
    {
        // Arrange
        var createDto = new CreatePromocionDto
        {
            NegocioId = 1,
            Titulo = "Promo",
            Descripcion = "Descripción",
            Descuento = 10,
            FechaInicio = DateTime.UtcNow.Date.AddDays(30),
            FechaFin = DateTime.UtcNow.Date,
            Activa = true
        };

        var negocio = new Negocio { Id = 1, Nombre = "Test Negocio" };
        _mockNegocioRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(negocio);

        // Act
        var resultado = await _controller.CreatePromocion(createDto);

        // Assert
        resultado.Should().BeOfType<BadRequestObjectResult>();
    }

    // ============================================================
    // Pruebas para UpdatePromocion
    // ============================================================

    [Fact]
    public async Task UpdatePromocion_ConDatosValidos_DeberiaActualizarYRetornarNoContent()
    {
        // Arrange
        var promocion = new Promocion
        {
            Id = 1,
            NegocioId = 1,
            Titulo = "Título Antiguo",
            Descripcion = "Descripción Antigua",
            Descuento = 10,
            FechaInicio = DateTime.UtcNow.Date,
            FechaFin = DateTime.UtcNow.Date.AddDays(30),
            Activa = false
        };

        var updateDto = new UpdatePromocionDto
        {
            Titulo = "Título Nuevo",
            Descripcion = "Descripción Nueva",
            Descuento = 25,
            FechaInicio = DateTime.UtcNow.Date,
            FechaFin = DateTime.UtcNow.Date.AddDays(15),
            Activa = true
        };

        _mockPromocionRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(promocion);
        _mockPromocionRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var resultado = await _controller.UpdatePromocion(1, updateDto);

        // Assert
        resultado.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task UpdatePromocion_PromocionNoExistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var updateDto = new UpdatePromocionDto
        {
            Titulo = "Título",
            Descripcion = "Descripción",
            Descuento = 10,
            FechaInicio = DateTime.UtcNow.Date,
            FechaFin = DateTime.UtcNow.Date.AddDays(30),
            Activa = true
        };

        _mockPromocionRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Promocion?)null);

        // Act
        var resultado = await _controller.UpdatePromocion(999, updateDto);

        // Assert
        resultado.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task UpdatePromocion_FechaInicioMayorQueFechaFin_DeberiaRetornarBadRequest()
    {
        // Arrange
        var promocion = new Promocion { Id = 1 };
        var updateDto = new UpdatePromocionDto
        {
            Titulo = "Título",
            Descripcion = "Descripción",
            Descuento = 10,
            FechaInicio = DateTime.UtcNow.Date.AddDays(30),
            FechaFin = DateTime.UtcNow.Date,
            Activa = true
        };

        _mockPromocionRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(promocion);

        // Act
        var resultado = await _controller.UpdatePromocion(1, updateDto);

        // Assert
        resultado.Should().BeOfType<BadRequestObjectResult>();
    }

    // ============================================================
    // Pruebas para DeletePromocion
    // ============================================================

    [Fact]
    public async Task DeletePromocion_PromocionExistente_DeberiaEliminarYRetornarOk()
    {
        // Arrange
        var promocion = new Promocion { Id = 1 };
        _mockPromocionRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(promocion);
        _mockPromocionRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var resultado = await _controller.DeletePromocion(1);

        // Assert
        resultado.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeletePromocion_PromocionNoExistente_DeberiaRetornarNotFound()
    {
        // Arrange
        _mockPromocionRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Promocion?)null);

        // Act
        var resultado = await _controller.DeletePromocion(999);

        // Assert
        resultado.Should().BeOfType<NotFoundResult>();
    }
}