using HelpDeskHQ.Core.Common.Exceptions;
using HelpDeskHQ.Core.DTOs.Admin;
using HelpDeskHQ.Core.Entities;
using HelpDeskHQ.Core.Interfaces;
using HelpDeskHQ.Infrastructure.Common;
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

            return teams.Select(MapToDto).ToList();
        }

        public async Task<TeamResponseDto> CreateAsync(CreateTeamDto request)
        {
            await EntityValidationHelper.ThrowIfExistsAsync(
                _context.Teams,
                t => t.Name == request.Name,
                "A team with this name already exists.");

            var team = new Team { Name = request.Name };
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            return MapToDto(team);
        }

        public async Task<TeamResponseDto> UpdateAsync(int teamId, CreateTeamDto request)
        {
            var team = await EntityValidationHelper.GetOrThrowAsync(
                _context.Teams,
                t => t.Id == teamId,
                "Team not found.");

            team.Name = request.Name;
            await _context.SaveChangesAsync();

            return MapToDto(team);
        }

        public async Task DeleteAsync(int teamId)
        {
            var team = await EntityValidationHelper.GetOrThrowAsync(
                _context.Teams,
                t => t.Id == teamId,
                "Team not found.");

            var hasTickets = await _context.Tickets.AnyAsync(t => t.TeamId == teamId);
            if (hasTickets)
            {
                throw new ValidationException("Cannot delete a team that has tickets assigned to it.");
            }

            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();
        }

        private static TeamResponseDto MapToDto(Team team)
        {
            return new TeamResponseDto
            {
                Id = team.Id,
                Name = team.Name,
                MemberCount = team.Members?.Count ?? 0,
                CategoryCount = team.Categories?.Count ?? 0
            };
        }
    }
}