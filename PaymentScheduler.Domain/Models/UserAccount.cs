using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentScheduler.Domain.Models;

public class UserAccount
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string UserName { get; set; }

    [Required, MaxLength(100)]
    public string Password { get; set; }

    [Required, MaxLength(200)]
    public string PhoneNumber { get; set; }

    [Required, MaxLength(200)]
    public Guid Sid { get; set; } = Guid.NewGuid();

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Balance { get; set; }

    [Required]
    public string Currency { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}