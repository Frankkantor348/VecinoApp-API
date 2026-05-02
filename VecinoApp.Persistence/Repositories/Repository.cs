using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using VecinoApp.Domain.Entities;
using VecinoApp.Domain.Interfaces;
using VecinoApp.Persistence.Data;

namespace VecinoApp.Persistence.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;
        protected readonly ILogger<Repository<T>> _logger;

        public Repository(ApplicationDbContext context, ILogger<Repository<T>> logger = null)
        {
            _context = context;
            _dbSet = context.Set<T>();
            _logger = logger;
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            try
            {
                return await _dbSet.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error en GetByIdAsync para id {id}");
                throw;
            }
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            try
            {
                return await _dbSet.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error en GetAllAsync");
                throw;
            }
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                return await _dbSet.Where(predicate).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error en FindAsync");
                throw;
            }
        }

        public async Task AddAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            try
            {
                // Validaciones específicas para Reseña
                if (entity is Reseña reseña)
                {
                    if (reseña.Calificacion < 1 || reseña.Calificacion > 5)
                        throw new ArgumentException("La calificación debe estar entre 1 y 5");

                    if (string.IsNullOrWhiteSpace(reseña.Comentario))
                        throw new ArgumentException("El comentario no puede estar vacío");

                    if (reseña.Comentario.Length > 500) // Ejemplo de límite
                        throw new ArgumentException("El comentario no puede exceder 500 caracteres");
                }

                await _dbSet.AddAsync(entity);
                _logger?.LogInformation($"Entidad {typeof(T).Name} agregada correctamente");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error en AddAsync para {typeof(T).Name}");
                throw;
            }
        }

        public void Update(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            try
            {
                _dbSet.Update(entity);
                _logger?.LogInformation($"Entidad {typeof(T).Name} actualizada");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error en Update para {typeof(T).Name}");
                throw;
            }
        }

        public void Delete(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            try
            {
                _dbSet.Remove(entity);
                _logger?.LogInformation($"Entidad {typeof(T).Name} eliminada");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error en Delete para {typeof(T).Name}");
                throw;
            }
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                return await _dbSet.AnyAsync(predicate);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error en ExistsAsync");
                throw;
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            try
            {
                _logger?.LogInformation("Guardando cambios en la base de datos...");
                var result = await _context.SaveChangesAsync();
                _logger?.LogInformation($"Cambios guardados exitosamente. Registros afectados: {result}");
                return result;
            }
            catch (DbUpdateException dbEx)
            {
                _logger?.LogError(dbEx, $"Error de base de datos al guardar: {dbEx.InnerException?.Message ?? dbEx.Message}");

                // Extraer error específico de SQL Server
                if (dbEx.InnerException?.Message.Contains("truncate") == true)
                    throw new Exception("El comentario excede la longitud máxima permitida");

                if (dbEx.InnerException?.Message.Contains("duplicate") == true)
                    throw new Exception("Ya existe un registro con esos datos");

                throw new Exception($"Error al guardar en la base de datos: {dbEx.InnerException?.Message ?? dbEx.Message}");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error inesperado en SaveChangesAsync");
                throw;
            }
        }
    }
}