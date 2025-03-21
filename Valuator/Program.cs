using StackExchange.Redis;
using Services;
using Services.MessageBroker;

namespace Valuator;

public class Program
{
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var messageBrokerServiceConnectionString = Environment.GetEnvironmentVariable("RABBITMQ_HOSTNAME");
            var messageBrokerService = await RabbitMqService.CreateAsync(messageBrokerServiceConnectionString);

            builder.Services.AddSingleton<IMessageBrokerService>(_ => messageBrokerService);
            
            // Add services to the container.
            builder.Services.AddRazorPages();
		    builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
		    {
			    var configuration = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");
			    return ConnectionMultiplexer.Connect(configuration);
		    });
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
}
