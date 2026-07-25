using System;

namespace Formix.Domain.Dtos
{
    public class ProyectoPlantillaDto
    {
        public int PlantillaId { get; set; }
        public int ProyectoId { get; set; }
        public string? Nombre { get; set; }
        public string? Archivo { get; set; }
        public int Estado { get; set; }
    }
}
