using System.ComponentModel.DataAnnotations;

namespace VecinoApp.Application.DTOs
{
    public class CreateFavoritoDto
    {
        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public int NegocioId { get; set; }
    }
}