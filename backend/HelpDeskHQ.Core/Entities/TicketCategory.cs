using System.Net.Sockets;

namespace HelpDeskHQ.Core.Entities
{
    public class TicketCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public int TeamId { get; set; }
        public Team Team { get; set; } = null!;

        public ICollection<SlaPolicy> SlaPolicies { get; set; } = new List<SlaPolicy>();
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}