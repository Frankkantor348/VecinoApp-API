using System.ComponentModel.DataAnnotations;

namespace VecinoApp.Application.DTOs
{
    public class CreatePromocionDto
    {
        [Required]
        public int NegocioId { get; set; }

        [Required(ErrorMessage = "El título es obligatorio")]
        [StringLength(100)]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El descuento es obligatorio")]
        public int Descuento { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [StringLength(500)]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }

        public bool Activa { get; set; } = true;
    }
}