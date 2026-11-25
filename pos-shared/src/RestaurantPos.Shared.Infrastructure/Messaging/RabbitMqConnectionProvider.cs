using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace RestaurantPos.Shared.Infrastructure.Messaging;

public sealed class RabbitMqConnectionProvider(
    IOptions<RabbitMqOptions> options,
    ILogger<RabbitMqConnectionProvider> logger) : IAsyncDisposable
{
    private readonly RabbitMqOptions _options = options.Value;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private IConnection? _connection;

    public async Task<IConnection> GetConnectionAsync(CancellationToken ct)
    {
        if (_connection is { IsOpen: true })
        {
            return _connection;
        }

        await _gate.WaitAsync(ct);
        try
        {
            if (_connection is { IsOpen: true })
            {
                return _connection;
            }

            if (_connection is not null)
            {
                await SafeDisposeAsync(_connection);
                _connection = null;
            }

            var factory = BuildFactory();
            Exception? lastError = null;

            for (var attempt = 1; attempt <= _options.StartupRetryCount; attempt++)
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    _connection = await factory.CreateConnectionAsync(cancellationToken: ct);
                    logger.LogInformation("Connected to RabbitMQ at {Host}:{Port}", _options.HostName, _options.Port);
                    return _connection;
                }
                catch (Exception ex)
                {
                    lastError = ex;

                    if (attempt < _options.StartupRetryCount)
                    {
                        logger.LogWarning(
                            "RabbitMQ not ready (attempt {Attempt}/{Max}): {Reason}. Retrying in {Delay}s",
                            attempt, _options.StartupRetryCount, ex.Message, _options.StartupRetryDelaySeconds);
                        
                        await  Task.Delay(TimeSpan.FromSeconds(_options.StartupRetryDelaySeconds), ct);
                    }
                }
            }
            
            throw new InvalidOperationException(
                $"Could not connect to RabbitMQ at {_options.HostName}:{_options.Port} " +
                $"after {_options.StartupRetryCount} attempts.", lastError);
        }
        finally
        {
            _gate.Release();
        }
    }

    private ConnectionFactory BuildFactory()
    {
        var factory = new ConnectionFactory
        {
            // The client rebuilds its own channels and re-declares topology after a drop.
            // Without this, every blip needs manual and reconnect code.
            AutomaticRecoveryEnabled = true,
            TopologyRecoveryEnabled = true,
            RequestedHeartbeat = TimeSpan.FromSeconds(30),
            NetworkRecoveryInterval = TimeSpan.FromSeconds(_options.NetworkRecoveryIntervalSeconds)
        };

        if (!string.IsNullOrWhiteSpace(_options.Uri))
        {
            factory.Uri = new Uri(_options.Uri);
        }
        else
        {
            factory.HostName = _options.HostName;
            factory.Port = _options.Port;
            factory.UserName = _options.Username;
            factory.Password = _options.Password;
            factory.VirtualHost = _options.VirtualHost;
        }

        return factory;
    }

    private async Task SafeDisposeAsync(IConnection connection)
    {
        try
        {
            if (connection.IsOpen)
                await connection.CloseAsync();
        }
        catch
        {
            // Already broken, nothing left to do but drop it
        }
        finally
        {
            await connection.DisposeAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
            await SafeDisposeAsync(_connection);
        _gate.Dispose();
    }
}