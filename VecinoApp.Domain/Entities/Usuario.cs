using Microsoft.AspNetCore.Identity;

namespace VecinoApp.Domain.Entities
{
    public class Usuario : IdentityUser<int>
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? FotoPerfilUrl { get; set; }
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
        public bool Activo { get; set; } = true;

        // Navigation properties
        public ICollection<Reseña> Reseñas { get; set; } = new List<Reseña>();
        public ICollection<Favorito> Favoritos { get; set; } = new List<Favorito>();
        public ICollection<Negocio> Negocios { get; set; } = new List<Negocio>();
    }
}