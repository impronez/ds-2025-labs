using System.Globalization;
using Services.Common;
using Services.Storage;

namespace RankCalculator;

public class RankCalculatorService
{
    private const string RankPrefix = "RANK-";
    private const string TextPrefix = "TEXT-";
    
    private readonly EnvironmentConfiguration _envConfig;
    private readonly IStorageService _storageService;
    private readonly RankCalculatorRabbitMqService _rabbitMqService;

    public RankCalculatorService(
        IStorageService storageService, 
        RankCalculatorRabbitMqService rabbitMqService,
        EnvironmentConfiguration envConfig)
    {
        _storageService = storageService;
        _rabbitMqService = rabbitMqService;
        _envConfig = envConfig;
    }
    
    public async Task Process(string id)
    {
        var text = _storageService.GetValue(TextPrefix + id);
        
        var rank = CalculateRank(text);
        var key = RankPrefix + id;

        SaveRank(key, rank);

        var message = $"Id: {id}, rank: {rank}";
        await _rabbitMqService.SendMessageAsync(
            _envConfig.LoggerRabbitMqExchangeName,
            _envConfig.RankCalculatedRoutingKey,
            message);
    }
    
    public static double CalculateRank(string? text)
    {
        Console.WriteLine($"Text: {text}");
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        StringInfo stringInfo = new StringInfo(text);
        int total = stringInfo.LengthInTextElements;

        int nonLetterCount = Enumerable
            .Range(0, total)
            .Select(i => stringInfo.SubstringByTextElements(i, 1))
            .Count(element => !element.Any(char.IsLetter));
        Console.WriteLine($"total: {total}, non-letter count: {nonLetterCount}");
        return (double)nonLetterCount / total;
    }

    private void SaveRank(string key, double value)
    {
        _storageService.Save(key, value.ToString(CultureInfo.InvariantCulture));
    }
}