namespace VecinoApp.Application.DTOs
{
    public class ProductoDto
    {
        public int Id { get; set; }
        public int NegocioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal? Precio { get; set; }
        public bool Destacado { get; set; }
        public string NombreNegocio { get; set; } = string.Empty;
    }
}