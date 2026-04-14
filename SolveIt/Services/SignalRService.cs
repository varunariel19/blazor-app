using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Components;
using System.Net;

namespace SolveIt.Services
{
    public class SignalRService(NavigationManager navManager, IHttpContextAccessor httpContextAccessor) : IAsyncDisposable
    {
        private readonly NavigationManager _navManager = navManager;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private HubConnection? _hubConnection;

        public event Action<ConversationService.ConversationDto>? OnMessageReceived;
        public event Action<Guid, string>? OnMsgRead;

        public HubConnectionState State => _hubConnection?.State ?? HubConnectionState.Disconnected;

        public async Task StartConnectionAsync()
        {
            if (_hubConnection is not null && _hubConnection.State != HubConnectionState.Disconnected)
                return;

            var container = new CookieContainer();
            var context = _httpContextAccessor.HttpContext;

            if (context != null)
            {
                foreach (var cookie in context.Request.Cookies)
                {
                    container.Add(new Cookie(cookie.Key, cookie.Value, "/", context.Request.Host.Host));
                }
            }

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_navManager.ToAbsoluteUri("/chathub"), options =>
                {
                    options.Cookies = container;
                })
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<ConversationService.ConversationDto>("ReceiveMessage", (message) =>
            {
                OnMessageReceived?.Invoke(message);
            });

            _hubConnection.On<Guid , string>("ReceiveMessage", (InboxId , UserId) =>
            {
                OnMsgRead?.Invoke(InboxId , UserId);

            });

            await _hubConnection.StartAsync();
        }

        public async Task NotifyMsgReaded(Guid inboxId , string UserId)
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.SendAsync("MsgRead", inboxId , UserId);
            }
        }


        public async ValueTask DisposeAsync()
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.DisposeAsync();
            }
        }
    }
}