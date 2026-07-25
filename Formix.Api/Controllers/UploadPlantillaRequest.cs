using Microsoft.AspNetCore.Http;

namespace Formix.API.Controllers
{
    public class UploadPlantillaRequest
    {
        public string? Nombre { get; set; }
        public bool Estado { get; set; } = true;
        public IFormFile Archivo { get; set; } = null!;
    }
}
