using HelpDeskHQ.API.Common;
using HelpDeskHQ.Core.DTOs.Tickets;
using HelpDeskHQ.Core.Enums;
using HelpDeskHQ.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskHQ.API.Controllers
{
    [ApiController]
    [Route("api/tickets")]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;

        public TicketsController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        // POST /api/tickets — Any authenticated user can raise a ticket
        [HttpPost]
        public async Task<IActionResult> CreateTicket([FromBody] CreateTicketDto request)
        {
            var userId = User.GetUserId();
            var result = await _ticketService.CreateTicketAsync(request, userId);
            return Ok(result);
        }

        // GET /api/tickets — List tickets (role-aware filtering happens in the service)
        [HttpGet]
        public async Task<IActionResult> GetTickets()
        {
            var userId = User.GetUserId();
            var role = User.GetUserRole();

            var tickets = await _ticketService.GetTicketsAsync(userId, role);
            return Ok(tickets);
        }

        // GET /api/tickets/{id} — Full ticket detail
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicketById(int id)
        {
            var ticket = await _ticketService.GetTicketByIdAsync(id);
            if (ticket == null)
                return NotFound(new { message = "Ticket not found." });

            return Ok(ticket);
        }

        // PATCH /api/tickets/{id}/assign — Only agents/leads/admins can assign
        [HttpPatch("{id}/assign")]
        [Authorize(Roles = "SupportAgent,TeamLead,Admin")]
        public async Task<IActionResult> AssignTicket(int id, [FromBody] AssignTicketDto request)
        {
            var changedByUserId = User.GetUserId();
            var result = await _ticketService.AssignTicketAsync(id, request.AgentUserId, changedByUserId);
            return Ok(result);
        }

        // PATCH /api/tickets/{id}/status — Only agents/leads/admins can change status directly
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "SupportAgent,TeamLead,Admin")]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeStatusDto request)
        {
            var changedByUserId = User.GetUserId();
            var result = await _ticketService.ChangeStatusAsync(id, request.NewStatus, changedByUserId, request.Note);
            return Ok(result);
        }

        // PATCH /api/tickets/{id}/resolve — Only agents/leads/admins can resolve
        [HttpPatch("{id}/resolve")]
        [Authorize(Roles = "SupportAgent,TeamLead,Admin")]
        public async Task<IActionResult> ResolveTicket(int id, [FromBody] ResolveTicketDto request)
        {
            var changedByUserId = User.GetUserId();
            var result = await _ticketService.ResolveTicketAsync(id, request.ResolutionNotes, changedByUserId);
            return Ok(result);
        }

        // PATCH /api/tickets/{id}/confirm — Employee confirms resolution → Closed
        [HttpPatch("{id}/confirm")]
        public async Task<IActionResult> ConfirmResolution(int id)
        {
            var changedByUserId = User.GetUserId();
            var result = await _ticketService.ChangeStatusAsync(
                id, (int)TicketStatus.Closed, changedByUserId, "Employee confirmed resolution");
            return Ok(result);
        }

        // PATCH /api/tickets/{id}/reopen — Employee rejects resolution → Reopened
        [HttpPatch("{id}/reopen")]
        public async Task<IActionResult> ReopenTicket(int id)
        {
            var changedByUserId = User.GetUserId();
            var result = await _ticketService.ChangeStatusAsync(
                id, (int)TicketStatus.Reopened, changedByUserId, "Employee rejected resolution — ticket reopened");
            return Ok(result);
        }

        // POST /api/tickets/{id}/comments — Add a comment
        [HttpPost("{id}/comments")]
        public async Task<IActionResult> AddComment(int id, [FromBody] AddCommentDto request)
        {
            var authorUserId = User.GetUserId();
            var result = await _ticketService.AddCommentAsync(id, authorUserId, request.Content);
            return Ok(result);
        }

        // GET /api/tickets/{id}/comments — List comments
        [HttpGet("{id}/comments")]
        public async Task<IActionResult> GetComments(int id)
        {
            var result = await _ticketService.GetCommentsAsync(id);
            return Ok(result);
        }
    }
}