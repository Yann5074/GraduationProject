using System;
using System.Collections.Generic;

namespace GraduationProject.Models;

public partial class TChatRoom
{
    public int FChatRoomId { get; set; }

    public int? FMemberId { get; set; }

    public string? FVisitorKey { get; set; }

    public string? FChannel { get; set; }

    public string? FStatus { get; set; }

    public int? FEmployeeId { get; set; }

    public DateTime? FCreatedAt { get; set; }

    public DateTime? FFirstMessageAt { get; set; }

    public DateTime? FLastMessageAt { get; set; }

    public DateTime? FClosedAt { get; set; }

    public int? FIsBotActive { get; set; }

    public string? FBotStateJson { get; set; }

}
