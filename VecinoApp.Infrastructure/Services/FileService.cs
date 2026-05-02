using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;

namespace VecinoApp.Infrastructure.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FileService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string> GuardarImagen(IFormFile imagen, string carpeta)
        {
            if (imagen == null || imagen.Length == 0)
                throw new ArgumentException("No se ha seleccionado ninguna imagen");

            var extension = Path.GetExtension(imagen.FileName).ToLower();
            if (!new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(extension))
                throw new ArgumentException("Formato no permitido");

            using var image = await Image.LoadAsync(imagen.OpenReadStream());

            if (image.Width > 1024 || image.Height > 1024)
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(1024, 1024),
                    Mode = ResizeMode.Max
                }));
            }

            var nombreArchivo = $"{Guid.NewGuid()}.jpg";
            var carpetaDestino = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", carpeta);
            var rutaCompleta = Path.Combine(carpetaDestino, nombreArchivo);

            if (!Directory.Exists(carpetaDestino))
                Directory.CreateDirectory(carpetaDestino);

            using (var outputStream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await image.SaveAsync(outputStream, new JpegEncoder { Quality = 80 });
            }

            return $"/uploads/{carpeta}/{nombreArchivo}";
        }

        public Task EliminarImagen(string urlImagen)
        {
            if (string.IsNullOrEmpty(urlImagen))
                return Task.CompletedTask;

            var rutaRelativa = urlImagen.TrimStart('/');
            var rutaCompleta = Path.Combine(_webHostEnvironment.WebRootPath, rutaRelativa);

            if (File.Exists(rutaCompleta))
                File.Delete(rutaCompleta);

            return Task.CompletedTask;
        }
    }
}