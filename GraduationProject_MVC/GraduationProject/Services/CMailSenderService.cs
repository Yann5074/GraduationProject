using GraduationProject.Interfaces;
using GraduationProject.Options;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;


namespace GraduationProject.Services
{
    public class CMailSenderService : IEmployeeEmailSender
    {
        private readonly CEmployeeEmailOptions _opt;
        public CMailSenderService(IOptions<CEmployeeEmailOptions> opt) => _opt = opt.Value;

        public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
        {
            var msg = new MimeKit.MimeMessage();
            msg.From.Add(new MailboxAddress(_opt.FromName, _opt.FromAddress));
            msg.To.Add(MimeKit.MailboxAddress.Parse(toEmail));
            msg.Subject = subject;
            msg.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_opt.Smtp.Host, _opt.Smtp.Port, SecureSocketOptions.StartTls, ct);
            await smtp.AuthenticateAsync(_opt.Smtp.User, _opt.Smtp.Password, ct);
            await smtp.SendAsync(msg, ct);
            await smtp.DisconnectAsync(true, ct);
        }
    }
}
