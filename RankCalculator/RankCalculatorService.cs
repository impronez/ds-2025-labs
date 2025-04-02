using System.Globalization;
using Services;

namespace RankCalculator;

public class RankCalculatorService
{
    private const string RankPrefix = "RANK-";
    private const string TextPrefix = "TEXT-";
    
    private readonly IStorageService _storageService;
    private readonly RankCalculatorRabbitMqService _rabbitMqService;

    public RankCalculatorService(IStorageService storageService, RankCalculatorRabbitMqService rabbitMqService)
    {
        _storageService = storageService;
        _rabbitMqService = rabbitMqService;
    }
    
    public async Task Process(string id)
    {
        var text = _storageService.GetValue(TextPrefix + id);
        
        var rank = CalculateRank(text);
        var key = RankPrefix + id;

        SaveRank(key, rank);

        var message = $"Id: {id}, rank: {rank}";
        await _rabbitMqService.SendLogMessage(message);
    }

    private void SaveRank(string key, double value)
    {
        _storageService.Save(key, value.ToString(CultureInfo.InvariantCulture));
    }
    
    private static double CalculateRank(string? text)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        
        return text.Count(ch => !char.IsLetter(ch)) / (double)text.Length;
    }
}