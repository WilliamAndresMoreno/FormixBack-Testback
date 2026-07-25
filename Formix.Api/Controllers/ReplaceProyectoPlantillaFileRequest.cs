using Microsoft.AspNetCore.Http;

namespace Formix.API.Controllers
{
    public class ReplaceProyectoPlantillaFileRequest
    {
        public IFormFile Archivo { get; set; } = null!;
    }
}
