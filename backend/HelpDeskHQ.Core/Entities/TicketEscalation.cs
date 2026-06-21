namespace HelpDeskHQ.Core.Entities
{
    public class TicketEscalation
    {
        public int Id { get; set; }

        public int TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;

        public int EscalationLevel { get; set; }
        public DateTime EscalatedAt { get; set; } = DateTime.UtcNow;

        public int? EscalatedToUserId { get; set; }
        public User? EscalatedToUser { get; set; }

        public string Reason { get; set; } = string.Empty;
    }
}