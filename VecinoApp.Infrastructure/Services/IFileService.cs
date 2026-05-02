using Microsoft.AspNetCore.Http;

namespace VecinoApp.Infrastructure.Services
{
    public interface IFileService
    {
        Task<string> GuardarImagen(IFormFile imagen, string carpeta);
        Task EliminarImagen(string urlImagen);
    }
}