using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace SolveIt.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private const string UserIdPrefix = "user-";

        public string? GetMyUserId()
        {
            return Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public override async Task OnConnectedAsync()
        {

            string? userId = null;
            if (Context.User?.Identity?.IsAuthenticated == true)
            {
                userId = Context.UserIdentifier;
            }
            var connectionId = Context.ConnectionId;
            
            if (!string.IsNullOrEmpty(userId))
            {
                Console.WriteLine($"[ChatHub] User Connected - UserId: {userId}, ConnectionId: {connectionId}");
                await Groups.AddToGroupAsync(connectionId, UserIdPrefix + userId);
            }
            else
            {
                Console.WriteLine($"[ChatHub] Anonymous Connected - ConnectionId: {connectionId}");
            }

            await base.OnConnectedAsync();
        }

        public async Task MsgRead(Guid inboxId , string userId)
        {
         
            await Clients
                .Group(UserIdPrefix + userId)
                .SendAsync("MsgReaded", inboxId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine($"[ChatHub] Disconnected - UserId: {userId}, ConnectionId: {Context.ConnectionId}, Exception: {exception?.Message}");
            await base.OnDisconnectedAsync(exception);
        }
    }
}