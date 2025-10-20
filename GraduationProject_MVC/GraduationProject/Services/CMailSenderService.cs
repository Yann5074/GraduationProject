using GraduationProject.Interfaces;
using GraduationProject.Options;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;


namespace GraduationProject.Services
{
    //Send Mail
    public class CMailSenderService : IEmployeeEmailSender
    {
        private readonly CEmployeeEmailOptions _opt;
        public CMailSenderService(IOptions<CEmployeeEmailOptions> opt) => _opt = opt.Value;

        public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
        {
            //建立新的信件物件
            var msg = new MimeKit.MimeMessage();
            //設定寄件者名稱與信箱
            msg.From.Add(new MailboxAddress(_opt.FromName, _opt.FromAddress));
            //設定收件者地址
            msg.To.Add(MimeKit.MailboxAddress.Parse(toEmail));
            //主題
            msg.Subject = subject;
            //內容（HTML格式）
            msg.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();


            //建立 SMTP 連線並寄出

            using var smtp = new SmtpClient();
            //ConnectAsync()連線到 SMTP 伺服器
            await smtp.ConnectAsync(_opt.Smtp.Host, _opt.Smtp.Port, SecureSocketOptions.StartTls, ct);
            //登入帳號密碼
            await smtp.AuthenticateAsync(_opt.Smtp.User, _opt.Smtp.Password, ct);
            //寄信
            await smtp.SendAsync(msg, ct);
            //關閉連線
            await smtp.DisconnectAsync(true, ct);
        }
    }
}
