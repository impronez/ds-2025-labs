using Common.MessageBroker;
using Services;
using StackExchange.Redis;

namespace RankCalculator;

public class Program
{
    public static async Task Main(string[] args)
    {
        var rankCalculatorRabbitMqQueueName = Environment.GetEnvironmentVariable("RANK_CALCULATOR_RABBIT_MQ_QUEUE_NAME");
        var rankCalculatorRabbitMqExchangeName = Environment.GetEnvironmentVariable("RANK_CALCULATOR_RABBIT_MQ_EXCHANGE_NAME");
        var rankCalculatorRoutingKey = Environment.GetEnvironmentVariable("RANK_CALCULATED_ROUTING_KEY");
        
        var loggerRabbitMqExchangeName = Environment.GetEnvironmentVariable("LOGGER_RABBIT_MQ_EXCHANGE_NAME");
        
        var redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");
        var rabbitMqHostname = Environment.GetEnvironmentVariable("RABBITMQ_HOSTNAME");
        
        // TODO: проверки
        
        var storageService = GetStorageService(redisConnectionString!);

        var rabbitMqClient = await RabbitMqClient.CreateAsync(rabbitMqHostname!);
        var rabbitMqService = new RankCalculatorRabbitMqService(rabbitMqClient, loggerRabbitMqExchangeName!, rankCalculatorRoutingKey!);
        await rabbitMqService.DeclareTopologyAsync(rankCalculatorRabbitMqExchangeName!, rankCalculatorRabbitMqQueueName!);

        var rankCalculatorService = new RankCalculatorService(storageService, rabbitMqService);

        await rabbitMqService.ReceiveMessageAsync(rankCalculatorRabbitMqQueueName!, rankCalculatorService.Process);

        await WaitForShutdownSignalAsync();

        Console.WriteLine("Rank calculator service stopped");
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

    private static RedisStorageService GetStorageService(string connectionString)
    {
        var multiplexer = ConnectionMultiplexer.Connect(connectionString);

        return new RedisStorageService(multiplexer);
    }
}