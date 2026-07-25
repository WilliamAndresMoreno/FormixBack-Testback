using Microsoft.AspNetCore.Http;

namespace Formix.API.Controllers
{
    public class UploadProyectoPlantillaRequest
    {
        public int ProyectoId { get; set; }
        public int? PlantillaId { get; set; }
        public string? Nombre { get; set; }
        public int? Estado { get; set; } = 1;
        public IFormFile Archivo { get; set; } = null!;
    }
}
