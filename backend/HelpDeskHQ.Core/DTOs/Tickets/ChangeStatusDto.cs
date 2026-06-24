namespace HelpDeskHQ.Core.DTOs.Tickets
{
    public class ChangeStatusDto
    {
        public int NewStatus { get; set; } // matches TicketStatus enum int value
        public string? Note { get; set; }
    }
}