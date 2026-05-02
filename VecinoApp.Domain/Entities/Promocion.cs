using VecinoApp.Domain.Entities;

namespace VecinoApp.Domain.Entities;

public class Promocion
{
public int Id { get; set; }
public int NegocioId { get; set; }
public string Titulo { get; set; } = string.Empty;
public int Descuento { get; set; }
public string Descripcion { get; set; } = string.Empty;
public DateTime FechaInicio { get; set; }
public DateTime FechaFin { get; set; }
public bool Activa { get; set; } = true;

public Negocio Negocio { get; set; } = null!;
}