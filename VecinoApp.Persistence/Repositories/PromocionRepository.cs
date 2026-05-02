using Microsoft.EntityFrameworkCore;
using VecinoApp.Domain.Entities;
using VecinoApp.Domain.Interfaces;
using VecinoApp.Persistence.Data;

namespace VecinoApp.Persistence.Repositories
{
    public class PromocionRepository : Repository<Promocion>, IPromocionRepository
    {
        public PromocionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Promocion>> GetByNegocioAsync(int negocioId)
        {
            return await _context.Promociones
                .Include(p => p.Negocio)
                .Where(p => p.NegocioId == negocioId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Promocion>> GetActivasAsync()
        {
            // CAMBIAR DE UTC A LOCAL
            var hoy = DateTime.Now.Date;  // En lugar de DateTime.UtcNow.Date

            Console.WriteLine($"📅 Fecha actual (Local): {hoy}");

            return await _context.Promociones
                .Include(p => p.Negocio)
                .Where(p => p.Activa && p.FechaInicio.Date <= hoy && p.FechaFin.Date >= hoy)
                .ToListAsync();
        }
    }
}