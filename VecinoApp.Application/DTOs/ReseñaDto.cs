namespace VecinoApp.Application.DTOs
{
    public class ReseñaDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int NegocioId { get; set; }
        public int Calificacion { get; set; }
        public string Comentario { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string NombreNegocio { get; set; } = string.Empty;
    }
}