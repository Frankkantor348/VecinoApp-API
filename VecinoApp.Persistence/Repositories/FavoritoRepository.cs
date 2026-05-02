using Microsoft.EntityFrameworkCore;
using VecinoApp.Domain.Entities;
using VecinoApp.Domain.Interfaces;
using VecinoApp.Persistence.Data;

namespace VecinoApp.Persistence.Repositories
{
    public class FavoritoRepository : Repository<Favorito>, IFavoritoRepository
    {
        public FavoritoRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Favorito>> GetByUsuarioAsync(int usuarioId)
        {
            return await _context.Favoritos
                .Include(f => f.Usuario)
                .Include(f => f.Negocio)
                .Where(f => f.UsuarioId == usuarioId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Favorito>> GetByNegocioAsync(int negocioId)
        {
            return await _context.Favoritos
                .Include(f => f.Usuario)
                .Include(f => f.Negocio)
                .Where(f => f.NegocioId == negocioId)
                .ToListAsync();
        }

        public async Task<bool> EsFavoritoAsync(int usuarioId, int negocioId)
        {
            return await _context.Favoritos
                .AnyAsync(f => f.UsuarioId == usuarioId && f.NegocioId == negocioId);
        }

        public async Task RemoveAsync(int usuarioId, int negocioId)
        {
            var favorito = await _context.Favoritos
                .FirstOrDefaultAsync(f => f.UsuarioId == usuarioId && f.NegocioId == negocioId);

            if (favorito != null)
            {
                _context.Favoritos.Remove(favorito);
            }
        }
        public async Task<IEnumerable<Negocio>> GetNegociosFavoritosByUsuarioAsync(int usuarioId)
        {
            return await _context.Favoritos
                .Include(f => f.Negocio)
                    .ThenInclude(n => n.Propietario)
                .Include(f => f.Negocio)
                    .ThenInclude(n => n.Reseñas)
                .Where(f => f.UsuarioId == usuarioId)
                .Select(f => f.Negocio)
                .ToListAsync();
        }
    }
}