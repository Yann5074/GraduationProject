// Services/MessageService.cs
using GraduationProject.DTOs;
using GraduationProject.Interfaces;
using GraduationProject.Models;
using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Services
{
    public class MessageService : IMessageService
    {
        private readonly DbFurniMartContext _ctx;
        public MessageService(DbFurniMartContext ctx) => _ctx = ctx;

        public async Task<MessageDto> SaveMessageAsync(
            int chatRoomId, string senderType, int? senderId, string content,
            CancellationToken ct = default)
        {
            // 簡單防呆
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("content is empty");

            var msg = new TMessage
            {
                FChatRoomId = chatRoomId,
                FSenderType = senderType,
                FSenderId = senderId,
                FContent = content,
                FCreatedAt = DateTime.Now
            };

            _ctx.TMessages.Add(msg);
            await _ctx.SaveChangesAsync(ct);

            // 同步更新聊天室最後訊息時間（若資料表有此欄位）
            var room = await _ctx.TChatRooms.FirstOrDefaultAsync(r => r.FChatRoomId == chatRoomId, ct);
            if (room != null)
            {
                room.FLastMessageAt = msg.FCreatedAt;
                await _ctx.SaveChangesAsync(ct);
            }

            return new MessageDto
            {
                MessageId = msg.FMessagesId,
                SenderType = msg.FSenderType,
                SenderId = msg.FSenderId,
                Content = msg.FContent,
                CreatedAt = msg.FCreatedAt
            };
        }

        public async Task<List<MessageDto>> GetHistoryAsync(
            int chatRoomId, int take = 50, int? beforeMessageId = null,
            CancellationToken ct = default)
        {
            var q = _ctx.TMessages.AsNoTracking()
                .Where(m => m.FChatRoomId == chatRoomId);

            if (beforeMessageId.HasValue)
            {
                var beforeTime = await _ctx.TMessages
                    .Where(m => m.FMessagesId == beforeMessageId.Value)
                    .Select(m => m.FCreatedAt)
                    .FirstOrDefaultAsync(ct);

                if (beforeTime != null)
                    q = q.Where(m => m.FCreatedAt < beforeTime);
            }

            var list = await q.OrderByDescending(m => m.FCreatedAt)
                .Take(take)
                .Select(m => new MessageDto
                {
                    MessageId = m.FMessagesId,
                    SenderType = m.FSenderType,
                    SenderId = m.FSenderId,
                    Content = m.FContent ?? m.FContentJson,
                    CreatedAt = m.FCreatedAt
                })
                .ToListAsync(ct);

            list.Reverse(); // 由舊到新回傳
            return list;
        }
    }
}
