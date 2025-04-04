using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Common;
using Services.Storage;
using Valuator.Services.MessageBrokerService;

namespace Valuator.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IStorageService _redisService;
    private readonly IMessageBrokerService _messageBrokerService;
    private readonly EnvironmentConfiguration _envConfig;
    
    public IndexModel(
	    ILogger<IndexModel> logger,
	    IStorageService redisService,
	    IMessageBrokerService messageBrokerService,
	    IConfiguration configuration,
	    EnvironmentConfiguration environmentConfiguration)
    {
        _logger = logger;
        _redisService = redisService;
        _messageBrokerService = messageBrokerService;
        _envConfig = environmentConfiguration;
    }

    public void OnGet()
    {

    }

    public async Task<IActionResult> OnPost(string text)
    {
        _logger.LogDebug(text);

        if (String.IsNullOrEmpty(text))
        {
			return Redirect("index");
		}

		var id = Guid.NewGuid().ToString();

		var similarityKey = "SIMILARITY-" + id;
		var similarity = HasDuplicates(text) ? "1" : "0";
		_redisService.Save(similarityKey, similarity);

		var textKey = "TEXT-" + id;
        _redisService.Save(textKey, text);
        
        await _messageBrokerService.SendMessageAsync(string.Empty, _envConfig.RankCalculatorRabbitMqQueueName, id);

        var message = $"Id: {id}, similarity: {similarity}";
        await _messageBrokerService.SendMessageAsync(_envConfig.LoggerRabbitMqExchangeName, _envConfig.SimilarityCalculatedRoutingKey, message);

		return Redirect($"summary?id={id}");
    }

	private bool HasDuplicates(string text)
    {
        return _redisService
            .GetAllValuesByKeyPrefix("TEXT")
			.Exists(value => text == value);
    }
}
