using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Storages;

namespace Valuator.Pages;
public class SummaryModel : PageModel
{
    private readonly ILogger<SummaryModel> _logger;
    private readonly IStorageService _redisService;

    public SummaryModel(ILogger<SummaryModel> logger, IStorageService redisService)
    {
        _logger = logger;
        _redisService = redisService;
    }

    public double Rank { get; set; }
    public double Similarity { get; set; }
    
    public string Error { get; set; }

    public void OnGet(string id)
    {
        _logger.LogDebug(id);
        
        var rankValue = _redisService.GetValue($"RANK-{id}");
        var similarityValue = _redisService.GetValue($"SIMILARITY-{id}");

        if (!ParseValues(rankValue, similarityValue))
            return;

        Rank = double.Parse(rankValue!);
        Similarity = double.Parse(similarityValue!);
    }

    private bool ParseValues(string? rankValue, string? similarityValue)
    {
        if (string.IsNullOrEmpty(rankValue))
        {
            Error = "Информация об оценке не найдена. Попробуйте перезагрузить страницу";
            return false;
        }

        if (string.IsNullOrEmpty(similarityValue))
        {
            Error = "Информация о плагиате не найдена. Попробуйте перезагрузить страницу";
            return false;
        }

        return true;
    }
}