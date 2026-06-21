using HelpDeskHQ.Core.Enums;

namespace HelpDeskHQ.Core.Entities
{
    public class TicketStatusHistory
    {
        public int Id { get; set; }

        public int TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;

        public TicketStatus FromStatus { get; set; }
        public TicketStatus ToStatus { get; set; }

        public int ChangedByUserId { get; set; }
        public User ChangedByUser { get; set; } = null!;

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        public string? Note { get; set; }
    }
}