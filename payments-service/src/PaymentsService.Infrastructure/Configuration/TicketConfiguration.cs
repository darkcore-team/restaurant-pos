using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentsService.Domain.Constants;
using PaymentsService.Domain.Entities;

namespace PaymentsService.Infrastructure.Configuration;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("tickets");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).UseIdentityByDefaultColumn();
        builder.Property(t => t.PublicId).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(t => t.TicketNumber).HasMaxLength(30).IsRequired();
        builder.Property(t => t.IssuedAt).HasDefaultValueSql("now()");
        builder.Property(t => t.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(t => t.UpdatedAt).HasDefaultValueSql("now()");

        builder.HasIndex(t => new { t.BranchId, t.TicketNumber }).IsUnique();
    }
}