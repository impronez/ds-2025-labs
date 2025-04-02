using Polly;
using RabbitMQ.Client;

namespace Common.MessageBroker;

public class RabbitMqClient : IAsyncDisposable
{
    public readonly IConnection Connection;
    public readonly IChannel Channel;

    public RabbitMqClient(IConnection connection, IChannel channel)
    {
        Connection = connection;
        Channel = channel;
    }

    public static async Task<RabbitMqClient> CreateAsync(string hostname)
    {
        var retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 5,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        
        return await retryPolicy.ExecuteAsync(async () =>
        {
            var factory = new ConnectionFactory
            {
                HostName = hostname
            };

            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();

            return new RabbitMqClient(connection, channel);
        });
    }

    public async ValueTask DisposeAsync()
    {
        await Channel.CloseAsync();
        Channel.Dispose();
        await Connection.CloseAsync();
        Connection.Dispose();
    }
}