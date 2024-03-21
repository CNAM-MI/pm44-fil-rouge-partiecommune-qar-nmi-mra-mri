using Microsoft.AspNetCore.SignalR;

namespace RestOlympe_Server.Hubs
{
    public class TestHub : Hub
    {
        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}
