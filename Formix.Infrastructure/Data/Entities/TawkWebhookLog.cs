using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Formix.Infrastructure.Data.Entities;

[Table("SysTawkToLogs")]
public class TawkWebhookLog
{
    [Key]
    public int Id { get; set; }

    [MaxLength(100)]
    public string Event { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? VisitorName { get; set; }

    [MaxLength(200)]
    public string? VisitorEmail { get; set; }

    [MaxLength(200)]
    public string? Notaria { get; set; }

    public int? TenantId { get; set; }

    [MaxLength(100)]
    public string? TicketId { get; set; }

    public string? Message { get; set; }

    public string? RawPayload { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
