using Microsoft.AspNetCore.Authentication.Cookies;
using Services.Common;
using Services.MessageBroker;
using Services.Storage;
using Valuator.Services;

namespace Valuator;

public class Program
{
    public static async Task Main(string[] args)
    {
        var config = new EnvironmentConfiguration();

        var builder = WebApplication.CreateBuilder(args);

        var rabbitMqService = await GetRabbitMqServiceAsync(config);
        var redisStorageService = new RedisStorageService(config.RedisPassword);
        var redisUserStorageService = new RedisUserStorageService(config.RedisPassword);

        builder.Services.AddSingleton(config);

        builder.Services.AddSingleton<IMessageBrokerService>(_ => rabbitMqService);
        builder.Services.AddScoped<IStorageService>(_ => redisStorageService);
        builder.Services.AddScoped<IUserStorageService>(_ => redisUserStorageService);

        builder.Services.AddRazorPages();
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Login";
                options.LogoutPath = "/Logout";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            });

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }

        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapRazorPages();
        app.MapDefaultControllerRoute();

        app.Run();
    }

    private static async Task<ValuatorRabbitMqService> GetRabbitMqServiceAsync(EnvironmentConfiguration config)
    {
        var rabbitMqClient = await RabbitMqClient.CreateAsync(config.RabbitMqHostname, config.RabbitMqUsername, config.RabbitMqPassword);
        var rabbitMqService = new ValuatorRabbitMqService(rabbitMqClient, config.LoggerRabbitMqExchangeName);
        await rabbitMqService.DeclareTopologyAsync(config.RankCalculatorRabbitMqExchangeName,
            config.RankCalculatorRabbitMqQueueName);

        return rabbitMqService;
    }
}