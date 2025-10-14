using System;
using System.Collections.Generic;

namespace GraduationProject.Models;

public partial class TMessage
{
    public int FMessagesId { get; set; }

    public int FChatRoomId { get; set; }

    public string FSenderType { get; set; } = null!;

    public int? FSenderId { get; set; }

    public string? FContent { get; set; }

    public byte FContentType { get; set; }

    public string? FContentJson { get; set; }

    public int? FParentMessageId { get; set; }

    public string? FMetaJson { get; set; }

    public DateTime FCreatedAt { get; set; }
}
