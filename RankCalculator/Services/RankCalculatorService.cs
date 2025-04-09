using System.Globalization;
using Services.Common;
using Services.Storages;
using Microsoft.AspNetCore.SignalR;
using RankCalculator.Hubs;

namespace RankCalculator.Services;

public class RankCalculatorService
{
    private const string RankPrefix = "RANK-";
    private const string TextPrefix = "TEXT-";

    private readonly EnvironmentConfiguration _envConfig;
    private readonly IStorageService _storageService;
    private readonly RankCalculatorRabbitMqService _rabbitMqService;
    private readonly IHubContext<RankProcessingHub> _hubContext;

    public RankCalculatorService(
        IStorageService storageService,
        RankCalculatorRabbitMqService rabbitMqService,
        IHubContext<RankProcessingHub> hubContext,
        EnvironmentConfiguration envConfig)
    {
        _storageService = storageService;
        _rabbitMqService = rabbitMqService;
        _hubContext = hubContext;
        _envConfig = envConfig;
    }

    public async Task Process(string id)
    {
        var text = _storageService.GetValue(TextPrefix + id);

        var rank = CalculateRank(text);
        var key = RankPrefix + id;

        var interval = TimeSpan.FromSeconds(new Random().Next(3, 15));
        Console.WriteLine($"Waiting {interval}");
        await Task.Delay(interval);

        _storageService.Save(key, rank.ToString(CultureInfo.InvariantCulture));

        var message = $"Id: {id}, rank: {rank}";
        await _rabbitMqService.SendMessageAsync(
            _envConfig.LoggerRabbitMqExchangeName,
            _envConfig.RankCalculatedRoutingKey,
            message);
        
        await _hubContext.Clients.Group(id).SendAsync("RankCalculated", new {
            Id = id,
            Rank = rank
        });
        
        Console.WriteLine("Message received");
    }

    private static double CalculateRank(string? text)
    {
        if (string.IsNullOrEmpty(text)) return 0;

        return text.Count(ch => !char.IsLetter(ch)) / (double)text.Length;
    }
}