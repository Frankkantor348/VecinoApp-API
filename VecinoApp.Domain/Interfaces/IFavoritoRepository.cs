using VecinoApp.Domain.Entities;

namespace VecinoApp.Domain.Interfaces
{
    public interface IFavoritoRepository : IRepository<Favorito>
    {
        Task<IEnumerable<Favorito>> GetByUsuarioAsync(int usuarioId);
        Task<IEnumerable<Favorito>> GetByNegocioAsync(int negocioId);
        Task<bool> EsFavoritoAsync(int usuarioId, int negocioId);
        Task RemoveAsync(int usuarioId, int negocioId);
        Task<IEnumerable<Negocio>> GetNegociosFavoritosByUsuarioAsync(int usuarioId);
    }
}