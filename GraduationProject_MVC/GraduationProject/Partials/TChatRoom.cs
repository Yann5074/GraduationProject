//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace GraduationProject.Models
//{
//    public partial class TChatRoom
//    {
//        // ✅ 外鍵屬性已在主檔：public int? FMemberId { get; set; }

//        // ✅ 導覽屬性（多對一）：這個聊天室屬於哪個會員
//        [ForeignKey(nameof(FMemberId))]
//        public virtual TMember? FMember { get; set; }

//        // ✅ 導覽屬性（一對多）：聊天室底下的所有訊息
//        public virtual ICollection<TMessage> TMessages { get; set; } = new List<TMessage>();
//    }
//}
