using Services.Common;

namespace RankCalculator.Services;

public class RankCalculationMessageProcessor : BackgroundService
{
    private readonly RankCalculatorRabbitMqService _rabbitMqService;
    private readonly RankCalculatorService _rankCalculatorService;
    private readonly EnvironmentConfiguration _config;

    public RankCalculationMessageProcessor(
        RankCalculatorRabbitMqService rabbitMqService,
        RankCalculatorService rankCalculatorService,
        EnvironmentConfiguration config)
    {
        _rabbitMqService = rabbitMqService;
        _rankCalculatorService = rankCalculatorService;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await _rabbitMqService.ReceiveMessageAsync(
            _config.RankCalculatorRabbitMqQueueName,
            _rankCalculatorService.Process
        );
    }
}