using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;
using RestaurantPos.Shared.Abstractions.Tenancy;

namespace RestaurantPos.Shared.Persistence.Interceptors;

public sealed class RlsConnectionInterceptor(ITenantContext tenant) : DbConnectionInterceptor
{
    public const string SettingName = "app.restaurant_id";

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        if (connection is NpgsqlConnection pg)
            Apply(pg).GetAwaiter().GetResult();
    }

    public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken ct = default)
    {
        if (connection is NpgsqlConnection pg)
            await Apply(pg, ct);
    }

    public override void ConnectionClosed(DbConnection connection, ConnectionEndEventData eventData)
    {
        if (connection is NpgsqlConnection pg)
            Clear(pg).GetAwaiter().GetResult();
    }

    private async Task Apply(NpgsqlConnection connection, CancellationToken ct = default)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = $"SELECT set_config('{SettingName}', @value, false)";
        cmd.Parameters.Add(new NpgsqlParameter("value", tenant.RestaurantId?.ToString() ?? string.Empty));
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private async Task Clear(NpgsqlConnection connection, CancellationToken ct = default)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = $"SELECT set_config('{SettingName}', '', false)";
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
