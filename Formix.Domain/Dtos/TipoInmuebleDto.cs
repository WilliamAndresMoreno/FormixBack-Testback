using System;

namespace Formix.Domain.Dtos
{
    public class TipoInmuebleDto
    {
        public int TipoInmuebleId { get; set; }
        public string Nombre { get; set; }
        public bool Activo { get; set; }
    }
}
