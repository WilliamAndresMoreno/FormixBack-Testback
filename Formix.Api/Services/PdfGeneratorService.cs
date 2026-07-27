using PuppeteerSharp;
using PuppeteerSharp.Media;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Formix.Api.Services
{
    public class PdfGeneratorService
    {
        private readonly IConfiguration _configuration;

        public PdfGeneratorService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<byte[]> GenerarPdfDesdeHtmlAsync(string htmlContent)
        {
            string executablePath = _configuration["PdfSettings:ExecutablePath"];
            LaunchOptions launchOptions;

            if (!string.IsNullOrEmpty(executablePath))
            {
                launchOptions = new LaunchOptions
                {
                    Headless = true,
                    ExecutablePath = executablePath,
                    Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
                };
            }
            else
            {
                // Desarrollo local: descarga automática
                var browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync();
                
                launchOptions = new LaunchOptions
                {
                    Headless = true,
                    Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
                };
            }

            using var browser = await Puppeteer.LaunchAsync(launchOptions);
            
            using var page = await browser.NewPageAsync();
            
            await page.SetContentAsync(htmlContent, new SetContentOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle0 } });

            var pdfOptions = new PdfOptions
            {
                Format = PaperFormat.Letter,
                PrintBackground = true,
                MarginOptions = new MarginOptions
                {
                    Top = "40px",
                    Bottom = "40px",
                    Left = "40px",
                    Right = "40px"
                }
            };

            var pdfStream = await page.PdfStreamAsync(pdfOptions);
            
            using var memoryStream = new MemoryStream();
            await pdfStream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
