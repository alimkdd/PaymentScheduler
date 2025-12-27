using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentScheduler.Domain.Models;

public class PaymentExecution
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int RecurringPaymentId { get; set; }

    [Required]
    public int PaymentId { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    public DateTime ExecutedAt { get; set; }

    [Required]
    public bool Success { get; set; }

    [StringLength(500)]
    public string FailureReason { get; set; }

    public virtual Payment Payment { get; set; }
}