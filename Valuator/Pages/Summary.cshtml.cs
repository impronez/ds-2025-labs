using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Storage;

namespace Valuator.Pages;

[Authorize]
public class SummaryModel : PageModel
{
    private readonly ILogger<SummaryModel> _logger;
    private readonly IStorageService _storageService;

    public SummaryModel(ILogger<SummaryModel> logger, IStorageService storageService)
    {
        _logger = logger;
        _storageService = storageService;
    }

    public double Rank { get; set; }
    public double Similarity { get; set; }
    
    public string Error { get; set; }

    public void OnGet(string id)
    {
        string? username = User.Identity.Name;
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(userId))
        {
            Error = "Ошибка авторизации";
            return;
        }

        string? userIdByText = _storageService.GetById(id, $"USER-{id}");
        if (userIdByText != userId)
        {
            Error = "Ошибка доступа! Ваш ID не совпадает с пользовательским ID текста";
            return;
        }
        
        _logger.LogDebug(id);
        
        var rankValue = _storageService.GetById(id, $"RANK-{id}");
        var similarityValue = _storageService.GetById(id, $"SIMILARITY-{id}");

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