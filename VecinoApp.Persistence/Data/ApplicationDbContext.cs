using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VecinoApp.Domain.Entities;

namespace VecinoApp.Persistence.Data
{
    // El DbContext ahora hereda de IdentityDbContext<Usuario, IdentityRole<int>, int>
    // Esto integra las tablas de Identity (AspNetUsers, AspNetRoles, etc.) con nuestras entidades
    public class ApplicationDbContext : IdentityDbContext<Usuario, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSets de nuestras entidades de negocio
        public DbSet<Negocio> Negocios { get; set; }
        public DbSet<Reseña> Reseñas { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Promocion> Promociones { get; set; }
        public DbSet<Favorito> Favoritos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Llamada base para configurar Identity
            base.OnModelCreating(modelBuilder);

            // Índices para optimizar búsquedas frecuentes
            modelBuilder.Entity<Negocio>()
                .HasIndex(n => n.Tipo)
                .HasDatabaseName("IX_Negocios_Tipo");

            // Evita que un usuario reseñe dos veces el mismo negocio
            modelBuilder.Entity<Reseña>()
                .HasIndex(r => new { r.NegocioId, r.UsuarioId })
                .IsUnique()
                .HasDatabaseName("IX_Reseñas_NegocioUsuario");

            // Email único en Usuarios (Identity ya lo maneja, pero lo dejamos explícito)
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("IX_Usuarios_Email");
            // Índice para optimizae búsquedas por ubicación
            modelBuilder.Entity<Negocio>()
                .HasIndex(n => n.Ubicacion)
                .HasDatabaseName("IX_Negocios_Ubicacion");

            // Relación: Una reseña pertenece a un usuario (con restricción de borrado)
            modelBuilder.Entity<Reseña>()
                .HasOne(r => r.Usuario)
                .WithMany(u => u.Reseñas)
                .HasForeignKey(r => r.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict); // Evita borrar usuario si tiene reseñas

            // Relación: Una reseña pertenece a un negocio (borrado en cascada)
            modelBuilder.Entity<Reseña>()
                .HasOne(r => r.Negocio)
                .WithMany(n => n.Reseñas)
                .HasForeignKey(r => r.NegocioId)
                .OnDelete(DeleteBehavior.Cascade); // Si borras el negocio, se borran sus reseñas

            // Relación: Favorito con Usuario (restrictivo)
            modelBuilder.Entity<Favorito>()
                .HasOne(f => f.Usuario)
                .WithMany(u => u.Favoritos)
                .HasForeignKey(f => f.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict); // No permite borrar usuario con favoritos

            // Relación: Favorito con Negocio (en cascada)
            modelBuilder.Entity<Favorito>()
                .HasOne(f => f.Negocio)
                .WithMany(n => n.Favoritos)
                .HasForeignKey(f => f.NegocioId)
                .OnDelete(DeleteBehavior.Cascade); // Si borras el negocio, se borran sus favoritos
        }
    }
}