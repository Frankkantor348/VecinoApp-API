using System.ComponentModel.DataAnnotations;

namespace VecinoApp.Application.DTOs
{
    public class CreateNegocioDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Tipo { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Direccion { get; set; } = string.Empty;

        [Phone]
        public string? Telefono { get; set; }

        public string? Horario { get; set; }

        public string? ImagenUrl { get; set; }

        [Required]
        public int PropietarioId { get; set; }

        public double? Latitud { get; set; }
        public double? Longitud { get; set; }
    }
}