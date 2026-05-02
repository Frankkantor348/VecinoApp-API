using Microsoft.EntityFrameworkCore;
using VecinoApp.Domain.Entities;
using VecinoApp.Domain.Interfaces;
using VecinoApp.Persistence.Data;

namespace VecinoApp.Persistence.Repositories
{
    public class ProductoRepository : Repository<Producto>, IProductoRepository
    {
        public ProductoRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Producto>> GetByNegocioAsync(int negocioId)
        {
            return await _context.Productos
                .Include(p => p.Negocio)
                .Where(p => p.NegocioId == negocioId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Producto>> GetDestacadosAsync()
        {
            return await _context.Productos
                .Include(p => p.Negocio)
                .Where(p => p.Destacado)
                .ToListAsync();
        }
    }
}