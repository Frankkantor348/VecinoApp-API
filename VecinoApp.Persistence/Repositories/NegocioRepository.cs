using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using VecinoApp.Domain.Entities;
using VecinoApp.Domain.Interfaces;
using VecinoApp.Persistence.Data;

namespace VecinoApp.Persistence.Repositories
{
    public class NegocioRepository : Repository<Negocio>, INegocioRepository
    {
        public NegocioRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Negocio>> GetNegociosCercanosAsync(double latitud, double longitud, int radioMetros)
        {
            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            var puntoUsuario = geometryFactory.CreatePoint(new Coordinate(longitud, latitud));

            return await _context.Negocios
                .Where(n => n.Ubicacion != null && n.Ubicacion.IsWithinDistance(puntoUsuario, radioMetros))
                .Include(n => n.Propietario)
                .Include(n => n.Reseñas)
                .ToListAsync();
        }

        public async Task<IEnumerable<Negocio>> GetByPropietarioAsync(int propietarioId)
        {
            return await _context.Negocios
                .Where(n => n.PropietarioId == propietarioId)
                .Include(n => n.Propietario)
                .ToListAsync();
        }

        public async Task<IEnumerable<Negocio>> GetByTipoAsync(string tipo)
        {
            var tipoNormalizado = tipo.Trim().ToLower();

            return await _context.Negocios
                .Where(n => n.Tipo != null && n.Tipo.Trim().ToLower() == tipoNormalizado)
                .Include(n => n.Propietario)
                .ToListAsync();
        }

        // Método para obtener un negocio por ID incluyendo sus reseñas
        public async Task<Negocio?> GetByIdWithReseñasAsync(int id)
        {
            return await _context.Negocios
                .Include(n => n.Propietario)
                .Include(n => n.Reseñas)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        // Obtener todas las categorías únicas de negocios aprobados
        public async Task<IEnumerable<string>> GetCategoriasAsync()
        {
            return await _context.Negocios
                .Where(n => n.Aprobado)
                .Select(n => n.Tipo)
                .Where(t => t != null && t != "")
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }
    }
}