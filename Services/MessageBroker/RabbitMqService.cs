using System.Text;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Services.MessageBroker;

public class RabbitMqService : IMessageBrokerService, IAsyncDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    
    private RabbitMqService(IConnection connection, IChannel channel)
    {
        _connection = connection;
        _channel = channel;
    }

    public static async Task<RabbitMqService> CreateAsync(string hostname, string queueName, string exchangeName)
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

            await DeclareTopologyAsync(channel, queueName, exchangeName);

            return new RabbitMqService(connection, channel);
        });
    }
    
    public async Task SendMessageAsync(string queueName, string message)
    {
        var body = Encoding.UTF8.GetBytes(message);
        await _channel.BasicPublishAsync(exchange: string.Empty, routingKey: queueName, body: body);
    }

    public async Task ReceiveMessageAsync(string queueName, Action<string> messageHandler)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            
            messageHandler(message);
            
            await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
        };
        
        await _channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);
    }
    
    private static async Task DeclareTopologyAsync(IChannel channel, string queueName, string exchangeName)
    {
        await channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Direct
        );
        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false
        );
        await channel.QueueBindAsync(
            queue: queueName,
            exchange: exchangeName,
            routingKey: "");
    }

    public async ValueTask DisposeAsync()
    {
        await _channel.CloseAsync();
        _channel.Dispose();
        await _connection.CloseAsync();
        _connection.Dispose();
    }
}