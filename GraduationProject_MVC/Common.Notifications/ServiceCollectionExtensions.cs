using Common.Notifications.Infrastructures;
using Common.Notifications.Interfaces;
using Common.Notifications.Models;
using Common.Notifications.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Common.Notifications
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNotification(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<EmailOptions>(config.GetSection("NotificationEmail"));
            services.AddScoped<IEmailSender, CSmtpEmailSender>();
            services.AddScoped<IOrderNotificationService, COrderNotificationService>();
            return services;
        }
    }
}
