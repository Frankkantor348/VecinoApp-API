using VecinoApp.Domain.Entities;

namespace VecinoApp.Domain.Interfaces
{
    public interface IPromocionRepository : IRepository<Promocion>
    {
        Task<IEnumerable<Promocion>> GetByNegocioAsync(int negocioId);
        Task<IEnumerable<Promocion>> GetActivasAsync();
    }
}