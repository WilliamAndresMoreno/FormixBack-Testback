using System;

namespace Formix.Domain.Dtos
{
    public class TiposDocumentoDto
    {
        public int IdTipoDocumento { get; set; }
        public string Nombre { get; set; }
        public string? Sigla { get; set; }
        public string? Codigo { get; set; }
    }
}
