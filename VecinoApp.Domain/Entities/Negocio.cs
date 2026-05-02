using NetTopologySuite.Geometries;

namespace VecinoApp.Domain.Entities
{
    public class Negocio
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty; // "panadería", "frutería", "tienda", "papelería"
        public string Direccion { get; set; } = string.Empty;

        // Coordenadas geográficas (simples, sin NetTopologySuite)
        public double? Latitud { get; set; }
        public double? Longitud { get; set; }
        public Point? Ubicacion { get; set; }

        // Ubicación como texto (opcional, para direcciones completas)
        public string? UbicacionTexto { get; set; }


        public string? Telefono { get; set; }
        public string? Horario { get; set; }
        public string? ImagenUrl { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
        public int PropietarioId { get; set; } // Dueño del negocio
        public bool Aprobado { get; set; } = false;  // campo para aprobar el negocio por un administrador

        // Navigation properties
        public Usuario Propietario { get; set; } = null!;
        public ICollection<Reseña> Reseñas { get; set; } = new List<Reseña>();
        public ICollection<Favorito> Favoritos { get; set; } = new List<Favorito>();
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
        public ICollection<Promocion> Promociones { get; set; } = new List<Promocion>();
    }
}