using MassTransit;

namespace PaymentService.Infrastructure.Data;
public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options)
    {
    }

    public DbSet<Payment> Payments { get; set; }
    public DbSet<PaymentState> PaymentStates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.Entity<PaymentState>(entity =>
        {
            entity.HasKey(e => e.CorrelationId);
            entity.Property(e => e.OrderId).HasMaxLength(64);
            entity.Property(e => e.CurrentState).HasMaxLength(64);
            entity.Property(e => e.CreatedAt).IsRequired();
        });

    }
}

