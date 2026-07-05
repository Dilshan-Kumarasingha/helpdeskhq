using Microsoft.AspNetCore.SignalR;

namespace HelpDeskHQ.API.Hubs
{
    public class TicketHub : Hub
    {
        // Clients call this to join a "room" for a specific ticket,
        // so they only receive updates relevant to that ticket.
        public async Task JoinTicketGroup(int ticketId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"ticket-{ticketId}");
        }

        public async Task LeaveTicketGroup(int ticketId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"ticket-{ticketId}");
        }
    }
}