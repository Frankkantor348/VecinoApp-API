using VecinoApp.Domain.Entities;

namespace VecinoApp.Domain.Entities;
public class Favorito
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int NegocioId { get; set; }

    public Usuario Usuario { get; set; } = null!;
    public Negocio Negocio { get; set; } = null!;
}