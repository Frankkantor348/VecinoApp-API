using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VecinoApp.Application.DTOs;
using VecinoApp.Domain.Entities;
using VecinoApp.Persistence.Data;

namespace VecinoApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UsuariosController(
            UserManager<Usuario> userManager,
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: api/usuarios/perfil
        [HttpGet("perfil")]
        public async Task<ActionResult<UsuarioDto>> GetPerfil()
        {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var usuario = await _userManager.FindByIdAsync(usuarioId.ToString());

            if (usuario == null)
                return NotFound();

            var totalResenas = await _context.Reseñas.CountAsync(r => r.UsuarioId == usuarioId);
            var totalFavoritos = await _context.Favoritos.CountAsync(f => f.UsuarioId == usuarioId);

            return Ok(new UsuarioDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email ?? "",
                Telefono = usuario.Telefono,
                FotoPerfilUrl = usuario.FotoPerfilUrl,
                FechaRegistro = usuario.FechaRegistro,
                TotalResenas = totalResenas,
                TotalFavoritos = totalFavoritos
            });
        }

        // PUT: api/usuarios/perfil
        [HttpPut("perfil")]
        public async Task<IActionResult> UpdatePerfil(UpdatePerfilDto updateDto)
        {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var usuario = await _userManager.FindByIdAsync(usuarioId.ToString());

            if (usuario == null)
                return NotFound();

            usuario.Nombre = updateDto.Nombre;
            usuario.Telefono = updateDto.Telefono;

            var result = await _userManager.UpdateAsync(usuario);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "Perfil actualizado correctamente" });
        }

        // PUT: api/usuarios/perfil/foto
        [HttpPut("perfil/foto")]
        public async Task<IActionResult> UpdateFotoPerfil(IFormFile foto)
        {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var usuario = await _userManager.FindByIdAsync(usuarioId.ToString());

            if (usuario == null)
                return NotFound();

            if (foto == null || foto.Length == 0)
                return BadRequest(new { message = "No se ha seleccionado ninguna foto" });

            var extension = Path.GetExtension(foto.FileName).ToLower();
            if (!new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(extension))
                return BadRequest(new { message = "Formato no permitido" });

            // Eliminar foto anterior si existe
            if (!string.IsNullOrEmpty(usuario.FotoPerfilUrl))
            {
                var rutaAnterior = Path.Combine(_webHostEnvironment.WebRootPath, usuario.FotoPerfilUrl.TrimStart('/'));
                if (System.IO.File.Exists(rutaAnterior))
                    System.IO.File.Delete(rutaAnterior);
            }

            // Guardar nueva foto
            var nombreArchivo = $"{Guid.NewGuid()}.jpg";
            var carpeta = Path.Combine("uploads", "perfiles");
            var rutaCarpeta = Path.Combine(_webHostEnvironment.WebRootPath, carpeta);

            if (!Directory.Exists(rutaCarpeta))
                Directory.CreateDirectory(rutaCarpeta);

            var rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivo);

            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await foto.CopyToAsync(stream);
            }

            usuario.FotoPerfilUrl = $"/{carpeta}/{nombreArchivo}";
            await _userManager.UpdateAsync(usuario);

            return Ok(new { fotoPerfilUrl = usuario.FotoPerfilUrl });
        }

        // DELETE: api/usuarios/perfil/foto
        [HttpDelete("perfil/foto")]
        public async Task<IActionResult> DeleteFotoPerfil()
        {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var usuario = await _userManager.FindByIdAsync(usuarioId.ToString());

            if (usuario == null)
                return NotFound();

            if (!string.IsNullOrEmpty(usuario.FotoPerfilUrl))
            {
                var ruta = Path.Combine(_webHostEnvironment.WebRootPath, usuario.FotoPerfilUrl.TrimStart('/'));
                if (System.IO.File.Exists(ruta))
                    System.IO.File.Delete(ruta);

                usuario.FotoPerfilUrl = null;
                await _userManager.UpdateAsync(usuario);
            }

            return Ok(new { message = "Foto de perfil eliminada" });
        }
    }
}