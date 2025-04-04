using StackExchange.Redis;
using Services.Common;
using Services.MessageBroker;
using Services.Storage;
using Valuator.Services.MessageBrokerService;

namespace Valuator;

public class Program
{
        public static async Task Main(string[] args)
        {
	        var config = new EnvironmentConfiguration();
	        
            var builder = WebApplication.CreateBuilder(args);

            var rabbitMqService = await GetRabbitMqServiceAsync(config);

            builder.Services.AddSingleton(config);

            builder.Services.AddSingleton<IMessageBrokerService>(_ => rabbitMqService);
            
            // Add services to the container.
            builder.Services.AddRazorPages();	
		    builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(config.RedisConnectionString));
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

        private static async Task<ValuatorRabbitMqService> GetRabbitMqServiceAsync(EnvironmentConfiguration config)
        {
	        var rabbitMqClient = await RabbitMqClient.CreateAsync(config.RabbitMqHostname);
	        var rabbitMqService = new ValuatorRabbitMqService(rabbitMqClient, config.LoggerRabbitMqExchangeName);
	        await rabbitMqService.DeclareTopologyAsync(config.RankCalculatorRabbitMqExchangeName, config.RankCalculatorRabbitMqQueueName);

	        return rabbitMqService;
        }
}
