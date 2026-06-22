using HelpDeskHQ.Core.DTOs.Tickets;

namespace HelpDeskHQ.Core.Interfaces
{
    public interface ITicketService
    {
        Task<TicketResponseDto> CreateTicketAsync(CreateTicketDto request, int raisedByUserId);
        Task<TicketResponseDto?> GetTicketByIdAsync(int ticketId);
        Task<List<TicketResponseDto>> GetTicketsAsync(int requestingUserId, string requestingUserRole);
    }
}