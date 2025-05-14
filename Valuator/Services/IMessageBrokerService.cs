namespace Valuator.Services;

public interface IMessageBrokerService
{
    public Task SendMessageAsync(string exchangeName, string routingKey, string message);
}