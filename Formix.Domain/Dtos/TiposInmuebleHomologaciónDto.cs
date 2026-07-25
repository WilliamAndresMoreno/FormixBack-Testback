using System;

namespace Formix.Domain.Dtos
{
    public class TiposInmuebleHomologaciónDto
    {
        public int IdTipoInmuebleHomologado { get; set; }
        public int IdTercero { get; set; }
        public int TipoInmuebleId { get; set; }
        public string Nombre { get; set; }
    }
}
