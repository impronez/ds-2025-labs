using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services;
using Services.MessageBroker;

namespace Valuator.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IStorageService _redisService;
    private readonly IMessageBrokerService _messageBrokerService;
    
    public IndexModel(ILogger<IndexModel> logger, IStorageService redisService, IMessageBrokerService messageBrokerService)
    {
        _logger = logger;
        _redisService = redisService;
        _messageBrokerService = messageBrokerService;
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

		string id = Guid.NewGuid().ToString();

		string similarityKey = "SIMILARITY-" + id;
		string similarity = HasDuplicates(text) ? "1" : "0";
		_redisService.Save(similarityKey, similarity);

		string textKey = "TEXT-" + id;
        _redisService.Save(textKey, text);

        await _messageBrokerService.SendMessageAsync(RabbitMqService.RankCalculatorQueueName, id); 

		return Redirect($"summary?id={id}");
    }

	private bool HasDuplicates(string text)
    {
        return _redisService
            .GetAllValuesByKeyPrefix("TEXT")
			.Exists(value => text == value);
    }
}
