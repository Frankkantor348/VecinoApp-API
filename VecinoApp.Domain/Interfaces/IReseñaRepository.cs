using VecinoApp.Domain.Entities;

namespace VecinoApp.Domain.Interfaces
{
    public interface IReseñaRepository : IRepository<Reseña>
    {
        Task<IEnumerable<Reseña>> GetByNegocioAsync(int negocioId);
        Task<IEnumerable<Reseña>> GetByUsuarioAsync(int usuarioId);
        Task<double> GetPromedioCalificacionAsync(int negocioId);
    }
}