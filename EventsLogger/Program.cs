using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Services.Common;
using Services.MessageBroker;

namespace EventsLogger;

class Program
{
    static async Task Main()
    {
        var config = new EnvironmentConfiguration();

        var rabbitMqClient = await RabbitMqClient.CreateAsync(config.RabbitMqHostname);
        
        await DeclareTopologyAsync(rabbitMqClient,
            config.LoggerRabbitMqExchangeName,
            config.LoggerRabbitMqQueueName, 
            [config.RankCalculatedRoutingKey, config.SimilarityCalculatedRoutingKey]);

        await ReceiveMessageAsync(rabbitMqClient, config.LoggerRabbitMqQueueName);
        
        await WaitForShutdownSignalAsync();
        Console.WriteLine("Events logger service stopped");
    }

    private static async Task DeclareTopologyAsync(
        RabbitMqClient rabbitMqClient,
        string exchangeName,
        string queueName,
        string[] routingKeys)
    {
        await rabbitMqClient.Channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Direct);
        
        await rabbitMqClient.Channel.QueueDeclareAsync(queueName, exclusive: false);

        foreach (var routingKey in routingKeys)
        {
            await rabbitMqClient.Channel.QueueBindAsync(queueName, exchangeName, routingKey);
        }
    }

    private static async Task ReceiveMessageAsync(
        RabbitMqClient rabbitMqClient,
        string queueName)
    {
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