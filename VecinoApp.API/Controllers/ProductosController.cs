using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VecinoApp.Application.DTOs;
using VecinoApp.Domain.Entities;
using VecinoApp.Domain.Interfaces;

namespace VecinoApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductosController : ControllerBase
    {
        private readonly IProductoRepository _productoRepository;
        private readonly INegocioRepository _negocioRepository;

        public ProductosController(
            IProductoRepository productoRepository,
            INegocioRepository negocioRepository)
        {
            _productoRepository = productoRepository;
            _negocioRepository = negocioRepository;
        }

        // GET: api/productos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductoDto>>> GetProductos()
        {
            var productos = await _productoRepository.GetAllAsync();
            return Ok(productos.Select(MapToDto));
        }

        // GET: api/productos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductoDto>> GetProducto(int id)
        {
            var producto = await _productoRepository.GetByIdAsync(id);
            if (producto == null)
                return NotFound();

            return Ok(MapToDto(producto));
        }

        // GET: api/productos/negocio/4
        [HttpGet("negocio/{negocioId}")]
        public async Task<ActionResult<IEnumerable<ProductoDto>>> GetProductosPorNegocio(int negocioId)
        {
            var productos = await _productoRepository.GetByNegocioAsync(negocioId);
            return Ok(productos.Select(MapToDto));
        }

        // GET: api/productos/destacados
        [HttpGet("destacados")]
        public async Task<ActionResult<IEnumerable<ProductoDto>>> GetProductosDestacados()
        {
            var productos = await _productoRepository.GetDestacadosAsync();
            return Ok(productos.Select(MapToDto));
        }

        // POST: api/productos
        [HttpPost]
        public async Task<ActionResult<ProductoDto>> CreateProducto(CreateProductoDto createDto)
        {
            var negocio = await _negocioRepository.GetByIdAsync(createDto.NegocioId);
            if (negocio == null)
                return BadRequest("Negocio no encontrado");

            var producto = new Producto
            {
                NegocioId = createDto.NegocioId,
                Nombre = createDto.Nombre,
                Descripcion = createDto.Descripcion,
                Precio = createDto.Precio,
                Destacado = createDto.Destacado
            };

            await _productoRepository.AddAsync(producto);
            await _productoRepository.SaveChangesAsync();

            var dto = MapToDto(producto);
            dto.NombreNegocio = negocio.Nombre;

            return CreatedAtAction(nameof(GetProducto), new { id = producto.Id }, dto);
        }

        // PUT: api/productos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProducto(int id, UpdateProductoDto updateDto)
        {
            var producto = await _productoRepository.GetByIdAsync(id);
            if (producto == null)
                return NotFound();

            producto.Nombre = updateDto.Nombre;
            producto.Descripcion = updateDto.Descripcion;
            producto.Precio = updateDto.Precio;
            producto.Destacado = updateDto.Destacado;

            _productoRepository.Update(producto);
            await _productoRepository.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/productos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProducto(int id)
        {
            var producto = await _productoRepository.GetByIdAsync(id);
            if (producto == null)
                return NotFound();

            _productoRepository.Delete(producto);
            await _productoRepository.SaveChangesAsync();

            return NoContent();
        }

        // Método auxiliar para mapear entidad a DTO
        private ProductoDto MapToDto(Producto p) => new ProductoDto
        {
            Id = p.Id,
            NegocioId = p.NegocioId,
            Nombre = p.Nombre,
            Descripcion = p.Descripcion,
            Precio = p.Precio,
            Destacado = p.Destacado,
            NombreNegocio = p.Negocio?.Nombre ?? "Sin negocio"
        };
    }
}