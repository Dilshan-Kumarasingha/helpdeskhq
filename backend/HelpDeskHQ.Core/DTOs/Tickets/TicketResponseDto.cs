namespace HelpDeskHQ.Core.DTOs.Tickets
{
    public class TicketResponseDto
    {
        public int Id { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public string RaisedByName { get; set; } = string.Empty;
        public string? AssignedAgentName { get; set; }
        public string TeamName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime FirstResponseDueAt { get; set; }
        public DateTime ResolutionDueAt { get; set; }
        public DateTime? FirstRespondedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }

        public string SlaBreachStatus { get; set; } = string.Empty;
        public int EscalationLevel { get; set; }
    }
}