using Microsoft.AspNetCore.SignalR;

namespace API.Hubs
{
    public class MessageHub : Hub
    {      
    public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        // (Có thể thêm method LeaveGroup nếu muốn)
        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
    }
}
