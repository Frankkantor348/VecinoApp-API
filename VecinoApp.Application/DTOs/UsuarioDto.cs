namespace VecinoApp.Application.DTOs
{
    public class UsuarioDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? FotoPerfilUrl { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int TotalResenas { get; set; }
        public int TotalFavoritos { get; set; }
    }
}