using System.ComponentModel.DataAnnotations;

namespace VecinoApp.Application.DTOs
{
    public class CreateReseñaDto
    {
        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public int NegocioId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "La calificación debe ser entre 1 y 5 estrellas")]
        public int Calificacion { get; set; }

        [Required]
        [StringLength(500)]
        public string Comentario { get; set; } = string.Empty;
    }
}