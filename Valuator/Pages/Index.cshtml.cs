using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Valuator.Services;

namespace Valuator.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IStorageService _redisService;

    public IndexModel(ILogger<IndexModel> logger, IStorageService redisService)
    {
        _logger = logger;
        _redisService = redisService;
    }

    public void OnGet()
    {

    }

    public IActionResult OnPost(string text)
    {
        _logger.LogDebug(text);

        if (String.IsNullOrEmpty(text))
        {
			return Redirect("index");
		}

		string id = Guid.NewGuid().ToString();

		string similarityKey = "SIMILARITY-" + id;
		string similarity = HasDuplicates(text) ? "1" : "0";
		_redisService.SetValue(similarityKey, similarity);

		string textKey = "TEXT-" + id;
        _redisService.SetValue(textKey, text);

		string rankKey = "RANK-" + id;
        _redisService.SetValue(rankKey, CalculateRank(text).ToString());       

		return Redirect($"summary?id={id}");
    }

	private bool HasDuplicates(string text)
    {
        return _redisService
            .GetAllValuesByKeyPrefix("TEXT")
			.Exists(value => text == value);
    }

    private static double CalculateRank(string text)
    {
        return text.Count(ch => !char.IsLetter(ch)) / (double)text.Length;
    }
}
