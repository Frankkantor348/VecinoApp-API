using System.ComponentModel.DataAnnotations;

namespace VecinoApp.Application.DTOs
{
    public class CoordenadasDto
    {
        [Required]
        public double Latitud { get; set; }

        [Required]
        public double Longitud { get; set; }

        [Range(100, 50000)]
        public int RadioMetros { get; set; } = 1000;
    }
}