using System;

namespace Formix.Domain.Dtos
{
    public partial class InmuebleDto
    {
        public string? CompradorPrincipalNombre { get; set; }
        public string? CompradorPrincipalCedula { get; set; }
        public string? CompradorPrincipalCorreo { get; set; }
        public string? CompradorPrincipalCelular { get; set; }
        public string? Vendedor { get; set; }
        public string? CompradorAlternoNombre { get; set; }
        public string? CompradorAlternoCedula { get; set; }
        public string? CompradorAlternoCorreo { get; set; }
        public string? CompradorAlternoCelular { get; set; }
        public string? TipoInmuebleNombre { get; set; }
        public int? Orden { get; set; }
    }
}
