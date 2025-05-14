using Services.Common;
using Services.MessageBroker;
using Services.Storage;
using StackExchange.Redis;

namespace RankCalculator;

public class Program
{
    public static async Task Main()
    {
        var config = new EnvironmentConfiguration();
        
        var storageService = new RedisStorageService(config.RedisPassword);

        var rabbitMqClient = await RabbitMqClient.CreateAsync(config.RabbitMqHostname, config.RabbitMqUsername, config.RabbitMqPassword);
        var rabbitMqService = new RankCalculatorRabbitMqService(rabbitMqClient, config.LoggerRabbitMqExchangeName);
        await rabbitMqService.DeclareTopologyAsync(config.RankCalculatorRabbitMqExchangeName, config.RankCalculatorRabbitMqQueueName);

        var rankCalculatorService = new RankCalculatorService(storageService, rabbitMqService, config);

        await rabbitMqService.ReceiveMessageAsync(config.RankCalculatorRabbitMqQueueName, rankCalculatorService.Process);

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
}