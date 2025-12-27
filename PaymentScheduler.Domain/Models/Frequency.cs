using System.ComponentModel.DataAnnotations;

namespace PaymentScheduler.Domain.Models;

public class Frequency
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; }

    public string Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Payment> Payments { get; set; }
}