using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentsService.Domain.Entities;

namespace PaymentsService.Infrastructure.Configuration;

public class TicketSequenceConfiguration : IEntityTypeConfiguration<TicketSequence>
{
    public void Configure(EntityTypeBuilder<TicketSequence> builder)
    {
        builder.ToTable("ticket_sequences");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).UseIdentityByDefaultColumn();
        builder.Property(s => s.LastNumber).HasDefaultValue(0L);
    }
}