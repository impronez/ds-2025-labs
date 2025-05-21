using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Common;
using Services.Storage;
using Valuator.Services;

namespace Valuator.Pages;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IStorageService _storageService;
    private readonly IMessageBrokerService _messageBrokerService;
    private readonly EnvironmentConfiguration _envConfig;
    
    public IndexModel(
	    ILogger<IndexModel> logger,
	    IStorageService storageService,
	    IMessageBrokerService messageBrokerService,
	    IConfiguration configuration,
	    EnvironmentConfiguration environmentConfiguration)
    {
        _logger = logger;
        _storageService = storageService;
        _messageBrokerService = messageBrokerService;
        _envConfig = environmentConfiguration;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost(string text, string country)
    {
        _logger.LogDebug($"{text} to {country}");
        
        if (string.IsNullOrEmpty(text))
        {
			return Redirect("index");
		}

        if (string.IsNullOrEmpty(country))
        {
	        Console.WriteLine($"Invalid shard key: {country}");
	        return Redirect("index");
        }
        
        string? username = User.Identity.Name;
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(userId))
        {
	        Console.WriteLine($"Not authorized");
	        return Redirect("index"); 
        }
        
		string textId = Guid.NewGuid().ToString();
		_storageService.SaveShardKey(textId, country);
		
		string userKey = "USER-" + textId;
		_storageService.SaveByShardKey(userKey, userId, country);

		string similarityKey = "SIMILARITY-" + textId;
		string similarity = HasDuplicates(text, country) ? "1" : "0";
		
		_storageService.SaveByShardKey(similarityKey, similarity, country);

		string textKey = "TEXT-" + textId;
        _storageService.SaveByShardKey(textKey, text, country);
        
        await _messageBrokerService.SendMessageAsync(string.Empty, _envConfig.RankCalculatorRabbitMqQueueName, textId);

        string message = $"Id: {textId}, similarity: {similarity}";
        await _messageBrokerService.SendMessageAsync(_envConfig.LoggerRabbitMqExchangeName, _envConfig.SimilarityCalculatedRoutingKey, message);
        
		return Redirect($"summary?id={textId}");
    }

	private bool HasDuplicates(string text, string shardKey)
    {
        return _storageService
            .GetAllValuesByKeyPrefix("TEXT", shardKey)
			.Exists(value => text == value);
    }
}
