using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using VecinoApp.API.Controllers;
using VecinoApp.Application.DTOs;
using VecinoApp.Domain.Entities;
using VecinoApp.Domain.Interfaces;

namespace VecinoApp.Tests.API.Controllers;

public class ProductosControllerTests
{
    private readonly Mock<IProductoRepository> _mockProductoRepo;
    private readonly Mock<INegocioRepository> _mockNegocioRepo;
    private readonly ProductosController _controller;

    public ProductosControllerTests()
    {
        _mockProductoRepo = new Mock<IProductoRepository>();
        _mockNegocioRepo = new Mock<INegocioRepository>();
        _controller = new ProductosController(
            _mockProductoRepo.Object,
            _mockNegocioRepo.Object);
    }

    // ============================================================
    // Pruebas para GetProductos
    // ============================================================

    [Fact]
    public async Task GetProductos_DeberiaRetornarOkConListaDeProductos()
    {
        // Arrange
        var productos = new List<Producto>
        {
            new Producto { Id = 1, Nombre = "Producto 1", Precio = 1000, Negocio = new Negocio { Nombre = "Negocio 1" } },
            new Producto { Id = 2, Nombre = "Producto 2", Precio = 2000, Negocio = new Negocio { Nombre = "Negocio 2" } }
        };
        _mockProductoRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(productos);

        // Act
        var resultado = await _controller.GetProductos();

        // Assert
        resultado.Result.Should().BeOfType<OkObjectResult>();
    }

    // ============================================================
    // Pruebas para GetProducto
    // ============================================================

    [Fact]
    public async Task GetProducto_CuandoExiste_DeberiaRetornarOk()
    {
        // Arrange
        var producto = new Producto
        {
            Id = 1,
            Nombre = "Producto Test",
            Precio = 1500,
            Negocio = new Negocio { Nombre = "Negocio Test" }
        };
        _mockProductoRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(producto);

        // Act
        var resultado = await _controller.GetProducto(1);

        // Assert
        resultado.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetProducto_CuandoNoExiste_DeberiaRetornarNotFound()
    {
        // Arrange
        _mockProductoRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Producto?)null);

        // Act
        var resultado = await _controller.GetProducto(999);

