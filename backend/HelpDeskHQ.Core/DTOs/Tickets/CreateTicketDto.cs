namespace HelpDeskHQ.Core.DTOs.Tickets
{
    public class CreateTicketDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TicketCategoryId { get; set; }
        public int Priority { get; set; } // 0=Low, 1=Medium, 2=High, 3=Critical
    }
}