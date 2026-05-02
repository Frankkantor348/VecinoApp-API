using VecinoApp.Domain.Entities;

namespace VecinoApp.Domain.Interfaces
{
    public interface INegocioRepository : IRepository<Negocio>
    {
        Task<IEnumerable<Negocio>> GetNegociosCercanosAsync(double latitud, double longitud, int radioMetros);
        Task<IEnumerable<Negocio>> GetByPropietarioAsync(int propietarioId);
        Task<IEnumerable<Negocio>> GetByTipoAsync(string tipo);
        Task<Negocio?> GetByIdWithReseñasAsync(int id);
        Task<IEnumerable<string>> GetCategoriasAsync();

    }
}