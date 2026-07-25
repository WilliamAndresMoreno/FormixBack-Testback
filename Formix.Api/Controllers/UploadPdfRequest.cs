using Microsoft.AspNetCore.Http;

namespace FormixBack.Controllers
{
    public class UploadPdfRequest
    {
        public IFormFile? File { get; set; }
        public int IdRadicado { get; set; }
    }
}