using HelpDeskHQ.Core.DTOs.Tickets;

namespace HelpDeskHQ.Core.Interfaces
{
    public interface ITicketService
    {
        Task<TicketResponseDto> CreateTicketAsync(CreateTicketDto request, int raisedByUserId);
        Task<TicketResponseDto?> GetTicketByIdAsync(int ticketId);
        Task<List<TicketResponseDto>> GetTicketsAsync(int requestingUserId, string requestingUserRole);
        Task<TicketResponseDto> AssignTicketAsync(int ticketId, int agentUserId, int changedByUserId);
        Task<TicketResponseDto> ChangeStatusAsync(int ticketId, int newStatus, int changedByUserId, string? note);
        Task<TicketResponseDto> ResolveTicketAsync(int ticketId, string resolutionNotes, int changedByUserId);
    }
}