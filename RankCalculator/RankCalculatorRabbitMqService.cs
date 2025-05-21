using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Services.MessageBroker;

namespace RankCalculator;

public class RankCalculatorRabbitMqService : IAsyncDisposable
{
    private readonly string _loggerExchangeName;
    
    private readonly RabbitMqClient _rabbitMqClient;

    public RankCalculatorRabbitMqService(
        RabbitMqClient rabbitMqClient,
        string loggerExchangeName)
    {
        _rabbitMqClient = rabbitMqClient;
        _loggerExchangeName = loggerExchangeName;
    }

    public async Task SendMessageAsync(string exchangeName, string routingKey, string message)
    {
        var body = Encoding.UTF8.GetBytes(message);
        
        await _rabbitMqClient.Channel.BasicPublishAsync(
            exchange: exchangeName,
            routingKey: routingKey,
            body: body);
    }

    public async Task ReceiveMessageAsync(string queueName, Func<string, Task> messageHandler)
    {
        var consumer = new AsyncEventingBasicConsumer(_rabbitMqClient.Channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            
            await messageHandler(message);

            await _rabbitMqClient.Channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
        };
        
        await _rabbitMqClient.Channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);
    }
    
    public async Task DeclareTopologyAsync(string exchangeName, string queueName)
    {
        await DeclareLoggerExchangeAsync();
        
        await _rabbitMqClient.Channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Direct
        );

        await _rabbitMqClient.Channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false
        );
        
        await _rabbitMqClient.Channel.QueueBindAsync(
            queue: queueName,
            exchange: exchangeName,
            routingKey: string.Empty);
    }

    private async Task DeclareLoggerExchangeAsync()
    {
        await _rabbitMqClient.Channel.ExchangeDeclareAsync(
            exchange: _loggerExchangeName,
            type: ExchangeType.Direct
        );
    }

    public async ValueTask DisposeAsync()
    {
        await _rabbitMqClient.DisposeAsync();
    }
}