        // Assert
        resultado.Result.Should().BeOfType<NotFoundResult>();
    }

    // ============================================================
    // Pruebas para GetProductosPorNegocio
    // ============================================================

    [Fact]
    public async Task GetProductosPorNegocio_DeberiaRetornarOkConLista()
    {
        // Arrange
        var negocioId = 1;
        var productos = new List<Producto>
        {
            new Producto { Id = 1, NegocioId = negocioId, Nombre = "Producto 1" },
            new Producto { Id = 2, NegocioId = negocioId, Nombre = "Producto 2" }
        };
        _mockProductoRepo.Setup(r => r.GetByNegocioAsync(negocioId)).ReturnsAsync(productos);

        // Act
        var resultado = await _controller.GetProductosPorNegocio(negocioId);

        // Assert
        resultado.Result.Should().BeOfType<OkObjectResult>();
    }

    // ============================================================
    // Pruebas para GetProductosDestacados
    // ============================================================

    [Fact]
    public async Task GetProductosDestacados_DeberiaRetornarSoloProductosDestacados()
    {
        // Arrange
        var productos = new List<Producto>
        {
            new Producto { Id = 1, Destacado = true, Negocio = new Negocio { Nombre = "Negocio 1" } },
            new Producto { Id = 2, Destacado = false, Negocio = new Negocio { Nombre = "Negocio 2" } },
            new Producto { Id = 3, Destacado = true, Negocio = new Negocio { Nombre = "Negocio 3" } }
        };
        var productosDestacados = productos.Where(p => p.Destacado).ToList();

        _mockProductoRepo.Setup(r => r.GetDestacadosAsync()).ReturnsAsync(productosDestacados);

        // Act
        var resultado = await _controller.GetProductosDestacados();

        // Assert
        resultado.Result.Should().BeOfType<OkObjectResult>();
    }

    // ============================================================
    // Pruebas para CreateProducto
    // ============================================================

    [Fact]
    public async Task CreateProducto_ConDatosValidos_DeberiaCrearYRetornarCreated()
    {
        // Arrange
        var createDto = new CreateProductoDto
        {
            NegocioId = 1,
            Nombre = "Producto Nuevo",
            Descripcion = "Descripción del producto",
            Precio = 2500,
            Destacado = true
        };

        var negocio = new Negocio { Id = 1, Nombre = "Test Negocio" };
        var productoCreado = new Producto { Id = 1 };

        _mockNegocioRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(negocio);
        _mockProductoRepo.Setup(r => r.AddAsync(It.IsAny<Producto>())).Returns(Task.CompletedTask);
        _mockProductoRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var resultado = await _controller.CreateProducto(createDto);

        // Assert
        resultado.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task CreateProducto_NegocioNoExistente_DeberiaRetornarBadRequest()
    {
        // Arrange
        var createDto = new CreateProductoDto
        {
            NegocioId = 999,
            Nombre = "Producto",
            Descripcion = "Descripción",
            Precio = 1000,
            Destacado = true
        };

        _mockNegocioRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Negocio?)null);

        // Act
        var resultado = await _controller.CreateProducto(createDto);

        // Assert
        resultado.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateProducto_PrecioNulo_DeberiaCrearProductoCorrectamente()
    {
        // Arrange
        var createDto = new CreateProductoDto
        {
            NegocioId = 1,
            Nombre = "Producto Sin Precio",
            Descripcion = "Descripción",
            Precio = null,
            Destacado = false
        };

        var negocio = new Negocio { Id = 1, Nombre = "Test Negocio" };

        _mockNegocioRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(negocio);
        _mockProductoRepo.Setup(r => r.AddAsync(It.IsAny<Producto>())).Returns(Task.CompletedTask);
        _mockProductoRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var resultado = await _controller.CreateProducto(createDto);

        // Assert
        resultado.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    // ============================================================
    // Pruebas para UpdateProducto
    // ============================================================

    [Fact]
    public async Task UpdateProducto_ConDatosValidos_DeberiaActualizarYRetornarNoContent()
    {
        // Arrange
        var producto = new Producto
        {
            Id = 1,
            NegocioId = 1,
            Nombre = "Nombre Antiguo",
            Descripcion = "Descripción Antigua",
            Precio = 1000,
            Destacado = false
        };

        var updateDto = new UpdateProductoDto
        {
            Nombre = "Nombre Nuevo",
            Descripcion = "Descripción Nueva",
            Precio = 2500,
            Destacado = true
        };

        _mockProductoRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(producto);
        _mockProductoRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var resultado = await _controller.UpdateProducto(1, updateDto);

        // Assert
        resultado.Should().BeOfType<NoContentResult>();
        producto.Nombre.Should().Be("Nombre Nuevo");
        producto.Descripcion.Should().Be("Descripción Nueva");
        producto.Precio.Should().Be(2500);
        producto.Destacado.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateProducto_ProductoNoExistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var updateDto = new UpdateProductoDto
        {
            Nombre = "Producto",
            Descripcion = "Descripción",
            Precio = 1000,
            Destacado = true
        };

        _mockProductoRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Producto?)null);

        // Act
        var resultado = await _controller.UpdateProducto(999, updateDto);

        // Assert
        resultado.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task UpdateProducto_PrecioNulo_DeberiaActualizarCorrectamente()
    {
        // Arrange
        var producto = new Producto
        {
            Id = 1,
            Nombre = "Nombre Antiguo",
            Descripcion = "Descripción Antigua",
            Precio = 1000,
            Destacado = true
        };

        var updateDto = new UpdateProductoDto
        {
            Nombre = "Nombre Nuevo",
            Descripcion = "Descripción Nueva",
            Precio = null,
            Destacado = false
        };

        _mockProductoRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(producto);
        _mockProductoRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var resultado = await _controller.UpdateProducto(1, updateDto);

        // Assert
        resultado.Should().BeOfType<NoContentResult>();
        producto.Precio.Should().BeNull();
    }

    // ============================================================
    // Pruebas para DeleteProducto
    // ============================================================

    [Fact]
    public async Task DeleteProducto_ProductoExistente_DeberiaEliminarYRetornarNoContent()
    {
        // Arrange
        var producto = new Producto { Id = 1 };
        _mockProductoRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(producto);
        _mockProductoRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var resultado = await _controller.DeleteProducto(1);

        // Assert
        resultado.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteProducto_ProductoNoExistente_DeberiaRetornarNotFound()
    {
        // Arrange
        _mockProductoRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Producto?)null);

        // Act
        var resultado = await _controller.DeleteProducto(999);

        // Assert
        resultado.Should().BeOfType<NotFoundResult>();
    }
}