using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.Security.Claims;
using VecinoApp.Application.DTOs;
using VecinoApp.Domain.Entities;
using VecinoApp.Domain.Interfaces;
using VecinoApp.Infrastructure.Services;

namespace VecinoApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NegociosController : ControllerBase
    {
        private readonly INegocioRepository _negocioRepository;
        private readonly UserManager<Usuario> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IFileService _fileService;

        public NegociosController(
            INegocioRepository negocioRepository,
            UserManager<Usuario> userManager,
            IWebHostEnvironment webHostEnvironment,
            IFileService fileService)
        {
            _negocioRepository = negocioRepository;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _fileService = fileService;
        }
        // GET: api/negocios/categorias
        [HttpGet("categorias")]
        public async Task<ActionResult<IEnumerable<string>>> GetCategorias()
        {
            var categorias = await _negocioRepository.GetCategoriasAsync();

          
            return Ok(categorias);
        }

        // GET: api/negocios/tipo/{tipo}
        [HttpGet("tipo/{tipo}")]
        public async Task<ActionResult<IEnumerable<NegocioDto>>> GetNegociosPorTipo(string tipo)
        {
            var negocios = await _negocioRepository.GetByTipoAsync(tipo);
            return Ok(negocios.Select(MapToDto));
        }

        [HttpGet("tipos-test")]
        public async Task<IActionResult> TestTipos()
        {
            var todos = await _negocioRepository.GetAllAsync();
            var tipos = todos.Select(n => n.Tipo).Distinct().ToList();
            return Ok(new
            {
                todos = todos.Select(n => new { n.Id, n.Nombre, n.Tipo }),
                tipos,
                busqueda = "panadería y pastelería"
            });
        }

        // GET: api/negocios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NegocioDto>>> GetNegocios()
        {
            var negocios = await _negocioRepository.GetAllAsync();
            return Ok(negocios.Select(MapToDto));
        }

        // GET: api/negocios/cercanos
        [HttpGet("cercanos")]
        public async Task<ActionResult<IEnumerable<NegocioDto>>> GetNegociosCercanos(
            [FromQuery] double latitud,
            [FromQuery] double longitud,
            [FromQuery] int radioMetros = 1000,
            [FromQuery] string? tipo = null)
        {
            var negocios = await _negocioRepository.GetNegociosCercanosAsync(latitud, longitud, radioMetros);

            if (!string.IsNullOrEmpty(tipo) && tipo != "Todos")
            {
                negocios = negocios.Where(n => n.Tipo != null &&
                    n.Tipo.ToLower().Contains(tipo.ToLower())).ToList();
            }

            return Ok(negocios.Select(MapToDto));
        }

        // GET: api/negocios/5
        [HttpGet("{id}")]
        public async Task<ActionResult<NegocioDto>> GetNegocio(int id)
        {
            var negocio = await _negocioRepository.GetByIdWithReseñasAsync(id);
            if (negocio == null)
                return NotFound();

            return Ok(MapToDto(negocio));
        }

        // GET: api/negocios/pendientes (solo admin)
        [HttpGet("pendientes")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<NegocioDto>>> GetNegociosPendientes()
        {
            var negocios = await _negocioRepository.GetAllAsync();
            var pendientes = negocios.Where(n => !n.Aprobado).ToList();
            return Ok(pendientes.Select(MapToDto));
        }

        // PUT: api/negocios/aprobar/{id} (solo admin)
        [HttpPut("aprobar/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AprobarNegocio(int id)
        {
            var negocio = await _negocioRepository.GetByIdAsync(id);
            if (negocio == null)
                return NotFound(new { message = "Negocio no encontrado" });

            negocio.Aprobado = true;
            await _negocioRepository.SaveChangesAsync();

            return Ok(new { message = "Negocio aprobado correctamente" });
        }

        // PUT: api/negocios/rechazar/{id} (solo admin)
        [HttpPut("rechazar/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RechazarNegocio(int id)
        {
            var negocio = await _negocioRepository.GetByIdAsync(id);
            if (negocio == null)
                return NotFound(new { message = "Negocio no encontrado" });

            _negocioRepository.Delete(negocio);
            await _negocioRepository.SaveChangesAsync();

            return Ok(new { message = "Negocio rechazado y eliminado" });
        }

        // POST: api/negocios/subir-imagen (usando FileService)
        [HttpPost("subir-imagen")]
        [Authorize]
        public async Task<IActionResult> SubirImagen([FromForm] int negocioId, IFormFile imagen)
        {
            var negocio = await _negocioRepository.GetByIdAsync(negocioId);
            if (negocio == null)
                return NotFound(new { message = "Negocio no encontrado" });

            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            if (negocio.PropietarioId != usuarioId && !User.IsInRole("Admin"))
                return StatusCode(403, new { message = "No tienes permiso" });

            if (imagen == null || imagen.Length == 0)
                return BadRequest(new { message = "No se ha seleccionado ninguna imagen" });

            try
            {
                if (!string.IsNullOrEmpty(negocio.ImagenUrl))
                {
                    await _fileService.EliminarImagen(negocio.ImagenUrl);
                }

                var nuevaUrl = await _fileService.GuardarImagen(imagen, "negocios");
                negocio.ImagenUrl = nuevaUrl;
                await _negocioRepository.SaveChangesAsync();

                return Ok(new { url = negocio.ImagenUrl, message = "Imagen subida correctamente" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/negocios/{id}/foto - Actualizar foto del negocio usando FileService
        [HttpPut("{id}/foto")]
        [Authorize]
        public async Task<IActionResult> ActualizarFotoNegocio(int id, IFormFile foto)
        {
            var negocio = await _negocioRepository.GetByIdAsync(id);
            if (negocio == null)
                return NotFound(new { message = "Negocio no encontrado" });

            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            if (negocio.PropietarioId != usuarioId && !User.IsInRole("Admin"))
                return StatusCode(403, new { message = "No tienes permiso para modificar este negocio" });

            if (foto == null || foto.Length == 0)
                return BadRequest(new { message = "No se ha seleccionado ninguna foto" });

            try
            {
                if (!string.IsNullOrEmpty(negocio.ImagenUrl))
                {
                    await _fileService.EliminarImagen(negocio.ImagenUrl);
                }

                var nuevaUrl = await _fileService.GuardarImagen(foto, "negocios");
                negocio.ImagenUrl = nuevaUrl;
                await _negocioRepository.SaveChangesAsync();

                return Ok(new
                {
                    imagenUrl = negocio.ImagenUrl,
                    message = "Foto actualizada correctamente"
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/negocios/{id}/foto - Eliminar foto del negocio usando FileService
        [HttpDelete("{id}/foto")]
        [Authorize]
        public async Task<IActionResult> EliminarFotoNegocio(int id)
        {
            var negocio = await _negocioRepository.GetByIdAsync(id);
            if (negocio == null)
                return NotFound(new { message = "Negocio no encontrado" });

            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            if (negocio.PropietarioId != usuarioId && !User.IsInRole("Admin"))
                return StatusCode(403, new { message = "No tienes permiso para modificar este negocio" });

            if (string.IsNullOrEmpty(negocio.ImagenUrl))
                return BadRequest(new { message = "El negocio no tiene una foto para eliminar" });

            await _fileService.EliminarImagen(negocio.ImagenUrl);

            negocio.ImagenUrl = null;
            await _negocioRepository.SaveChangesAsync();

            return Ok(new { message = "Foto eliminada correctamente" });
        }

        // POST: api/negocios
        [HttpPost]
        public async Task<ActionResult<NegocioDto>> CreateNegocio(CreateNegocioDto createDto)
        {
            var propietario = await _userManager.FindByIdAsync(createDto.PropietarioId.ToString());
            if (propietario == null)
                return BadRequest("Propietario no encontrado");

            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            var negocio = new Negocio
            {
                Nombre = createDto.Nombre,
                Descripcion = createDto.Descripcion,
                Tipo = createDto.Tipo,
                Direccion = createDto.Direccion,
                Telefono = createDto.Telefono,
                Horario = createDto.Horario,
                ImagenUrl = createDto.ImagenUrl,
                Activo = true,
                Aprobado = false,
                FechaRegistro = DateTime.UtcNow,
                PropietarioId = createDto.PropietarioId,
                Latitud = createDto.Latitud,
                Longitud = createDto.Longitud
            };

            if (createDto.Latitud.HasValue && createDto.Longitud.HasValue)
            {
                negocio.Ubicacion = geometryFactory.CreatePoint(new Coordinate(createDto.Longitud.Value, createDto.Latitud.Value));
            }

            await _negocioRepository.AddAsync(negocio);
            await _negocioRepository.SaveChangesAsync();

            var dto = MapToDto(negocio);
            dto.NombrePropietario = propietario.Nombre;

            return CreatedAtAction(nameof(GetNegocio), new { id = negocio.Id }, dto);
        }

        // PUT: api/negocios/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNegocio(int id, UpdateNegocioDto updateDto)
        {
            var negocio = await _negocioRepository.GetByIdAsync(id);
            if (negocio == null)
                return NotFound();

            negocio.Nombre = updateDto.Nombre;
            negocio.Descripcion = updateDto.Descripcion;
            negocio.Tipo = updateDto.Tipo;
            negocio.Direccion = updateDto.Direccion;
            negocio.Telefono = updateDto.Telefono;
            negocio.Horario = updateDto.Horario;
            negocio.ImagenUrl = updateDto.ImagenUrl;
            negocio.Activo = updateDto.Activo;
            negocio.PropietarioId = updateDto.PropietarioId;
            negocio.Latitud = updateDto.Latitud;
            negocio.Longitud = updateDto.Longitud;

            _negocioRepository.Update(negocio);
            await _negocioRepository.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/negocios/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNegocio(int id)
        {
            var negocio = await _negocioRepository.GetByIdAsync(id);
            if (negocio == null)
                return NotFound();

            if (!string.IsNullOrEmpty(negocio.ImagenUrl))
            {
                await _fileService.EliminarImagen(negocio.ImagenUrl);
            }

            _negocioRepository.Delete(negocio);
            await _negocioRepository.SaveChangesAsync();

            return NoContent();
        }

        private NegocioDto MapToDto(Negocio n) => new NegocioDto
        {
            Id = n.Id,
            Nombre = n.Nombre,
            Descripcion = n.Descripcion,
            Tipo = n.Tipo,
            Direccion = n.Direccion,
            Telefono = n.Telefono,
            Horario = n.Horario,
            ImagenUrl = n.ImagenUrl,
            Activo = n.Activo,
            Aprobado = n.Aprobado,
            FechaRegistro = n.FechaRegistro,
            PropietarioId = n.PropietarioId,
            Latitud = n.Latitud,
            Longitud = n.Longitud,
            NombrePropietario = n.Propietario?.Nombre,
            CalificacionPromedio = n.Reseñas != null && n.Reseñas.Any()
                ? Math.Round(n.Reseñas.Average(r => r.Calificacion), 1)
                : (double?)null,
            TotalResenas = n.Reseñas?.Count ?? 0
        };
    }
}