using VecinoApp.Domain.Entities;

namespace VecinoApp.Domain.Entities;
public class Reseña
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int NegocioId { get; set; }
    public int Calificacion { get; set; }
    public string Comentario { get; set; } = string.Empty;
    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    public Usuario Usuario { get; set; } = null!;
    public Negocio Negocio { get; set; } = null!;
}