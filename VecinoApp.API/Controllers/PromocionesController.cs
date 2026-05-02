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
    public class PromocionesController : ControllerBase
    {
        private readonly IPromocionRepository _promocionRepository;
        private readonly INegocioRepository _negocioRepository;

        public PromocionesController(
            IPromocionRepository promocionRepository,
            INegocioRepository negocioRepository)
        {
            _promocionRepository = promocionRepository;
            _negocioRepository = negocioRepository;
        }

        // GET: api/promociones
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PromocionDto>>> GetPromociones()
        {
            var promociones = await _promocionRepository.GetAllAsync();
            return Ok(promociones.Select(MapToDto));
        }

        // GET: api/promociones/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PromocionDto>> GetPromocion(int id)
        {
            var promocion = await _promocionRepository.GetByIdAsync(id);
            if (promocion == null)
                return NotFound();

            return Ok(MapToDto(promocion));
        }

        // GET: api/promociones/negocio/4
        [HttpGet("negocio/{negocioId}")]
        public async Task<ActionResult<IEnumerable<PromocionDto>>> GetPromocionesPorNegocio(int negocioId)
        {
            var promociones = await _promocionRepository.GetByNegocioAsync(negocioId);

            Console.WriteLine($"=== GET PROMOCIONES NEGOCIO {negocioId} ===");
            foreach (var p in promociones)
            {
                Console.WriteLine($"ID: {p.Id}, Descuento: {p.Descuento}");
            }

            return Ok(promociones.Select(MapToDto));
        }

        // GET: api/promociones/activas
        [HttpGet("activas")]
        public async Task<ActionResult<IEnumerable<PromocionDto>>> GetPromocionesActivas()
        {
            var promociones = await _promocionRepository.GetActivasAsync();
            return Ok(promociones.Select(MapToDto));
        }

        // POST: api/promociones
        [HttpPost]
        public async Task<IActionResult> CreatePromocion(CreatePromocionDto createDto)
        {
            var negocio = await _negocioRepository.GetByIdAsync(createDto.NegocioId);
            if (negocio == null)
                return BadRequest("Negocio no encontrado");

            if (createDto.FechaInicio > createDto.FechaFin)
                return BadRequest("La fecha de inicio no puede ser mayor que la fecha de fin");

            var promocion = new Promocion
            {
                NegocioId = createDto.NegocioId,
                Titulo = createDto.Titulo,
                Descripcion = createDto.Descripcion,
                Descuento = createDto.Descuento,
                FechaInicio = createDto.FechaInicio.Date,
                FechaFin = createDto.FechaFin.Date.AddDays(1).AddTicks(-1),
                Activa = createDto.Activa  // USAR EL VALOR DEL DTO
            };

            await _promocionRepository.AddAsync(promocion);
            await _promocionRepository.SaveChangesAsync();

            var dto = MapToDto(promocion);
            dto.NombreNegocio = negocio.Nombre;

            return CreatedAtAction(nameof(GetPromocion), new { id = promocion.Id }, dto);
        }

        // PUT: api/promociones/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePromocion(int id, UpdatePromocionDto updateDto)
        {
            Console.WriteLine($"=== UPDATE PROMOCION ID: {id} ===");
            Console.WriteLine($"Descuento recibido: {updateDto.Descuento}");
            Console.WriteLine($"Activa recibida: {updateDto.Activa}");

            var promocion = await _promocionRepository.GetByIdAsync(id);
            if (promocion == null)
                return NotFound();

            if (updateDto.FechaInicio > updateDto.FechaFin)
                return BadRequest("La fecha de inicio no puede ser mayor que la fecha de fin");

            // Actualizar todas las propiedades
            promocion.Titulo = updateDto.Titulo;
            promocion.Descripcion = updateDto.Descripcion;
            promocion.Descuento = updateDto.Descuento;
            promocion.FechaInicio = updateDto.FechaInicio.Date;
            promocion.FechaFin = updateDto.FechaFin.Date.AddDays(1).AddTicks(-1);
            promocion.Activa = updateDto.Activa;  // ACTUALIZAR ACTIVA

            Console.WriteLine($"Activa asignada: {promocion.Activa}");

            _promocionRepository.Update(promocion);
            await _promocionRepository.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/promociones/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePromocion(int id)
        {
            var promocion = await _promocionRepository.GetByIdAsync(id);
            if (promocion == null)
                return NotFound();

            _promocionRepository.Delete(promocion);
            await _promocionRepository.SaveChangesAsync();

            return Ok(new { message = "Promoción eliminada correctamente" });
        }

        // Método auxiliar para mapear entidad a DTO
        private PromocionDto MapToDto(Promocion p) => new PromocionDto
        {
            Id = p.Id,
            NegocioId = p.NegocioId,
            Titulo = p.Titulo,
            Descripcion = p.Descripcion,
            Descuento = p.Descuento,
            FechaInicio = p.FechaInicio,
            FechaFin = p.FechaFin,
            Activa = p.Activa,
            NombreNegocio = p.Negocio?.Nombre ?? "Sin negocio"
        };
    }
}