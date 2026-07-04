using HelpDeskHQ.Core.DTOs.Notifications;

namespace HelpDeskHQ.Core.Interfaces
{
    public interface INotificationService
    {
        Task<List<NotificationResponseDto>> GetMyNotificationsAsync(int userId);
        Task MarkAsReadAsync(int notificationId, int userId);
    }
}