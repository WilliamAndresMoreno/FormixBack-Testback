using Formix.Infrastructure.Data.Configurations;
using Formix.Infrastructure.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;

namespace Formix.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TawkToWebhookController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public TawkToWebhookController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> ReceiveWebhook()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();

                if (string.IsNullOrWhiteSpace(body))
                {
                    return BadRequest("Empty body");
                }

                // Opcional: Validar firma HMAC si tienes el Webhook Secret configurado
                var secret = _configuration["TawkTo:WebhookSecret"];
                if (!string.IsNullOrEmpty(secret) && Request.Headers.TryGetValue("X-Tawk-Signature", out var signature))
                {
                    var expectedSignature = CalculateHmac(body, secret);
                    if (signature != expectedSignature)
                    {
                        return Unauthorized("Invalid signature");
                    }
                }

                using var jsonDoc = JsonDocument.Parse(body);
                var root = jsonDoc.RootElement;

                var eventType = root.TryGetProperty("event", out var eventProp) ? eventProp.GetString() : "unknown";

                var log = new TawkWebhookLog
                {
                    Event = eventType ?? "unknown",
                    RawPayload = body,
                    CreatedAt = DateTime.UtcNow
                };

                // Procesar base en el evento
                if (eventType == "chat:end")
                {
                    ExtractVisitorInfo(root, log);
                    
                    if (root.TryGetProperty("chatId", out var chatIdProp))
                        log.TicketId = chatIdProp.GetString();

                    if (root.TryGetProperty("message", out var msgProp) && msgProp.TryGetProperty("text", out var textProp))
                        log.Message = textProp.GetString();
                }
                else if (eventType == "ticket:create")
                {
                    if (root.TryGetProperty("ticket", out var ticketProp))
                    {
                        if (ticketProp.TryGetProperty("id", out var idProp))
                            log.TicketId = idProp.ToString();

                        if (ticketProp.TryGetProperty("message", out var msgProp))
                            log.Message = msgProp.GetString();

                        if (ticketProp.TryGetProperty("requester", out var reqProp))
                        {
                            log.VisitorName = reqProp.TryGetProperty("name", out var nProp) ? nProp.GetString() : null;
                            log.VisitorEmail = reqProp.TryGetProperty("email", out var eProp) ? eProp.GetString() : null;
                        }
                        
                        ExtractCustomAttributesFromTicket(ticketProp, log);
                    }
                }

                _context.TawkWebhookLogs.Add(log);
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        private void ExtractVisitorInfo(JsonElement root, TawkWebhookLog log)
        {
            if (root.TryGetProperty("visitor", out var visitorProp))
            {
                log.VisitorName = visitorProp.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null;
                log.VisitorEmail = visitorProp.TryGetProperty("email", out var emailProp) ? emailProp.GetString() : null;

                if (visitorProp.TryGetProperty("custom", out var customProp) && customProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var attr in customProp.EnumerateArray())
                    {
                        var attrName = attr.TryGetProperty("name", out var nProp) ? nProp.GetString() : null;
                        var attrValue = attr.TryGetProperty("value", out var vProp) ? vProp.GetString() : null;

                        if (attrName == "notaria") log.Notaria = attrValue;
                        if (attrName == "tenantId" && int.TryParse(attrValue, out var tId)) log.TenantId = tId;
                    }
                }
            }
        }

        private void ExtractCustomAttributesFromTicket(JsonElement ticketProp, TawkWebhookLog log)
        {
             // Los atributos personalizados pueden venir en diferentes estructuras en los tickets.
             // Aquí buscamos de forma genérica.
             if (ticketProp.TryGetProperty("custom", out var customProp) && customProp.ValueKind == JsonValueKind.Array)
             {
                 foreach (var attr in customProp.EnumerateArray())
                 {
                     var attrName = attr.TryGetProperty("name", out var nProp) ? nProp.GetString() : null;
                     var attrValue = attr.TryGetProperty("value", out var vProp) ? vProp.GetString() : null;

                     if (attrName == "notaria") log.Notaria = attrValue;
                     if (attrName == "tenantId" && int.TryParse(attrValue, out var tId)) log.TenantId = tId;
                 }
             }
        }

        private string CalculateHmac(string data, string secret)
        {
            var encoding = new UTF8Encoding();
            var keyByte = encoding.GetBytes(secret);
            var messageBytes = encoding.GetBytes(data);
            using (var hmacsha1 = new HMACSHA1(keyByte))
            {
                var hashMessage = hmacsha1.ComputeHash(messageBytes);
                return BitConverter.ToString(hashMessage).Replace("-", "").ToLower();
            }
        }
    }
}
