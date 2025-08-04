using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace API.Hubs
{
    public class CallHub : Hub
    {
        // Lưu trữ accountId (int) ↔ connectionId
        public static ConcurrentDictionary<int, string> UserConnectionMap = new();

        public override Task OnConnectedAsync()
        {
            var userIdStr = Context.GetHttpContext()?.Request.Query["userId"].ToString();

            if (int.TryParse(userIdStr, out int userId))
            {
                Console.WriteLine($"[SignalR] User {userId} connected with ConnectionId: {Context.ConnectionId}");
                UserConnectionMap[userId] = Context.ConnectionId;
            }
            else
            {
                Console.WriteLine("[SignalR] Cannot parse userId from query string.");
            }

            return base.OnConnectedAsync();
        }


        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var userEntry = UserConnectionMap.FirstOrDefault(kv => kv.Value == Context.ConnectionId);
            if (!userEntry.Equals(default(KeyValuePair<int, string>)))
            {
                UserConnectionMap.TryRemove(userEntry.Key, out _);
                Console.WriteLine($"User {userEntry.Key} disconnected.");
            }

            return base.OnDisconnectedAsync(exception);
        }

        public async Task JoinRoom(string roomToken)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomToken);
        }

        public async Task SendOffer(string roomToken, object offer)
        {
            await Clients.OthersInGroup(roomToken).SendAsync("ReceiveOffer", offer, Context.ConnectionId);
        }

        public async Task SendAnswer(string roomToken, object answer)
        {
            await Clients.OthersInGroup(roomToken).SendAsync("ReceiveAnswer", answer, Context.ConnectionId);
        }

        public async Task SendIceCandidate(string roomToken, object candidate)
        {
            await Clients.OthersInGroup(roomToken).SendAsync("ReceiveIceCandidate", candidate, Context.ConnectionId);
        }
    }
}
