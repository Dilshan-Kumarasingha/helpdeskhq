using HelpDeskHQ.Core.Entities;
using HelpDeskHQ.Core.Enums;
using HelpDeskHQ.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskHQ.API.Jobs
{
    public class AutoCloseJob
    {
        private readonly HelpDeskHQDbContext _context;
        private readonly ILogger<AutoCloseJob> _logger;

        // Tickets in Resolved status with no employee response
        // after this many days will be auto-closed.
        private const int AutoCloseDays = 3;

        public AutoCloseJob(HelpDeskHQDbContext context, ILogger<AutoCloseJob> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            _logger.LogInformation("AutoCloseJob started at {Time}", DateTime.UtcNow);

            var cutoff = DateTime.UtcNow.AddDays(-AutoCloseDays);

            // Find all tickets that have been sitting in Resolved
            // for longer than the confirmation window
            var tickets = await _context.Tickets
                .Where(t => t.Status == TicketStatus.Resolved
                         && t.ResolvedAt != null
                         && t.ResolvedAt.Value <= cutoff)
                .ToListAsync();

            _logger.LogInformation(
                "AutoCloseJob found {Count} ticket(s) to auto-close", tickets.Count);

            var now = DateTime.UtcNow;

            foreach (var ticket in tickets)
            {
                var oldStatus = ticket.Status;
                ticket.Status = TicketStatus.Closed;

                // Notify the employee who raised the ticket
                _context.Notifications.Add(new Notification
                {
                    UserId = ticket.RaisedByUserId,
                    TicketId = ticket.Id,
                    Message = $"✅ Ticket {ticket.TicketNumber} has been automatically closed " +
                              $"after {AutoCloseDays} days with no response.",
                    CreatedAt = now
                });

                // Audit trail
                _context.TicketStatusHistories.Add(new TicketStatusHistory
                {
                    TicketId = ticket.Id,
                    FromStatus = oldStatus,
                    ToStatus = TicketStatus.Closed,
                    ChangedByUserId = ticket.RaisedByUserId,
                    ChangedAt = now,
                    Note = $"Auto-closed after {AutoCloseDays} days with no employee confirmation"
                });

                _logger.LogInformation(
                    "Ticket {TicketNumber} auto-closed (resolved at {ResolvedAt})",
                    ticket.TicketNumber, ticket.ResolvedAt);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("AutoCloseJob finished at {Time}", DateTime.UtcNow);
        }
    }
}