using System;

namespace Formix.Domain.Dtos
{
    public class CajasCompensacionDto
    {
        public int IdCajaCompensacion { get; set; }
        public string Nombre { get; set; }
        public string? Codigo { get; set; }
        public int? IdTipoDocumento { get; set; }
        public string? Documento { get; set; }
        public string? Correo { get; set; }
        public string? Celular { get; set; }
        public string? Direccion { get; set; }
    }
}
