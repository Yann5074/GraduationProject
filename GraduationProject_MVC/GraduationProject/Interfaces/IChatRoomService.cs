// Interfaces/IMessageService.cs
using GraduationProject.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GraduationProject.Interfaces
{
    public interface IMessageService
    {
        Task<MessageDto> SaveMessageAsync(
            int chatRoomId, string senderType, string? senderId, string content,
            CancellationToken ct = default);

        Task<List<MessageDto>> GetHistoryAsync(
            int chatRoomId, int take = 50, int? beforeMessageId = null,
            CancellationToken ct = default);
    }
}
