using Microsoft.EntityFrameworkCore;
using PaymentScheduler.Domain.Models;

namespace PaymentScheduler.Infrastructure.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<UserAccount> UserAccounts { get; set; }

    public DbSet<Payment> Payments { get; set; }

    public DbSet<PaymentExecution> PaymentExecutions { get; set; }
}