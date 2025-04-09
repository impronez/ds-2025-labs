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

    public double? Rank { get; set; }
    public double Similarity { get; set; }

    public string Id { get; private set; }

    public void OnGet(string id)
    {
        _logger.LogDebug(id);

        Id = id;

        var rankValue = _redisService.GetValue($"RANK-{id}");
        var similarityValue = _redisService.GetValue($"SIMILARITY-{id}");

        if (string.IsNullOrEmpty(rankValue) || string.IsNullOrEmpty(similarityValue))
            return;

        Rank = double.Parse(rankValue!);
        Similarity = double.Parse(similarityValue!);
    }
}