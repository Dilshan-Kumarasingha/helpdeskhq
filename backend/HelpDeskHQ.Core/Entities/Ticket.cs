using HelpDeskHQ.Core.Enums;

namespace HelpDeskHQ.Core.Entities
{
    public class Ticket
    {
        public int Id { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public int TicketCategoryId { get; set; }
        public TicketCategory TicketCategory { get; set; } = null!;

        public TicketPriority Priority { get; set; }
        public TicketStatus Status { get; set; } = TicketStatus.New;

        public int RaisedByUserId { get; set; }
        public User RaisedByUser { get; set; } = null!;

        public int? AssignedAgentId { get; set; }
        public User? AssignedAgent { get; set; }

        public int TeamId { get; set; }
        public Team Team { get; set; } = null!;

        // SLA clock fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime FirstResponseDueAt { get; set; }
        public DateTime ResolutionDueAt { get; set; }
        public DateTime? FirstRespondedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }

        public DateTime? OnHoldSince { get; set; }
        public int TotalOnHoldMinutes { get; set; } = 0;

        public SlaBreachStatus SlaBreachStatus { get; set; } = SlaBreachStatus.OnTrack;
        public int EscalationLevel { get; set; } = 0;

        public string? ResolutionNotes { get; set; }
        public int? EmployeeRating { get; set; }

        // Navigation properties
        public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
        public ICollection<TicketAttachment> Attachments { get; set; } = new List<TicketAttachment>();
        public ICollection<TicketStatusHistory> StatusHistory { get; set; } = new List<TicketStatusHistory>();
        public ICollection<TicketEscalation> Escalations { get; set; } = new List<TicketEscalation>();
    }
}