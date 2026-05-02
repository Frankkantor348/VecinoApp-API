using System.ComponentModel.DataAnnotations;

namespace VecinoApp.Application.DTOs
{
    public class CreateProductoDto
    {
        [Required]
        public int NegocioId { get; set; }

        [Required(ErrorMessage = "El nombre del producto es obligatorio")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descripcion { get; set; }

        [Range(0, 999999.99)]
        public decimal? Precio { get; set; }

        public bool Destacado { get; set; }
    }
}