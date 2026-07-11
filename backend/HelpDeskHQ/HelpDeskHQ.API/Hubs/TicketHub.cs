using HelpDeskHQ.API.Common;
using HelpDeskHQ.Core.Enums;
using HelpDeskHQ.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskHQ.API.Hubs
{
    [Authorize]
    public class TicketHub : Hub
    {
        private readonly HelpDeskHQDbContext _context;

        public TicketHub(HelpDeskHQDbContext context)
        {
            _context = context;
        }

        // Clients call this to join a "room" for a specific ticket,
        // so they only receive updates relevant to that ticket.
        public async Task JoinTicketGroup(int ticketId)
        {
            var userId = Context.User!.GetUserId();
            var role = Context.User!.GetUserRole();

            var hasAccess = await UserCanAccessTicketAsync(ticketId, userId, role);
            if (!hasAccess)
            {
                throw new HubException("You do not have access to this ticket.");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"ticket-{ticketId}");
        }

        public async Task LeaveTicketGroup(int ticketId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"ticket-{ticketId}");
        }

        private async Task<bool> UserCanAccessTicketAsync(int ticketId, int userId, string role)
        {
            // Staff roles can view any ticket.
            if (role is nameof(UserRole.SupportAgent) or nameof(UserRole.TeamLead) or nameof(UserRole.Admin))
            {
                return await _context.Tickets.AnyAsync(t => t.Id == ticketId);
            }

            // Employees can only join the group for tickets they raised themselves.
            return await _context.Tickets.AnyAsync(t => t.Id == ticketId && t.RaisedByUserId == userId);
        }
    }
}