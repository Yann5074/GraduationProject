using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Notifications.Models
{
    public class EmailOptions
    {
        public string Host { get; set; } = "smtp.gmail.com";

        public int Port { get; set; } = 587; // 587 => STARTTLS, 465 => SSL/TLS

        public string UserName { get; set; } = default!;

        public string AppPassword { get; set; } = default!;

        public string FromAddress { get; set; } = default!;

        public string FromName { get; set; } = "Viewrniture 通知";

        public int TimeoutSeconds { get; set; } = 30;
    }
}
