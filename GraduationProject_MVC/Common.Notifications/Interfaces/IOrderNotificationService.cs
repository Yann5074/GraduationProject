using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Notifications.Interfaces
{
    public interface IOrderNotificationService
    {
        public Task SendOrderCreatedAsync(string toEmail, int orderId, string customerName, decimal total, CancellationToken ct = default);

        public Task SendOrderStatusChangedAsync(string toEmail, int orderId, string customerName, string newStatus, CancellationToken ct = default);
    }
}
