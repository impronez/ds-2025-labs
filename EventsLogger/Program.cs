using System.Text;
using Common.MessageBroker;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EventsLogger;

class Program
{
    static async Task Main()
    {
        var rabbitMqHostname = Environment.GetEnvironmentVariable("RABBITMQ_HOSTNAME");
        var loggerRabbitMqExchangeName = Environment.GetEnvironmentVariable("LOGGER_RABBIT_MQ_EXCHANGE_NAME");
        
        var rankCalculatedRoutingKey = Environment.GetEnvironmentVariable("RANK_CALCULATED_ROUTING_KEY");
        var similarityCalculatedRoutingKey = Environment.GetEnvironmentVariable("SIMILARITY_CALCULATED_ROUTING_KEY");

        // TODO: проверки
        
        var rabbitMqClient = await RabbitMqClient.CreateAsync(rabbitMqHostname!);
        
        await InitializeRabbitMqChannel(rabbitMqClient, loggerRabbitMqExchangeName!, [rankCalculatedRoutingKey!, similarityCalculatedRoutingKey!]);
        
        await WaitForShutdownSignalAsync();
        Console.WriteLine("Events logger service stopped");
    }

    private static async Task InitializeRabbitMqChannel(
        RabbitMqClient rabbitMqClient,
        string exchangeName,
        string[] routingKeys)
    {
        await rabbitMqClient.Channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Direct);
        
        var queueDeclareResult = await rabbitMqClient.Channel.QueueDeclareAsync();
        var queueName = queueDeclareResult.QueueName;

        foreach (var routingKey in routingKeys)
        {
            await rabbitMqClient.Channel.QueueBindAsync(queueName, exchangeName, routingKey);
        }
        
        var consumer = new AsyncEventingBasicConsumer(rabbitMqClient.Channel);
        consumer.ReceivedAsync += (_, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var routingKey = ea.RoutingKey;
            
            Console.WriteLine($"{DateTime.Now} {routingKey}: {message}");
            
            return Task.CompletedTask;
        };
        
        await rabbitMqClient.Channel.BasicConsumeAsync(queueName, autoAck: true, consumer: consumer);
    }

    private static async Task WaitForShutdownSignalAsync()
    {
        var exitEvent = new TaskCompletionSource<bool>();
        
        Console.CancelKeyPress += (_, e) =>
        {
            Console.WriteLine("Stopping events logger service...");
            e.Cancel = true;
            exitEvent.SetResult(true);
        };

        await exitEvent.Task;
    }
}