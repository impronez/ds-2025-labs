using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Common;
using Services.Storage;
using Valuator.Services;

namespace Valuator.Pages;

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
        
		var id = Guid.NewGuid().ToString();
		_storageService.SaveShardKey(id, country);

		var similarityKey = "SIMILARITY-" + id;
		var similarity = HasDuplicates(text, country) ? "1" : "0";
		
		_storageService.SaveByShardKey(similarityKey, similarity, country);

		var textKey = "TEXT-" + id;
        _storageService.SaveByShardKey(textKey, text, country);
        
        await _messageBrokerService.SendMessageAsync(string.Empty, _envConfig.RankCalculatorRabbitMqQueueName, id);

        var message = $"Id: {id}, similarity: {similarity}";
        await _messageBrokerService.SendMessageAsync(_envConfig.LoggerRabbitMqExchangeName, _envConfig.SimilarityCalculatedRoutingKey, message);
        
		return Redirect($"summary?id={id}");
    }

	private bool HasDuplicates(string text, string shardKey)
    {
        return _storageService
            .GetAllValuesByKeyPrefix("TEXT", shardKey)
			.Exists(value => text == value);
    }
}
