using System.Net.Sockets;

namespace HelpDeskHQ.Core.Entities
{
    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();
        public ICollection<TicketCategory> Categories { get; set; } = new List<TicketCategory>();
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}