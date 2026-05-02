namespace VecinoApp.Application.DTOs
{
    public class NegocioDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Horario { get; set; }
        public double? CalificacionPromedio { get; set; }
        public int? TotalResenas { get; set; }
        public string? ImagenUrl { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int PropietarioId { get; set; }
        public string? NombrePropietario { get; set; }
        public bool Aprobado { get; set; } = false;
        public double? Latitud { get; set; }
        public double? Longitud { get; set; }
    }
}