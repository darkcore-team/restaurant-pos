using Microsoft.EntityFrameworkCore;
using PaymentsService.Domain.Constants;
using PaymentsService.Domain.Entities;
using RestaurantPos.Shared.Persistence;

namespace PaymentsService.Infrastructure.Data;

public sealed class PaymentsServiceDbContext(DbContextOptions options) : TenantDbContext(options)
{
    protected override string Schema => "payments";

    public DbSet<CashRegister> CashRegisters => Set<CashRegister>();
    public DbSet<CashSession> CashSessions => Set<CashSession>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketSequence> TicketSequences => Set<TicketSequence>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentsServiceDbContext).Assembly);
    }
}