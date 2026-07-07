using HelpDeskHQ.Core.DTOs.Admin;
using HelpDeskHQ.Core.Entities;
using HelpDeskHQ.Core.Interfaces;
using HelpDeskHQ.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskHQ.Infrastructure.Services
{
    public class TeamService : ITeamService
    {
        private readonly HelpDeskHQDbContext _context;

        public TeamService(HelpDeskHQDbContext context)
        {
            _context = context;
        }

        public async Task<List<TeamResponseDto>> GetAllAsync()
        {
            var teams = await _context.Teams
                .Include(t => t.Members)
                .Include(t => t.Categories)
                .ToListAsync();

            return teams.Select(t => new TeamResponseDto
            {
                Id = t.Id,
                Name = t.Name,
                MemberCount = t.Members.Count,
                CategoryCount = t.Categories.Count
            }).ToList();
        }

        public async Task<TeamResponseDto> CreateAsync(CreateTeamDto request)
        {
            var nameExists = await _context.Teams.AnyAsync(t => t.Name == request.Name);
            if (nameExists)
            {
                throw new InvalidOperationException("A team with this name already exists.");
            }

            var team = new Team { Name = request.Name };
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            return new TeamResponseDto
            {
                Id = team.Id,
                Name = team.Name,
                MemberCount = 0,
                CategoryCount = 0
            };
        }

        public async Task<TeamResponseDto> UpdateAsync(int teamId, CreateTeamDto request)
        {
            var team = await _context.Teams.FirstOrDefaultAsync(t => t.Id == teamId);
            if (team == null)
            {
                throw new InvalidOperationException("Team not found.");
            }

            team.Name = request.Name;
            await _context.SaveChangesAsync();

            return new TeamResponseDto
            {
                Id = team.Id,
                Name = team.Name,
                MemberCount = 0,
                CategoryCount = 0
            };
        }

        public async Task DeleteAsync(int teamId)
        {
            var team = await _context.Teams.FirstOrDefaultAsync(t => t.Id == teamId);
            if (team == null)
            {
                throw new InvalidOperationException("Team not found.");
            }

            var hasTickets = await _context.Tickets.AnyAsync(t => t.TeamId == teamId);
            if (hasTickets)
            {
                throw new InvalidOperationException("Cannot delete a team that has tickets assigned to it.");
            }

            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();
        }
    }
}