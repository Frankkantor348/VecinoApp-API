using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VecinoApp.Application.DTOs;
using VecinoApp.Domain.Entities;
using VecinoApp.Domain.Interfaces;

namespace VecinoApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReseñasController : ControllerBase
    {
        private readonly IReseñaRepository _reseñaRepository;
        private readonly INegocioRepository _negocioRepository;
        private readonly UserManager<Usuario> _userManager;
        private readonly ILogger<ReseñasController> _logger;

        public ReseñasController(
            IReseñaRepository reseñaRepository,
            INegocioRepository negocioRepository,
            UserManager<Usuario> userManager,
            ILogger<ReseñasController> logger)
        {
            _reseñaRepository = reseñaRepository;
            _negocioRepository = negocioRepository;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: api/reseñas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReseñaDto>>> GetReseñas()
        {
            try
            {
                _logger.LogInformation("Obteniendo todas las reseñas");
                var reseñas = await _reseñaRepository.GetAllAsync();
                _logger.LogInformation($"Se obtuvieron {reseñas.Count()} reseñas");
                return Ok(reseñas.Select(MapToDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las reseñas");
                return StatusCode(500, new { message = "Error interno al obtener las reseñas" });
            }
        }

        // GET: api/reseñas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReseñaDto>> GetReseña(int id)
        {
            try
            {
                _logger.LogInformation($"Obteniendo reseña con ID: {id}");
                var reseña = await _reseñaRepository.GetByIdAsync(id);

                if (reseña == null)
                {
                    _logger.LogWarning($"Reseña con ID {id} no encontrada");
                    return NotFound(new { message = $"Reseña con ID {id} no encontrada" });
                }

                _logger.LogInformation($"Reseña {id} encontrada");
                return Ok(MapToDto(reseña));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener reseña {id}");
                return StatusCode(500, new { message = "Error interno al obtener la reseña" });
            }
        }

        // GET: api/reseñas/negocio/4
        [HttpGet("negocio/{negocioId}")]
        public async Task<ActionResult<IEnumerable<ReseñaDto>>> GetReseñasPorNegocio(int negocioId)
        {
            try
            {
                _logger.LogInformation($"Obteniendo reseñas para negocio ID: {negocioId}");

                var negocio = await _negocioRepository.GetByIdAsync(negocioId);
                if (negocio == null)
                {
                    _logger.LogWarning($"Negocio {negocioId} no encontrado");
                    return BadRequest(new { message = $"El negocio con ID {negocioId} no existe" });
                }

                var reseñas = await _reseñaRepository.GetByNegocioAsync(negocioId);
                _logger.LogInformation($"Se encontraron {reseñas.Count()} reseñas para negocio {negocioId}");
                return Ok(reseñas.Select(MapToDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener reseñas para negocio {negocioId}");
                return StatusCode(500, new { message = "Error interno al obtener las reseñas" });
            }
        }

        // GET: api/reseñas/usuario/1
        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<ReseñaDto>>> GetReseñasPorUsuario(int usuarioId)
        {
            try
            {
                _logger.LogInformation($"Obteniendo reseñas para usuario ID: {usuarioId}");

                var usuario = await _userManager.FindByIdAsync(usuarioId.ToString());
                if (usuario == null)
                {
                    _logger.LogWarning($"Usuario {usuarioId} no encontrado");
                    return BadRequest(new { message = $"El usuario con ID {usuarioId} no existe" });
                }

                var reseñas = await _reseñaRepository.GetByUsuarioAsync(usuarioId);
                _logger.LogInformation($"Se encontraron {reseñas.Count()} reseñas para usuario {usuarioId}");
                return Ok(reseñas.Select(MapToDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener reseñas para usuario {usuarioId}");
                return StatusCode(500, new { message = "Error interno al obtener las reseñas" });
            }
        }

        // POST: api/reseñas
        [HttpPost]
        public async Task<ActionResult<ReseñaDto>> CreateReseña(CreateReseñaDto createDto)
        {
            try
            {
                _logger.LogInformation("=== INICIANDO CREACIÓN DE RESEÑA ===");
                _logger.LogInformation($"Datos recibidos: UsuarioId={createDto.UsuarioId}, NegocioId={createDto.NegocioId}, Calificacion={createDto.Calificacion}");

                // VALIDACIONES
                if (createDto.UsuarioId <= 0)
                {
                    _logger.LogWarning("UsuarioId inválido");
                    return BadRequest(new { message = "El ID de usuario es inválido" });
                }

                if (createDto.NegocioId <= 0)
                {
                    _logger.LogWarning("NegocioId inválido");
                    return BadRequest(new { message = "El ID del negocio es inválido" });
                }

                if (createDto.Calificacion < 1 || createDto.Calificacion > 5)
                {
                    _logger.LogWarning($"Calificación inválida: {createDto.Calificacion}");
                    return BadRequest(new { message = "La calificación debe estar entre 1 y 5 estrellas" });
                }

                if (string.IsNullOrWhiteSpace(createDto.Comentario))
                {
                    _logger.LogWarning("Comentario vacío");
                    return BadRequest(new { message = "El comentario de la reseña es obligatorio" });
                }

                if (createDto.Comentario.Length > 1000)
                {
                    _logger.LogWarning($"Comentario demasiado largo: {createDto.Comentario.Length}");
                    return BadRequest(new { message = "El comentario no puede exceder los 1000 caracteres" });
                }

                // VERIFICAR USUARIO
                var usuario = await _userManager.FindByIdAsync(createDto.UsuarioId.ToString());
                if (usuario == null)
                {
                    _logger.LogWarning($"Usuario {createDto.UsuarioId} no encontrado");
                    return BadRequest(new { message = $"Usuario con ID {createDto.UsuarioId} no encontrado" });
                }

                // VERIFICAR NEGOCIO
                var negocio = await _negocioRepository.GetByIdAsync(createDto.NegocioId);
                if (negocio == null)
                {
                    _logger.LogWarning($"Negocio {createDto.NegocioId} no encontrado");
                    return BadRequest(new { message = $"Negocio con ID {createDto.NegocioId} no encontrado" });
                }

                // VERIFICAR SI YA EXISTE RESEÑA
                var existe = await _reseñaRepository.ExistsAsync(r => r.UsuarioId == createDto.UsuarioId && r.NegocioId == createDto.NegocioId);

                if (existe)
                {
                    _logger.LogWarning($"Ya existe reseña del usuario {createDto.UsuarioId} para negocio {createDto.NegocioId}");
                    // 👈 IMPORTANTE: Usar Conflict (409) en lugar de BadRequest (400)
                    return Conflict(new { message = "Ya has reseñado este negocio anteriormente. Solo puedes reseñar un negocio una vez." });
                }

                // CREAR RESEÑA
                var reseña = new Reseña
                {
                    UsuarioId = createDto.UsuarioId,
                    NegocioId = createDto.NegocioId,
                    Calificacion = createDto.Calificacion,
                    Comentario = createDto.Comentario.Trim(),
                    Fecha = DateTime.UtcNow
                };

                await _reseñaRepository.AddAsync(reseña);
                await _reseñaRepository.SaveChangesAsync();

                _logger.LogInformation($"✅ RESEÑA CREADA EXITOSAMENTE - ID: {reseña.Id}");

                var dto = MapToDto(reseña);
                dto.NombreUsuario = usuario.Nombre;
                dto.NombreNegocio = negocio.Nombre;

                return CreatedAtAction(nameof(GetReseña), new { id = reseña.Id }, dto);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, $"Error de base de datos: {dbEx.InnerException?.Message}");

                if (dbEx.InnerException?.Message.Contains("truncate") == true)
                    return BadRequest(new { message = "El comentario es demasiado largo" });

                if (dbEx.InnerException?.Message.Contains("duplicate") == true)
                    return Conflict(new { message = "Ya existe una reseña con estos datos" });

                return StatusCode(500, new { message = "Error al guardar la reseña" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inesperado: {ex.Message}");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        // PUT: api/reseñas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReseña(int id, UpdateReseñaDto updateDto)
        {
            try
            {
                _logger.LogInformation($"Actualizando reseña ID: {id}");

                if (updateDto.Calificacion < 1 || updateDto.Calificacion > 5)
                {
                    return BadRequest(new { message = "La calificación debe estar entre 1 y 5" });
                }

                if (string.IsNullOrWhiteSpace(updateDto.Comentario))
                {
                    return BadRequest(new { message = "El comentario es obligatorio" });
                }

                var reseña = await _reseñaRepository.GetByIdAsync(id);
                if (reseña == null)
                {
                    return NotFound(new { message = $"Reseña con ID {id} no encontrada" });
                }

                reseña.Calificacion = updateDto.Calificacion;
                reseña.Comentario = updateDto.Comentario.Trim();

                _reseñaRepository.Update(reseña);
                await _reseñaRepository.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar reseña {id}");
                return StatusCode(500, new { message = "Error interno al actualizar la reseña" });
            }
        }

        // DELETE: api/reseñas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReseña(int id)
        {
            try
            {
                _logger.LogInformation($"Eliminando reseña ID: {id}");

                var reseña = await _reseñaRepository.GetByIdAsync(id);
                if (reseña == null)
                {
                    return NotFound(new { message = $"Reseña con ID {id} no encontrada" });
                }

                _reseñaRepository.Delete(reseña);
                await _reseñaRepository.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar reseña {id}");
                return StatusCode(500, new { message = "Error interno al eliminar la reseña" });
            }
        }

        // Método auxiliar para mapear entidad a DTO
        private ReseñaDto MapToDto(Reseña r)
        {
            return new ReseñaDto
            {
                Id = r.Id,
                UsuarioId = r.UsuarioId,
                NegocioId = r.NegocioId,
                Calificacion = r.Calificacion,
                Comentario = r.Comentario,
                Fecha = r.Fecha,
                NombreUsuario = r.Usuario?.Nombre ?? "Usuario",
                NombreNegocio = r.Negocio?.Nombre ?? "Negocio"
            };
        }
    }
}