namespace Services.MessageBroker;

public interface IMessageBrokerService
{
    public Task SendMessageAsync(string queueName, string message);
    public Task ReceiveMessageAsync(string queueName, Action<string> messageHandler);
}