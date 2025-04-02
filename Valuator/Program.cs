using Common.MessageBroker;
using StackExchange.Redis;
using Services;
using Valuator.Services;

namespace Valuator;

public class Program
{
        public static async Task Main(string[] args)
        {
	        var redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");
	        
            var builder = WebApplication.CreateBuilder(args);

            var rabbitMqService = await GetRabbitMqServiceAsync();

            builder.Services.AddSingleton<IMessageBrokerService>(_ => rabbitMqService);
            
            // Add services to the container.
            builder.Services.AddRazorPages();	
		    builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString!));
            builder.Services.AddScoped<IStorageService, RedisStorageService>();

		    var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }

        private static async Task<ValuatorRabbitMqService> GetRabbitMqServiceAsync()
        {
	        var rabbitMqHostname = Environment.GetEnvironmentVariable("RABBITMQ_HOSTNAME");
	        var rankCalculatorRabbitMqQueueName = Environment.GetEnvironmentVariable("RANK_CALCULATOR_RABBIT_MQ_QUEUE_NAME");
	        var rankCalculatorRabbitMqExchangeName = Environment.GetEnvironmentVariable("RANK_CALCULATOR_RABBIT_MQ_EXCHANGE_NAME");
            
	        var loggerRabbitMqExchangeName = Environment.GetEnvironmentVariable("LOGGER_RABBIT_MQ_EXCHANGE_NAME");

	        var rabbitMqClient = await RabbitMqClient.CreateAsync(rabbitMqHostname!);
	        var rabbitMqService = new ValuatorRabbitMqService(rabbitMqClient, loggerRabbitMqExchangeName!);
	        await rabbitMqService.DeclareTopologyAsync(rankCalculatorRabbitMqExchangeName!, rankCalculatorRabbitMqQueueName!);

	        return rabbitMqService;
        }
}
