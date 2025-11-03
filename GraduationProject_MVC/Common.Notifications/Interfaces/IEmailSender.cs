using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Notifications.Interfaces
{
    public interface IEmailSender
    {
        public Task SendHtmlAsync(string to, string subject, string html, string? text = null, CancellationToken ct = default);
    }
}
