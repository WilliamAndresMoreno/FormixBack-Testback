using Microsoft.AspNetCore.Http;

namespace Formix.API.Controllers
{
    public class ReplacePlantillaFileRequest
    {
        public IFormFile Archivo { get; set; } = null!;
    }
}
