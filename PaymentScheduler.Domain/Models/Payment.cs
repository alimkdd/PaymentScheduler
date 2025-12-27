using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentScheduler.Domain.Models;

public class Payment
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid UserSid { get; set; }

    [Required]
    [StringLength(200)]
    public string PayeeName { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(100)]
    public string Title { get; set; }

    [Required]
    [MaxLength(100)]
    public string Description { get; set; }

    [Required]
    [MaxLength(255)]
    public string DestinationAccount { get; set; }

    [Required]
    public int FrequencyId { get; set; }

    [Required]
    public int StatusId { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime NextExecutionDate { get; set; }

    public DateTime? LastExecutionDate { get; set; }

    public int ConsecutiveFailures { get; set; } = 0;

    [MaxLength(500)]
    public string? LastFailureReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public Frequency Frequency { get; set; }

    public Status Status { get; set; }
}