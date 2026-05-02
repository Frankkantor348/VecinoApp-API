namespace VecinoApp.Application.DTOs
{
    public class PromocionDto
    {
        public int Id { get; set; }
        public int NegocioId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public int Descuento { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public bool Activa { get; set; }
        public string NombreNegocio { get; set; } = string.Empty;
    }
}