namespace Services.Common;

public class EnvironmentConfiguration
{
    public string RabbitMqHostname { get; }
    public string RedisConnectionString { get; }
    public string RankCalculatorRabbitMqQueueName { get; }
    public string RankCalculatorRabbitMqExchangeName { get; }
    public string LoggerRabbitMqExchangeName { get; }
    public string LoggerRabbitMqQueueName { get; }
    public string RankCalculatedRoutingKey { get; }
    public string SimilarityCalculatedRoutingKey { get; }

    public EnvironmentConfiguration()
    {
        RedisConnectionString = GetRequiredEnvironmentVariable("REDIS_CONNECTION_STRING");
        RabbitMqHostname = GetRequiredEnvironmentVariable("RABBITMQ_HOSTNAME");
        
        RankCalculatorRabbitMqQueueName = GetRequiredEnvironmentVariable("RANK_CALCULATOR_RABBIT_MQ_QUEUE_NAME");
        RankCalculatorRabbitMqExchangeName = GetRequiredEnvironmentVariable("RANK_CALCULATOR_RABBIT_MQ_EXCHANGE_NAME");
        LoggerRabbitMqExchangeName = GetRequiredEnvironmentVariable("LOGGER_RABBIT_MQ_EXCHANGE_NAME");
        LoggerRabbitMqQueueName = GetRequiredEnvironmentVariable("LOGGER_RABBIT_MQ_QUEUE_NAME");
        
        RankCalculatedRoutingKey = GetRequiredEnvironmentVariable("RANK_CALCULATED_ROUTING_KEY");
        SimilarityCalculatedRoutingKey = GetRequiredEnvironmentVariable("SIMILARITY_CALCULATED_ROUTING_KEY");
    }
    
    private static string GetRequiredEnvironmentVariable(string variableName)
    {
        var value = Environment.GetEnvironmentVariable(variableName);
        if (string.IsNullOrEmpty(value))
        {
            throw new InvalidOperationException($"Required environment variable '{variableName}' is not set or empty.");
        }
        return value;
    }
}