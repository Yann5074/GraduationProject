using Microsoft.AspNetCore.SignalR;

namespace ApiProject.Hubs
{
    public class ChatHub : Hub
    {
        // 前端在進入聊天室後要先呼叫
        public async Task JoinRoom(string chatRoomId)
        {
            //room:16 門牌號碼
            var roomGroup = $"room:{chatRoomId}";

            //這條連線加入這個群組，之後 Clients.Group(roomGroup) 就能只推給在這組的連線。
            //Context.ConnectionId：這個使用者目前這條 SignalR 連線的 ID。
            await Groups.AddToGroupAsync(Context.ConnectionId, roomGroup);
        }

        // 如果要從 Hub 直接發送也可以用這支
        public async Task SendToRole(string chatRoomId, string targetRole, string content)
        {
            await Clients.Group($"room:{chatRoomId}").SendAsync("ReceiveMessage", content, targetRole);
        }
    }
}
