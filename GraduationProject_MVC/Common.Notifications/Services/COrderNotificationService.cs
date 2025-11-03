using Common.Notifications.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Notifications.Services
{
    public class COrderNotificationService : IOrderNotificationService
    {
        private readonly IEmailSender _emailSender;

        public COrderNotificationService(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public Task SendOrderCreatedAsync(string toEmail, int orderId, string customerName, decimal total, CancellationToken ct = default)
        {
            var subject = $"訂單成立通知 #{orderId}";
            var html = $"""
                <p> {customerName} 您好: </p>
                <p> 您的訂單 <b>#{orderId}</b> 已成立</P>
                <p> 結帳金額: <b> NT$ {total.ToString("N0", CultureInfo.GetCultureInfo("zh-TW"))}</b></p>
                <p> 感謝您的購買! </p>
            """;
            return _emailSender.SendHtmlAsync(toEmail, subject, html, ct: ct);
        }

        public Task SendOrderStatusChangedAsync(string toEmail, int orderId, string customerName, string newStatus, CancellationToken ct = default)
        {
            var subject = $"訂單 #{orderId} {newStatus}";
            var html = $"""
                <p> {customerName} 您好: </p>
                <p> 您的訂單 {orderId} 已於 {DateTime.Now.ToString()} 更新為</p>
                <p> <b>{newStatus}</b> </p>
                """;
            return _emailSender.SendHtmlAsync(toEmail, subject, html, ct: ct);
        }

        public Task SendOrderDeliveryChangedAsync(string toEmail, int orderId, string customerName, string newDelivery, CancellationToken ct = default)
        {
            var subject = $"訂單 #{orderId} {newDelivery}";
            var html = $"""
                <p> {customerName} 您好: </p>
                <p> 您的訂單運輸狀態 已於 {DateTime.Now.ToString()} 更新為</p>
                <p>  <b>{newDelivery}</b> </p>
                """;
            return _emailSender.SendHtmlAsync(toEmail, subject, html, ct: ct);
        }
    }
}
