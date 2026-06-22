using HelpDeskHQ.Core.Enums;
using HelpDeskHQ.Core.Interfaces;
using HelpDeskHQ.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskHQ.Infrastructure.Services
{
    public class SlaService : ISlaService
    {
        private readonly HelpDeskHQDbContext _context;

        public SlaService(HelpDeskHQDbContext context)
        {
            _context = context;
        }

        public async Task<(DateTime FirstResponseDueAt, DateTime ResolutionDueAt)> CalculateDueDatesAsync(
            int ticketCategoryId, TicketPriority priority, DateTime createdAt)
        {
            var policy = await _context.SlaPolicies
                .FirstOrDefaultAsync(p => p.TicketCategoryId == ticketCategoryId && p.Priority == priority);

            if (policy == null)
            {
                // Fallback: if no specific policy exists for this category+priority
                // combination, apply a safe default so the ticket still gets due dates.
                return (createdAt.AddHours(8), createdAt.AddHours(48));
            }

            var firstResponseDueAt = createdAt.AddMinutes(policy.ResponseTargetMinutes);
            var resolutionDueAt = createdAt.AddMinutes(policy.ResolutionTargetMinutes);

            return (firstResponseDueAt, resolutionDueAt);
        }
    }
}