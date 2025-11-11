
using Microsoft.AspNetCore.SignalR;

namespace ApiProject.Hubs
{
    public class ChatHub : Hub
    {
        // 前端連上後要呼叫，加入 room:{chatRoomId}
        public async Task JoinRoom(string chatRoomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"room:{chatRoomId}");
        }

        // （選用）員工或系統直接廣播（物件 payload）
        public async Task SendToRole(string chatRoomId, string senderType, string content)
        {
            await Clients.Group($"room:{chatRoomId}")
                .SendAsync("ReceiveMessage", new
                {
                    chatRoomId,
                    senderType,  // "employee" / "member" / "user" / "bot"
                    content,
                    createdAt = DateTime.UtcNow
                    
                }); //記得加上,ct
        }
    }
}

