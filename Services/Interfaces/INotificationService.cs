using StudentPlacementManagement.Models;

namespace StudentPlacementManagement.Services.Interfaces
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string userId, string title, string message, string type = "General");
        Task<List<Notification>> GetUserNotificationsAsync(string userId, int count = 20);
        Task<int> GetUnreadCountAsync(string userId);
        Task<bool> MarkAsReadAsync(int notificationId, string userId);
        Task<bool> MarkAllAsReadAsync(string userId);
    }
}
