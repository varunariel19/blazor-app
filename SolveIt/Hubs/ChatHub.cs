using Microsoft.AspNetCore.SignalR;

namespace SolveIt.Hubs
{
    public class ChatHub : Hub
    {
        public async Task JoinInbox(string inboxId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, inboxId);
        }

        // Leave the inbox room
        public async Task LeaveInbox(string inboxId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, inboxId);
        }

        // Send message to everyone in the inbox group
        public async Task SendMessage(string inboxId, object message)
        {
            await Clients.Group(inboxId).SendAsync("ReceiveMessage", message);
        }
    }
}