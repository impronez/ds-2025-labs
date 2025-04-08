using RankCalculator.Hubs;
using RankCalculator.Services;
using Services.Common;
using Services.MessageBrokers;
using Services.Storages;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var config = new EnvironmentConfiguration();
builder.Services.AddSingleton(config);

var rabbitMqService = await CreateRankCalculatorRabbitMqServiceAsync(config);

builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(config.RedisConnectionString));
builder.Services.AddScoped<IStorageService, RedisStorageService>();

builder.Services.AddSingleton(rabbitMqService);

builder.Services.AddSingleton<RankCalculatorService>();

builder.Services.AddHostedService<RankCalculationMessageProcessor>();

builder.Services.AddSignalR();

var app = builder.Build();

app.MapHub<RankProcessingHub>("/rankHub");
app.Run();

static async Task<RankCalculatorRabbitMqService> CreateRankCalculatorRabbitMqServiceAsync(EnvironmentConfiguration configuration)
{
    var rabbitMqClient = await RabbitMqClient.CreateAsync(configuration.RabbitMqHostname);
    var rabbitMqService = new RankCalculatorRabbitMqService(rabbitMqClient, configuration.LoggerRabbitMqExchangeName);
    await rabbitMqService.DeclareTopologyAsync(configuration.RankCalculatorRabbitMqExchangeName, configuration.RankCalculatorRabbitMqQueueName);

    return rabbitMqService;
}