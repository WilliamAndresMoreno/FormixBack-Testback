using System;

namespace Formix.Domain.Dtos
{
    public class RadicadosInmuebleDto
    {
        public int IdRadicado { get; set; }
        public int IdInmueble { get; set; }
        public int? Orden { get; set; }
    }
}
