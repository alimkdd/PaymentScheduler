namespace PaymentScheduler.Domain.Enums;

public enum PaymentStatus
{
    Active = 1,
    Paused = 2,
    Cancelled = 3,
    Failed = 4,
    Completed = 5
}