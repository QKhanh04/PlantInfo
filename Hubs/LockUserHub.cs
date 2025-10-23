using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace PlantManagement.Hubs
{
    public class LockUserHub : Hub
    {
        // Mapping UserId với ConnectionId
        private static Dictionary<int, string> userConnections = new Dictionary<int, string>();

        public override async Task OnConnectedAsync()
        {
            // Lấy UserId từ Claims
            var userIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdStr, out int userId))
            {
                userConnections[userId] = Context.ConnectionId;
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdStr, out int userId))
            {
                userConnections.Remove(userId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        // Hàm gọi từ server để gửi thông báo khóa user
        public async Task SendLockNotification(int userId, string message)
        {
            if (userConnections.TryGetValue(userId, out var connId))
            {
                await Clients.Client(connId).SendAsync("Locked", message);
            }
        }
    }
}