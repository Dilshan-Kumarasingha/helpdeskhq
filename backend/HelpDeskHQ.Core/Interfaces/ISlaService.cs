using HelpDeskHQ.Core.Enums;

namespace HelpDeskHQ.Core.Interfaces
{
    public interface ISlaService
    {
        Task<(DateTime FirstResponseDueAt, DateTime ResolutionDueAt)> CalculateDueDatesAsync(
            int ticketCategoryId, TicketPriority priority, DateTime createdAt);
    }
}