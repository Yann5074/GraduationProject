using GraduationProject.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

public class ChatHub : Hub
{
    private readonly DbFurniMartContext _ctx;
    public ChatHub(DbFurniMartContext ctx) => _ctx = ctx;

    private static string RoomName(int chatRoomId) => $"room-{chatRoomId}";

    public Task JoinRoom(int chatRoomId)
        => Groups.AddToGroupAsync(Context.ConnectionId, RoomName(chatRoomId));

    public Task LeaveRoom(int chatRoomId)
        => Groups.RemoveFromGroupAsync(Context.ConnectionId, RoomName(chatRoomId));

    // 前端呼叫，寫入訊息並推播
    public async Task SendMessage(int chatRoomId, string senderType, int? senderId, string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return;

        var msg = new TMessage
        {
            FChatRoomId = chatRoomId,
            FSenderType = senderType,     // "member" / "employee" / "bot"
            FSenderId = senderId,
            FContent = content,
            FCreatedAt = DateTime.UtcNow  // 若全站使用台北時間也可改成 DateTime.UtcNow.AddHours(8)
        };

        _ctx.TMessages.Add(msg);
        await _ctx.SaveChangesAsync();

        var payload = new
        {
            id = msg.FMessagesId,
            senderType = msg.FSenderType,
            senderId = msg.FSenderId,
            content = msg.FContent,
            createdAt = msg.FCreatedAt
        };

        await Clients.Group(RoomName(chatRoomId))
                     .SendAsync("messageAdded", chatRoomId, payload);
    }
}
