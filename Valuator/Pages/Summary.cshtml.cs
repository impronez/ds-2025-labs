using Microsoft.AspNetCore.Mvc.RazorPages;
using Services;

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

    public void OnGet(string id)
    {
        _logger.LogDebug(id);

        Rank = ParseDouble(_redisService.GetValue($"RANK-{id}"));
        Similarity = ParseDouble(_redisService.GetValue($"SIMILARITY-{id}"));
    }

    private static double ParseDouble(string? value)
    {
        if (string.IsNullOrEmpty(value)) return 0;

        return double.Parse(value);
    }
}
