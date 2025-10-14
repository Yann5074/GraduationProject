using System;
using System.Collections.Generic;

namespace GraduationProject.Models;

public partial class TContactForm
{
    public int FContactFormsId { get; set; }

    public int? FChatRoomId { get; set; }

    public int? FMemberId { get; set; }

    public string? FCompanyName { get; set; }

    public string? FContactName { get; set; }

    public string? FRegion { get; set; }

    public string? FPhone { get; set; }

    public string? FEmail { get; set; }

    public string? FSubject { get; set; }

    public bool FFromChatRoom { get; set; }

    public string? FVisitorKey { get; set; }

    public DateTime FCreatedAt { get; set; }
}
