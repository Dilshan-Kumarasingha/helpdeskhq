using System.Security.Claims;
using HelpDeskHQ.Core.DTOs.Tickets;
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

        // POST /api/tickets — Employee creates a ticket
        [HttpPost]
        public async Task<IActionResult> CreateTicket([FromBody] CreateTicketDto request)
        {
            var userId = GetUserId();

            try
            {
                var result = await _ticketService.CreateTicketAsync(request, userId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/tickets — List tickets (role-aware)
        [HttpGet]
        public async Task<IActionResult> GetTickets()
        {
            var userId = GetUserId();
            var role = GetUserRole();

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

        // PATCH /api/tickets/{id}/assign — Assign an agent to a ticket
        [HttpPatch("{id}/assign")]
        public async Task<IActionResult> AssignTicket(int id, [FromBody] AssignTicketDto request)
        {
            var changedByUserId = GetUserId();

            try
            {
                var result = await _ticketService.AssignTicketAsync(id, request.AgentUserId, changedByUserId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PATCH /api/tickets/{id}/status — Change ticket status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeStatusDto request)
        {
            var changedByUserId = GetUserId();

            try
            {
                var result = await _ticketService.ChangeStatusAsync(id, request.NewStatus, changedByUserId, request.Note);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PATCH /api/tickets/{id}/resolve — Agent resolves a ticket
        [HttpPatch("{id}/resolve")]
        public async Task<IActionResult> ResolveTicket(int id, [FromBody] ResolveTicketDto request)
        {
            var changedByUserId = GetUserId();

            try
            {
                var result = await _ticketService.ResolveTicketAsync(id, request.ResolutionNotes, changedByUserId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PATCH /api/tickets/{id}/confirm — Employee confirms resolution → Closed
        [HttpPatch("{id}/confirm")]
        public async Task<IActionResult> ConfirmResolution(int id)
        {
            var changedByUserId = GetUserId();

            try
            {
                // Confirmed = move to Closed (status int 5)
                var result = await _ticketService.ChangeStatusAsync(id, (int)HelpDeskHQ.Core.Enums.TicketStatus.Closed, changedByUserId, "Employee confirmed resolution");
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PATCH /api/tickets/{id}/reopen — Employee rejects resolution → Reopened
        [HttpPatch("{id}/reopen")]
        public async Task<IActionResult> ReopenTicket(int id)
        {
            var changedByUserId = GetUserId();

            try
            {
                // Reopened = status int 6
                var result = await _ticketService.ChangeStatusAsync(id, (int)HelpDeskHQ.Core.Enums.TicketStatus.Reopened, changedByUserId, "Employee rejected resolution — ticket reopened");
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private int GetUserId()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(idClaim!);
        }

        private string GetUserRole()
        {
            return User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        }
    }
}