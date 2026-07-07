using HelpDeskHQ.Core.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace HelpDeskHQ.API.Hubs
{
    public class SignalRNotifier : IRealtimeNotifier
    {
        private readonly IHubContext<TicketHub> _hubContext;

        public SignalRNotifier(IHubContext<TicketHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyTicketUpdatedAsync(int ticketId, object payload)
        {
            await _hubContext.Clients.Group($"ticket-{ticketId}").SendAsync("TicketUpdated", payload);
        }
    }
}