using AFFZ_API.Models;
using Microsoft.AspNetCore.SignalR;

namespace AFFZ_API.NotificationsHubs
{
    public class NotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task AddNotification(Notification notification)
        {
            // Insert notification into database (pseudo-code)
            // _dbContext.Notifications.Add(notification);
            // await _dbContext.SaveChangesAsync();

            // Notify all clients about the new notification
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification.Message);
        }
    }
}
