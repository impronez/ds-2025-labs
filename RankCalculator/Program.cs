using Microsoft.Extensions.Configuration;
using Services;
using Services.MessageBroker;
using StackExchange.Redis;

namespace RankCalculator;

public class Program
{
    public static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();
        
        string queueName = config.GetSection("RankCalculatorRabbitMq")["QueueName"];
        string exchangeName = config.GetSection("RankCalculatorRabbitMq")["ExchangeName"];
        
        var redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");
        var messageBrokerServiceConnectionString = Environment.GetEnvironmentVariable("RABBITMQ_HOSTNAME");

        var storageService = GetStorageService(redisConnectionString);

        var rankCalculatorService = new RankCalculatorService(storageService);

        var messageBroker =  await RabbitMqService.CreateAsync(messageBrokerServiceConnectionString, queueName, exchangeName);
        
        await messageBroker.ReceiveMessageAsync(queueName, rankCalculatorService.Process);

        var exitEvent = new TaskCompletionSource<bool>();
        
        Console.CancelKeyPress += (sender, e) =>
        {
            Console.WriteLine("Stopping rank calculator service...");
            e.Cancel = true;
            exitEvent.SetResult(true);
        };

        await exitEvent.Task;

        Console.WriteLine("Rank calculator service stopped");
    }

    private static RedisStorageService GetStorageService(string connectionString)
    {
        var multiplexer = ConnectionMultiplexer.Connect(connectionString);

        return new RedisStorageService(multiplexer);
    }
}