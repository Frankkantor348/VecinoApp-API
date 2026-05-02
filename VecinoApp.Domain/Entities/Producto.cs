using System.ComponentModel.DataAnnotations.Schema;
using VecinoApp.Domain.Entities;

namespace VecinoApp.Domain.Entities;
public class Producto
{
    public int Id { get; set; }
    public int NegocioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal? Precio { get; set; }
    public bool Destacado { get; set; }

    public Negocio Negocio { get; set; } = null!;
}