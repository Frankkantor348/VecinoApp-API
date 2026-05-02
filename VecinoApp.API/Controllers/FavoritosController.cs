using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VecinoApp.Application.DTOs;
using VecinoApp.Domain.Entities;
using VecinoApp.Domain.Interfaces;

namespace VecinoApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FavoritosController : ControllerBase
    {
        private readonly IFavoritoRepository _favoritoRepository;
        private readonly INegocioRepository _negocioRepository;
        private readonly UserManager<Usuario> _userManager;

        public FavoritosController(
            IFavoritoRepository favoritoRepository,
            INegocioRepository negocioRepository,
            UserManager<Usuario> userManager)
        {
            _favoritoRepository = favoritoRepository;
            _negocioRepository = negocioRepository;
            _userManager = userManager;
        }

        // GET: api/favoritos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FavoritoDto>>> GetFavoritos()
        {
            var favoritos = await _favoritoRepository.GetAllAsync();
            return Ok(favoritos.Select(MapToDto));
        }

        // GET: api/favoritos/usuario/1
        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<FavoritoDto>>> GetFavoritosPorUsuario(int usuarioId)
        {
            var favoritos = await _favoritoRepository.GetByUsuarioAsync(usuarioId);
            return Ok(favoritos.Select(MapToDto));
        }
        // metodo para obtener los negocios favoritos de un usuario, con la información del negocio y su calificación promedio
        // GET: api/favoritos/negocios/usuario/1
        [HttpGet("negocios/usuario/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<NegocioDto>>> GetNegociosFavoritosPorUsuario(int usuarioId)
        {
            var negocios = await _favoritoRepository.GetNegociosFavoritosByUsuarioAsync(usuarioId);

            var dtos = negocios.Select(n => new NegocioDto
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
                FechaRegistro = n.FechaRegistro,
                PropietarioId = n.PropietarioId,
                Latitud = n.Latitud,
                Longitud = n.Longitud,
                CalificacionPromedio = n.Reseñas.Any() ? Math.Round(n.Reseñas.Average(r => r.Calificacion), 1) : null,
                TotalResenas = n.Reseñas.Count
            }).ToList();

            return Ok(dtos);
        }
        // GET: api/favoritos/negocio/4
        [HttpGet("negocio/{negocioId}")]
        public async Task<ActionResult<IEnumerable<FavoritoDto>>> GetFavoritosPorNegocio(int negocioId)
        {
            var favoritos = await _favoritoRepository.GetByNegocioAsync(negocioId);
            return Ok(favoritos.Select(MapToDto));
        }

        // GET: api/favoritos/verificar?usuarioId=1&negocioId=4
        [HttpGet("verificar")]
        public async Task<ActionResult<bool>> VerificarFavorito([FromQuery] int usuarioId, [FromQuery] int negocioId)
        {
            var existe = await _favoritoRepository.EsFavoritoAsync(usuarioId, negocioId);
            return Ok(existe);
        }

        // POST: api/favoritos
        [HttpPost]
        public async Task<ActionResult<FavoritoDto>> CreateFavorito(CreateFavoritoDto createDto)
        {
            var usuario = await _userManager.FindByIdAsync(createDto.UsuarioId.ToString());
            if (usuario == null)
                return BadRequest("Usuario no encontrado");

            var negocio = await _negocioRepository.GetByIdAsync(createDto.NegocioId);
            if (negocio == null)
                return BadRequest("Negocio no encontrado");

            var existe = await _favoritoRepository.EsFavoritoAsync(createDto.UsuarioId, createDto.NegocioId);
            if (existe)
                return BadRequest("Este negocio ya está en favoritos");

            var favorito = new Favorito
            {
                UsuarioId = createDto.UsuarioId,
                NegocioId = createDto.NegocioId
            };

            await _favoritoRepository.AddAsync(favorito);
            await _favoritoRepository.SaveChangesAsync();

            var dto = MapToDto(favorito);
            dto.NombreUsuario = usuario.Nombre;
            dto.NombreNegocio = negocio.Nombre;
            dto.DireccionNegocio = negocio.Direccion;

            return CreatedAtAction(nameof(GetFavoritosPorUsuario), new { usuarioId = favorito.UsuarioId }, dto);
        }

        // DELETE: api/favoritos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFavorito(int id)
        {
            var favorito = await _favoritoRepository.GetByIdAsync(id);
            if (favorito == null)
                return NotFound();

            _favoritoRepository.Delete(favorito);
            await _favoritoRepository.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/favoritos?usuarioId=1&negocioId=4
        [HttpDelete]
        public async Task<IActionResult> DeleteFavoritoPorUsuarioYNegocio([FromQuery] int usuarioId, [FromQuery] int negocioId)
        {
            var existe = await _favoritoRepository.EsFavoritoAsync(usuarioId, negocioId);
            if (!existe)
                return NotFound();

            await _favoritoRepository.RemoveAsync(usuarioId, negocioId);
            await _favoritoRepository.SaveChangesAsync();

            return NoContent();
        }

        // Método auxiliar para mapear entidad a DTO
        private FavoritoDto MapToDto(Favorito f) => new FavoritoDto
        {
            Id = f.Id,
            UsuarioId = f.UsuarioId,
            NegocioId = f.NegocioId,
            NombreUsuario = f.Usuario?.Nombre ?? "Usuario",
            NombreNegocio = f.Negocio?.Nombre ?? "Negocio",
            DireccionNegocio = f.Negocio?.Direccion ?? "Dirección no disponible"
        };
    }
}