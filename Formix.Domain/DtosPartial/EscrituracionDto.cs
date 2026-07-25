using System;

namespace Formix.Domain.Dtos
{
    public partial class EscrituracionDto
    {
        public int Id { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public DateTime PlazoElaboracion { get; set; }
        public string ModeloEscritura { get; set; }
        public string Revisor { get; set; }
        public string Digitador { get; set; }
        public string FlujoTrabajo { get; set; }
        public string IndiceRadicado { get; set; }
        public DateTime FechaFirma { get; set; }
        public string Observaciones { get; set; }

        //public ICollection<Otorgante> Otorgantes { get; set; }
        //public ICollection<BienInmueble> Bienes { get; set; }
        //public ICollection<DocumentoAdjunto> Documentos { get; set; }
    }

}
