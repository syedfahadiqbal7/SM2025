using Microsoft.AspNetCore.SignalR;

namespace AFFZ_API.NotificationsHubs
{
    public class NotificationHub : Hub
    {
        // Method to send notification to clients
        //only the intended client group will receive the specific notifications, ensuring that notifications are not shown across unintended applications.
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
    }
}
