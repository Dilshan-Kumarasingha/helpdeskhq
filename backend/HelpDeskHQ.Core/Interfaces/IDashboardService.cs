using HelpDeskHQ.Core.DTOs.Dashboard;

namespace HelpDeskHQ.Core.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetSummaryAsync();
    }
}