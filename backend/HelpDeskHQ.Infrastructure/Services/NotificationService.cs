using HelpDeskHQ.Core.Common.Exceptions;
using HelpDeskHQ.Core.DTOs.Notifications;
using HelpDeskHQ.Core.Interfaces;
using HelpDeskHQ.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskHQ.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly HelpDeskHQDbContext _context;

        public NotificationService(HelpDeskHQDbContext context)
        {
            _context = context;
        }

        public async Task<List<NotificationResponseDto>> GetMyNotificationsAsync(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return notifications.Select(n => new NotificationResponseDto
            {
                Id = n.Id,
                TicketId = n.TicketId,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            }).ToList();
        }

        public async Task MarkAsReadAsync(int notificationId, int userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null)
            {
                throw new NotFoundException("Notification not found.");
            }

            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }
}