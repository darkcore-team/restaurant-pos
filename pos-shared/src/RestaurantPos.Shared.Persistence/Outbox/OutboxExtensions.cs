using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RestaurantPos.Shared.Abstractions.Events;

namespace RestaurantPos.Shared.Persistence.Outbox;

public static class OutboxExtensions
{
    public static void AddDomainEvent(this DbContext db, DomainEvent @event)
    {
        var envelope = EventEnvelope.From(@event);

        db.Set<OutboxMessage>().Add(new OutboxMessage()
        {
            Id = envelope.EventId,
            EventType = envelope.EventType,
            Payload = JsonSerializer.Serialize(envelope, PosJson.Options),
            OccurredOn = envelope.OccurredOn,
            RestaurantId = envelope.RestaurantId,
            BranchId = envelope.BranchId
        });
    }

    public static void ConfigureOutbox(this ModelBuilder modelBuilder, string tableName)
    {
        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable(tableName);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Payload).HasColumnName("jsonb").IsRequired();
            entity.Property(e => e.LastError).HasMaxLength(2000);
            entity.HasIndex(e => new { e.PublishedOn, e.OccurredOn })
                .HasDatabaseName($"ix_{tableName}_pending");
        });
    }
}