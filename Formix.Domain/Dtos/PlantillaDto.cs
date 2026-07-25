using System;

namespace Formix.Domain.Dtos
{
    public class PlantillaDto
    {
        public int IdPlantilla { get; set; }
        public string Nombre { get; set; }
        public string Archivo { get; set; }
        public bool Estado { get; set; }
    }
}
