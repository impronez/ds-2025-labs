using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services;
using Valuator.Services;

namespace Valuator.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IStorageService _redisService;
    private readonly IMessageBrokerService _messageBrokerService;
    private readonly string _rankCalculatorMessageBrokerQueueName;
    private readonly string _loggerMessageBrokerExchangeName;
    private readonly string _similarityCalculatedRoutingKey;
    
    public IndexModel(
	    ILogger<IndexModel> logger,
	    IStorageService redisService,
	    IMessageBrokerService messageBrokerService,
	    IConfiguration configuration)
    {
        _logger = logger;
        _redisService = redisService;
        _messageBrokerService = messageBrokerService;
        _rankCalculatorMessageBrokerQueueName = Environment.GetEnvironmentVariable("RANK_CALCULATOR_RABBIT_MQ_QUEUE_NAME");
        _loggerMessageBrokerExchangeName = Environment.GetEnvironmentVariable("LOGGER_RABBIT_MQ_EXCHANGE_NAME");
        _similarityCalculatedRoutingKey = Environment.GetEnvironmentVariable("SIMILARITY_CALCULATED_ROUTING_KEY");
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
        
        await _messageBrokerService.SendMessageAsync(string.Empty, _rankCalculatorMessageBrokerQueueName, id);

        var message = $"Id: {id}, similarity: {similarity}";
        await _messageBrokerService.SendMessageAsync(_loggerMessageBrokerExchangeName, _similarityCalculatedRoutingKey, message);

		return Redirect($"summary?id={id}");
    }

	private bool HasDuplicates(string text)
    {
        return _redisService
            .GetAllValuesByKeyPrefix("TEXT")
			.Exists(value => text == value);
    }
}
