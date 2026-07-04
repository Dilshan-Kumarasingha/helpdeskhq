using HelpDeskHQ.Core.DTOs.Dashboard;
using HelpDeskHQ.Core.Enums;
using HelpDeskHQ.Core.Interfaces;
using HelpDeskHQ.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskHQ.Infrastructure.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly HelpDeskHQDbContext _context;

        public DashboardService(HelpDeskHQDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardSummaryDto> GetSummaryAsync()
        {
            var closedStatuses = new[] { TicketStatus.Closed };

            var allTickets = await _context.Tickets
                .Include(t => t.Team)
                .ToListAsync();

            var openTickets = allTickets.Where(t => !closedStatuses.Contains(t.Status)).ToList();
            var closedTickets = allTickets.Where(t => closedStatuses.Contains(t.Status)).ToList();

            var ticketsByStatus = allTickets
                .GroupBy(t => t.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            var ticketsByPriority = allTickets
                .GroupBy(t => t.Priority.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            var ticketsByTeam = allTickets
                .GroupBy(t => t.Team.Name)
                .ToDictionary(g => g.Key, g => g.Count());

            var atRiskCount = allTickets.Count(t => t.SlaBreachStatus == SlaBreachStatus.AtRisk);
            var breachedCount = allTickets.Count(t => t.SlaBreachStatus == SlaBreachStatus.Breached);

            // SLA compliance rate: rolling 30-day window, based on tickets created in the last 30 days
            var cutoff = DateTime.UtcNow.AddDays(-30);
            var recentTickets = allTickets.Where(t => t.CreatedAt >= cutoff).ToList();

            double complianceRate;
            if (recentTickets.Count == 0)
            {
                complianceRate = 100.0; // no tickets = nothing breached, treat as fully compliant
            }
            else
            {
                var compliantCount = recentTickets.Count(t => t.SlaBreachStatus != SlaBreachStatus.Breached);
                complianceRate = Math.Round((double)compliantCount / recentTickets.Count * 100, 2);
            }

            return new DashboardSummaryDto
            {
                TotalOpenTickets = openTickets.Count,
                TotalClosedTickets = closedTickets.Count,
                TicketsByStatus = ticketsByStatus,
                TicketsByPriority = ticketsByPriority,
                TicketsByTeam = ticketsByTeam,
                AtRiskCount = atRiskCount,
                BreachedCount = breachedCount,
                SlaComplianceRatePercent = complianceRate
            };
        }
    }
}