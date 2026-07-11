using HelpDeskHQ.Core.Entities;
using HelpDeskHQ.Core.Enums;
using HelpDeskHQ.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskHQ.API.Jobs
{
    public class SlaEscalationJob
    {
        private readonly HelpDeskHQDbContext _context;
        private readonly ILogger<SlaEscalationJob> _logger;

        private static readonly TicketStatus[] OpenStatuses =
        {
            TicketStatus.New,
            TicketStatus.Assigned,
            TicketStatus.InProgress,
            TicketStatus.OnHold,
            TicketStatus.Reopened,
            TicketStatus.Escalated
        };

        public SlaEscalationJob(HelpDeskHQDbContext context, ILogger<SlaEscalationJob> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            _logger.LogInformation("SlaEscalationJob started at {Time}", DateTime.UtcNow);

            var tickets = await _context.Tickets
                .Include(t => t.Team)
                    .ThenInclude(team => team.Members)
                .Where(t => OpenStatuses.Contains(t.Status))
                .ToListAsync();

            _logger.LogInformation("Checking {Count} open tickets for SLA breaches", tickets.Count);

            foreach (var ticket in tickets)
            {
                try
                {
                    EvaluateTicket(ticket);
                }
                catch (Exception ex)
                {
                    // One bad ticket should never stop the whole batch from being evaluated.
                    _logger.LogError(ex, "Failed to evaluate SLA for ticket {TicketId}", ticket.Id);
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("SlaEscalationJob finished at {Time}", DateTime.UtcNow);
        }

        private void EvaluateTicket(Ticket ticket)
        {
            var now = DateTime.UtcNow;

            // How much of the ticket's SLA clock has actually elapsed, excluding
            // any time spent OnHold (that time doesn't count against the SLA).
            var onHoldMinutesSoFar = ticket.TotalOnHoldMinutes;
            if (ticket.OnHoldSince != null)
            {
                onHoldMinutesSoFar += (int)(now - ticket.OnHoldSince.Value).TotalMinutes;
            }

            var rawElapsedMinutes = (now - ticket.CreatedAt).TotalMinutes;
            var effectiveElapsedMinutes = rawElapsedMinutes - onHoldMinutesSoFar;

            // --- Determine which clock to check ---
            // Not yet responded to: check against the response target.
            // Already responded to: check against the resolution target.
            bool checkingResponseClock = ticket.FirstRespondedAt == null;

            var targetMinutes = checkingResponseClock
                ? (ticket.FirstResponseDueAt - ticket.CreatedAt).TotalMinutes
                : (ticket.ResolutionDueAt - ticket.CreatedAt).TotalMinutes;

            if (targetMinutes <= 0) return;

            var percentElapsed = effectiveElapsedMinutes / targetMinutes;

            // --- AtRisk: 80% of time elapsed, not yet breached ---
            if (percentElapsed >= 0.8 && ticket.SlaBreachStatus == SlaBreachStatus.OnTrack)
            {
                ticket.SlaBreachStatus = SlaBreachStatus.AtRisk;

                _logger.LogWarning(
                    "Ticket {TicketNumber} is AtRisk ({Percent:P0} elapsed)",
                    ticket.TicketNumber, percentElapsed);

                if (ticket.AssignedAgentId != null)
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserId = ticket.AssignedAgentId.Value,
                        TicketId = ticket.Id,
                        Message = $"Ticket {ticket.TicketNumber} is at risk of breaching its SLA.",
                        CreatedAt = now
                    });
                }
            }

            // --- Breached: 100% elapsed, not yet marked Breached ---
            if (percentElapsed >= 1.0 && ticket.SlaBreachStatus != SlaBreachStatus.Breached)
            {
                var oldStatus = ticket.Status;

                ticket.SlaBreachStatus = SlaBreachStatus.Breached;
                ticket.EscalationLevel += 1;
                ticket.Status = TicketStatus.Escalated;

                _logger.LogError(
                    "Ticket {TicketNumber} has BREACHED SLA (escalation level {Level})",
                    ticket.TicketNumber, ticket.EscalationLevel);

                var teamLead = ticket.Team.Members.FirstOrDefault(m => m.IsTeamLead);

                if (teamLead != null)
                {
                    ticket.AssignedAgentId = teamLead.UserId;

                    _context.Notifications.Add(new Notification
                    {
                        UserId = teamLead.UserId,
                        TicketId = ticket.Id,
                        Message = $"Ticket {ticket.TicketNumber} has breached its SLA and been escalated to you.",
                        CreatedAt = now
                    });
                }

                _context.TicketEscalations.Add(new TicketEscalation
                {
                    TicketId = ticket.Id,
                    EscalationLevel = ticket.EscalationLevel,
                    EscalatedAt = now,
                    EscalatedToUserId = teamLead?.UserId,
                    Reason = "SLA breach detected by automated escalation job"
                });

                _context.TicketStatusHistories.Add(new TicketStatusHistory
                {
                    TicketId = ticket.Id,
                    FromStatus = oldStatus,
                    ToStatus = TicketStatus.Escalated,
                    ChangedByUserId = ticket.AssignedAgentId ?? ticket.RaisedByUserId,
                    ChangedAt = now,
                    Note = $"Auto-escalated by SLA engine (level {ticket.EscalationLevel})"
                });
            }
        }
    }
}