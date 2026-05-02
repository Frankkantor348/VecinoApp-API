using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VecinoApp.Domain.Entities;
using VecinoApp.Domain.Interfaces;
using VecinoApp.Persistence.Data;

namespace VecinoApp.Persistence.Repositories
{
    public class ReseñaRepository : Repository<Reseña>, IReseñaRepository
    {
        private readonly ILogger<ReseñaRepository> _logger;

        public ReseñaRepository(ApplicationDbContext context, ILogger<ReseñaRepository> logger) : base(context)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<Reseña>> GetByNegocioAsync(int negocioId)
        {
            try
            {
                _logger.LogInformation($"Obteniendo reseñas para negocio {negocioId}");
                return await _context.Reseñas
                    .Include(r => r.Usuario)
                    .Include(r => r.Negocio)
                    .Where(r => r.NegocioId == negocioId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener reseñas para negocio {negocioId}");
                throw;
            }
        }

        public async Task<IEnumerable<Reseña>> GetByUsuarioAsync(int usuarioId)
        {
            try
            {
                _logger.LogInformation($"Obteniendo reseñas para usuario {usuarioId}");
                return await _context.Reseñas
                    .Include(r => r.Negocio)
                    .Where(r => r.UsuarioId == usuarioId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener reseñas para usuario {usuarioId}");
                throw;
            }
        }

        public async Task<double> GetPromedioCalificacionAsync(int negocioId)
        {
            try
            {
                _logger.LogInformation($"Calculando promedio de calificaciones para negocio {negocioId}");

                var calificaciones = await _context.Reseñas
                    .Where(r => r.NegocioId == negocioId)
                    .Select(r => r.Calificacion)
                    .ToListAsync();

                if (!calificaciones.Any())
                    return 0;

                var promedio = calificaciones.Average();
                _logger.LogInformation($"Promedio calculado: {promedio}");
                return promedio;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al calcular promedio para negocio {negocioId}");
                throw;
            }
        }
    }
}