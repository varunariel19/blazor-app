using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;


namespace SolveIt.UI_state
{

    public class SignalRService(NavigationManager navManager) : IAsyncDisposable
    {
        public HubConnection HubConnection { get; private set; } = new HubConnectionBuilder()
                .WithUrl(navManager.ToAbsoluteUri("/chathub"))
                .WithAutomaticReconnect()
                .Build();

        public async Task StartAsync()
        {
            if (HubConnection.State == HubConnectionState.Disconnected)
            {
                await HubConnection.StartAsync();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (HubConnection is not null)
            {
                await HubConnection.DisposeAsync();
            }
        }
    }


}
