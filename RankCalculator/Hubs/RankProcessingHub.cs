using Microsoft.AspNetCore.SignalR;

namespace RankCalculator.Hubs;

public class RankProcessingHub : Hub
{
    public async Task JoinGroup(string id)
    {
        Console.WriteLine($"Connected: {id} | {Context.ConnectionId}");
        await Groups.AddToGroupAsync(Context.ConnectionId, id);
    }

    public async Task LeaveGroup(string id)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, id);
    }
}