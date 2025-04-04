namespace Valuator.Services.MessageBrokerService;

public interface IMessageBrokerService
{
    public Task SendMessageAsync(string exchangeName, string routingKey, string message);
}