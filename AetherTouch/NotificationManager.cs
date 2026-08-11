using System;
using System.Collections.Generic;
using System.Text;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Plugin.Services;

namespace AetherTouch
{
    public class NotificationManager(INotificationManager notificationManager)
    {
        private readonly INotificationManager notifManager = notificationManager;

        public void notifyDeviceConnected(string deviceName)
        {
            notifManager.AddNotification(new Notification()
            {
                InitialDuration = TimeSpan.FromSeconds(5),
                Title = "Device Connected",
                Content = $"[{deviceName}] connected.",
                Minimized = false,
                Type = NotificationType.Success
            });
        }

        public void notifyDeviceDisconnected(string deviceName)
        {
            notifManager.AddNotification(new Notification()
            {
                InitialDuration = TimeSpan.FromSeconds(5),
                Title = "Device Disconnected",
                Content = $"[{deviceName}] disconnected.",
                Minimized = false,
                Type = NotificationType.Warning
            });
        }

        public void notifyServerDisconnected()
        {
            notifManager.AddNotification(new Notification()
            {
                InitialDuration = TimeSpan.FromSeconds(5),
                Title = "Intiface Connection Lost",
                Content = $"Connection to the Intiface server was lost",
                Minimized = false,
                Type = NotificationType.Error
            });
        }
    }
}
