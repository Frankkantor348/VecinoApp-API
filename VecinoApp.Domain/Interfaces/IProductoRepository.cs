using VecinoApp.Domain.Entities;

namespace VecinoApp.Domain.Interfaces
{
    public interface IProductoRepository : IRepository<Producto>
    {
        Task<IEnumerable<Producto>> GetByNegocioAsync(int negocioId);
        Task<IEnumerable<Producto>> GetDestacadosAsync();
    }
}