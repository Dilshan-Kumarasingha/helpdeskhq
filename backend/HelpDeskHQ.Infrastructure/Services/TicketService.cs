using HelpDeskHQ.Core.DTOs.Tickets;
using HelpDeskHQ.Core.Entities;
using HelpDeskHQ.Core.Enums;
using HelpDeskHQ.Core.Interfaces;
using HelpDeskHQ.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskHQ.Infrastructure.Services
{
    public class TicketService : ITicketService
    {
        private readonly HelpDeskHQDbContext _context;
        private readonly ISlaService _slaService;

        public TicketService(HelpDeskHQDbContext context, ISlaService slaService)
        {
            _context = context;
            _slaService = slaService;
        }

        public async Task<TicketResponseDto> CreateTicketAsync(CreateTicketDto request, int raisedByUserId)
        {
            var category = await _context.TicketCategories
                .FirstOrDefaultAsync(c => c.Id == request.TicketCategoryId);

            if (category == null)
            {
                throw new InvalidOperationException("Invalid ticket category.");
            }

            var priority = (TicketPriority)request.Priority;
            var createdAt = DateTime.UtcNow;

            var (firstResponseDueAt, resolutionDueAt) = await _slaService.CalculateDueDatesAsync(
                category.Id, priority, createdAt);

            var ticketNumber = await GenerateTicketNumberAsync();

            var ticket = new Ticket
            {
                TicketNumber = ticketNumber,
                Title = request.Title,
                Description = request.Description,
                TicketCategoryId = category.Id,
                Priority = priority,
                Status = TicketStatus.New,
                RaisedByUserId = raisedByUserId,
                TeamId = category.TeamId,
                CreatedAt = createdAt,
                FirstResponseDueAt = firstResponseDueAt,
                ResolutionDueAt = resolutionDueAt,
                SlaBreachStatus = SlaBreachStatus.OnTrack,
                EscalationLevel = 0
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            // Record the initial status in the audit history
            _context.TicketStatusHistories.Add(new TicketStatusHistory
            {
                TicketId = ticket.Id,
                FromStatus = TicketStatus.New,
                ToStatus = TicketStatus.New,
                ChangedByUserId = raisedByUserId,
                ChangedAt = createdAt,
                Note = "Ticket created"
            });
            await _context.SaveChangesAsync();

            return await MapToResponseDto(ticket.Id);
        }

        private async Task<string> GenerateTicketNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var countThisYear = await _context.Tickets
                .CountAsync(t => t.CreatedAt.Year == year);

            var nextNumber = countThisYear + 1;
            return $"HD-{year}-{nextNumber:D5}";
        }

        private async Task<TicketResponseDto> MapToResponseDto(int ticketId)
        {
            var ticket = await _context.Tickets
                .Include(t => t.TicketCategory)
                .Include(t => t.RaisedByUser)
                .Include(t => t.AssignedAgent)
                .Include(t => t.Team)
                .FirstAsync(t => t.Id == ticketId);

            return new TicketResponseDto
            {
                Id = ticket.Id,
                TicketNumber = ticket.TicketNumber,
                Title = ticket.Title,
                Description = ticket.Description,
                Category = ticket.TicketCategory.Name,
                Priority = ticket.Priority.ToString(),
                Status = ticket.Status.ToString(),
                RaisedByName = ticket.RaisedByUser.FullName,
                AssignedAgentName = ticket.AssignedAgent?.FullName,
                TeamName = ticket.Team.Name,
                CreatedAt = ticket.CreatedAt,
                FirstResponseDueAt = ticket.FirstResponseDueAt,
                ResolutionDueAt = ticket.ResolutionDueAt,
                FirstRespondedAt = ticket.FirstRespondedAt,
                ResolvedAt = ticket.ResolvedAt,
                SlaBreachStatus = ticket.SlaBreachStatus.ToString(),
                EscalationLevel = ticket.EscalationLevel
            };
        }

        public async Task<TicketResponseDto?> GetTicketByIdAsync(int ticketId)
        {
            var exists = await _context.Tickets.AnyAsync(t => t.Id == ticketId);
            if (!exists) return null;

            return await MapToResponseDto(ticketId);
        }

        public async Task<List<TicketResponseDto>> GetTicketsAsync(int requestingUserId, string requestingUserRole)
        {
            IQueryable<Ticket> query = _context.Tickets
                .Include(t => t.TicketCategory)
                .Include(t => t.RaisedByUser)
                .Include(t => t.AssignedAgent)
                .Include(t => t.Team);

            // Employees only see their own tickets.
            // Agents, Team Leads, and Admins see everything for now —
            // team-scoped filtering for Agents/Leads will be refined later.
            if (requestingUserRole == "Employee")
            {
                query = query.Where(t => t.RaisedByUserId == requestingUserId);
            }

            var tickets = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();

            return tickets.Select(ticket => new TicketResponseDto
            {
                Id = ticket.Id,
                TicketNumber = ticket.TicketNumber,
                Title = ticket.Title,
                Description = ticket.Description,
                Category = ticket.TicketCategory.Name,
                Priority = ticket.Priority.ToString(),
                Status = ticket.Status.ToString(),
                RaisedByName = ticket.RaisedByUser.FullName,
                AssignedAgentName = ticket.AssignedAgent?.FullName,
                TeamName = ticket.Team.Name,
                CreatedAt = ticket.CreatedAt,
                FirstResponseDueAt = ticket.FirstResponseDueAt,
                ResolutionDueAt = ticket.ResolutionDueAt,
                FirstRespondedAt = ticket.FirstRespondedAt,
                ResolvedAt = ticket.ResolvedAt,
                SlaBreachStatus = ticket.SlaBreachStatus.ToString(),
                EscalationLevel = ticket.EscalationLevel
            }).ToList();
        }

        public async Task<TicketResponseDto> AssignTicketAsync(int ticketId, int agentUserId, int changedByUserId)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);
            if (ticket == null)
            {
                throw new InvalidOperationException("Ticket not found.");
            }

            var agentExists = await _context.Users.AnyAsync(u => u.Id == agentUserId);
            if (!agentExists)
            {
                throw new InvalidOperationException("Assigned agent not found.");
            }

            var oldStatus = ticket.Status;
            ticket.AssignedAgentId = agentUserId;

            // Assigning a ticket moves it from New to Assigned, if it's still New.
            if (ticket.Status == TicketStatus.New)
            {
                ValidateTransition(ticket.Status, TicketStatus.Assigned);
                ticket.Status = TicketStatus.Assigned;
            }

            await _context.SaveChangesAsync();

            await AddStatusHistoryAsync(ticket.Id, oldStatus, ticket.Status, changedByUserId, "Agent assigned");

            return await MapToResponseDto(ticket.Id);
        }

        public async Task<TicketResponseDto> ChangeStatusAsync(int ticketId, int newStatus, int changedByUserId, string? note)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);
            if (ticket == null)
            {
                throw new InvalidOperationException("Ticket not found.");
            }

            var targetStatus = (TicketStatus)newStatus;
            ValidateTransition(ticket.Status, targetStatus);

            var oldStatus = ticket.Status;

            // Stop the "first response" SLA clock the first time the ticket becomes InProgress
            if (targetStatus == TicketStatus.InProgress && ticket.FirstRespondedAt == null)
            {
                ticket.FirstRespondedAt = DateTime.UtcNow;
            }

            // Track OnHold periods so resolution-clock math can exclude paused time later
            if (targetStatus == TicketStatus.OnHold)
            {
                ticket.OnHoldSince = DateTime.UtcNow;
            }
            else if (oldStatus == TicketStatus.OnHold && ticket.OnHoldSince != null)
            {
                var pausedMinutes = (int)(DateTime.UtcNow - ticket.OnHoldSince.Value).TotalMinutes;
                ticket.TotalOnHoldMinutes += pausedMinutes;
                ticket.OnHoldSince = null;
            }

            ticket.Status = targetStatus;
            await _context.SaveChangesAsync();

            await AddStatusHistoryAsync(ticket.Id, oldStatus, targetStatus, changedByUserId, note);

            return await MapToResponseDto(ticket.Id);
        }

        public async Task<TicketResponseDto> ResolveTicketAsync(int ticketId, string resolutionNotes, int changedByUserId)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);
            if (ticket == null)
            {
                throw new InvalidOperationException("Ticket not found.");
            }

            ValidateTransition(ticket.Status, TicketStatus.Resolved);

            var oldStatus = ticket.Status;
            ticket.Status = TicketStatus.Resolved;
            ticket.ResolutionNotes = resolutionNotes;
            ticket.ResolvedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await AddStatusHistoryAsync(ticket.Id, oldStatus, TicketStatus.Resolved, changedByUserId, "Resolved");

            return await MapToResponseDto(ticket.Id);
        }

        private static readonly Dictionary<TicketStatus, TicketStatus[]> ValidTransitions = new()
        {
            [TicketStatus.New] = new[] { TicketStatus.Assigned, TicketStatus.Escalated },
            [TicketStatus.Assigned] = new[] { TicketStatus.InProgress, TicketStatus.Escalated },
            [TicketStatus.InProgress] = new[] { TicketStatus.OnHold, TicketStatus.Resolved, TicketStatus.Escalated },
            [TicketStatus.OnHold] = new[] { TicketStatus.InProgress },
            [TicketStatus.Resolved] = new[] { TicketStatus.Closed, TicketStatus.Reopened },
            [TicketStatus.Reopened] = new[] { TicketStatus.InProgress },
            [TicketStatus.Escalated] = new[] { TicketStatus.InProgress },
            [TicketStatus.Closed] = Array.Empty<TicketStatus>()
        };

        private static void ValidateTransition(TicketStatus from, TicketStatus to)
        {
            if (from == to) return; // allow no-op, e.g. re-saving the same status

            if (!ValidTransitions.TryGetValue(from, out var allowed) || !allowed.Contains(to))
            {
                throw new InvalidOperationException(
                    $"Invalid status transition: cannot move from {from} to {to}.");
            }
        }

        private async Task AddStatusHistoryAsync(int ticketId, TicketStatus from, TicketStatus to, int changedByUserId, string? note)
        {
            _context.TicketStatusHistories.Add(new TicketStatusHistory
            {
                TicketId = ticketId,
                FromStatus = from,
                ToStatus = to,
                ChangedByUserId = changedByUserId,
                ChangedAt = DateTime.UtcNow,
                Note = note
            });
            await _context.SaveChangesAsync();
        }
    }
}