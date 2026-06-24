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

        public SlaEscalationJob(HelpDeskHQDbContext context, ILogger<SlaEscalationJob> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            _logger.LogInformation("SlaEscalationJob started at {Time}", DateTime.UtcNow);

            var openStatuses = new[]
            {
                TicketStatus.New,
                TicketStatus.Assigned,
                TicketStatus.InProgress,
                TicketStatus.OnHold,
                TicketStatus.Reopened,
                TicketStatus.Escalated
            };

            var tickets = await _context.Tickets
                .Include(t => t.Team)
                    .ThenInclude(team => team.Members)
                .Where(t => openStatuses.Contains(t.Status))
                .ToListAsync();

            _logger.LogInformation("Checking {Count} open tickets for SLA breaches", tickets.Count);

            foreach (var ticket in tickets)
            {
                await EvaluateTicketAsync(ticket);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("SlaEscalationJob finished at {Time}", DateTime.UtcNow);
        }

        private async Task EvaluateTicketAsync(Ticket ticket)
        {
            var now = DateTime.UtcNow;

            // --- Determine which clock to check ---
            // If the ticket has never been responded to, check the response clock.
            // Otherwise check the resolution clock (adjusted for OnHold time).
            DateTime dueAt;
            bool checkingResponseClock = ticket.FirstRespondedAt == null;

            if (checkingResponseClock)
            {
                dueAt = ticket.FirstResponseDueAt;
            }
            else
            {
                // Adjust resolution due date forward by any OnHold time accumulated
                var holdOffset = TimeSpan.FromMinutes(ticket.TotalOnHoldMinutes);

                // If currently on hold, add the time it has been on hold so far too
                if (ticket.OnHoldSince != null)
                {
                    holdOffset += now - ticket.OnHoldSince.Value;
                }

                dueAt = ticket.ResolutionDueAt + holdOffset;
            }

            var totalMinutes = (dueAt - ticket.CreatedAt).TotalMinutes;
            var elapsedMinutes = (now - ticket.CreatedAt).TotalMinutes;

            if (totalMinutes <= 0) return;

            var percentElapsed = elapsedMinutes / totalMinutes;

            // --- AtRisk: 80% of time elapsed, not yet breached ---
            if (percentElapsed >= 0.8 && ticket.SlaBreachStatus == SlaBreachStatus.OnTrack)
            {
                ticket.SlaBreachStatus = SlaBreachStatus.AtRisk;

                _logger.LogWarning(
                    "Ticket {TicketNumber} is AtRisk ({Percent:P0} elapsed)",
                    ticket.TicketNumber, percentElapsed);

                // Create an in-app notification for the assigned agent (if any)
                if (ticket.AssignedAgentId != null)
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserId = ticket.AssignedAgentId.Value,
                        TicketId = ticket.Id,
                        Message = $"⚠️ Ticket {ticket.TicketNumber} is at risk of breaching its SLA.",
                        CreatedAt = now
                    });
                }
            }

            // --- Breached: 100% elapsed, not yet marked Breached ---
            if (percentElapsed >= 1.0 && ticket.SlaBreachStatus != SlaBreachStatus.Breached)
            {
                ticket.SlaBreachStatus = SlaBreachStatus.Breached;
                ticket.EscalationLevel += 1;
                ticket.Status = TicketStatus.Escalated;

                _logger.LogError(
                    "Ticket {TicketNumber} has BREACHED SLA (escalation level {Level})",
                    ticket.TicketNumber, ticket.EscalationLevel);

                // Find the Team Lead for this ticket's team
                var teamLead = ticket.Team.Members
                    .FirstOrDefault(m => m.IsTeamLead);

                if (teamLead != null)
                {
                    ticket.AssignedAgentId = teamLead.UserId;

                    _context.Notifications.Add(new Notification
                    {
                        UserId = teamLead.UserId,
                        TicketId = ticket.Id,
                        Message = $"🚨 Ticket {ticket.TicketNumber} has breached its SLA and been escalated to you.",
                        CreatedAt = now
                    });
                }

                // Write an escalation audit record
                _context.TicketEscalations.Add(new TicketEscalation
                {
                    TicketId = ticket.Id,
                    EscalationLevel = ticket.EscalationLevel,
                    EscalatedAt = now,
                    EscalatedToUserId = teamLead?.UserId,
                    Reason = "SLA breach detected by automated escalation job"
                });

                // Write a status history entry
                _context.TicketStatusHistories.Add(new TicketStatusHistory
                {
                    TicketId = ticket.Id,
                    FromStatus = ticket.Status,
                    ToStatus = TicketStatus.Escalated,
                    ChangedByUserId = ticket.AssignedAgentId ?? ticket.RaisedByUserId,
                    ChangedAt = now,
                    Note = $"Auto-escalated by SLA engine (level {ticket.EscalationLevel})"
                });
            }
        }
    }
}