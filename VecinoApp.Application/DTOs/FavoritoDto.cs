namespace VecinoApp.Application.DTOs
{
    public class FavoritoDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int NegocioId { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string NombreNegocio { get; set; } = string.Empty;
        public string DireccionNegocio { get; set; } = string.Empty;
    }
}