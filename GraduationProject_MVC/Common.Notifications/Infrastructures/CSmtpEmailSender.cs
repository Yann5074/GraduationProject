using Common.Notifications.Interfaces;
using Common.Notifications.Models;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace Common.Notifications.Infrastructures
{
    internal class CSmtpEmailSender : IEmailSender
    {
        private readonly EmailOptions _opt;

        public CSmtpEmailSender(IOptions<EmailOptions> opt)
        {
            _opt = opt.Value;
        }

        public async Task SendHtmlAsync(string to, string subject, string html, string? text = null, CancellationToken ct = default)
        {
            // 組裝信件
            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(_opt.FromName, _opt.FromAddress));
            msg.To.Add(MailboxAddress.Parse(to));
            msg.Subject = subject;

            var body = new BodyBuilder
            {
                HtmlBody = html,
                TextBody = text ?? "請查看 HTML 內容"
            };
            msg.Body = body.ToMessageBody();

            // 寄送信件
            using var smtp = new SmtpClient();
            smtp.Timeout = _opt.TimeoutSeconds * 1000;

            await smtp.ConnectAsync(_opt.Host, _opt.Port, SecureSocketOptions.StartTls, ct).ConfigureAwait(false);
            await smtp.AuthenticateAsync(_opt.UserName, _opt.AppPassword, ct).ConfigureAwait(false);
            await smtp.SendAsync(msg, ct).ConfigureAwait(false);
            await smtp.DisconnectAsync(true, ct).ConfigureAwait(false);

        }
    }
}
