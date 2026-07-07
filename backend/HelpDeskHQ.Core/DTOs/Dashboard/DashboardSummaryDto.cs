namespace HelpDeskHQ.Core.DTOs.Dashboard
{
    public class DashboardSummaryDto
    {
        public int TotalOpenTickets { get; set; }
        public int TotalClosedTickets { get; set; }

        public Dictionary<string, int> TicketsByStatus { get; set; } = new();
        public Dictionary<string, int> TicketsByPriority { get; set; } = new();
        public Dictionary<string, int> TicketsByTeam { get; set; } = new();

        public int AtRiskCount { get; set; }
        public int BreachedCount { get; set; }

        public double SlaComplianceRatePercent { get; set; }
    }
}