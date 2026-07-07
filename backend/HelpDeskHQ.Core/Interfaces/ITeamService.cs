using HelpDeskHQ.Core.DTOs.Admin;

namespace HelpDeskHQ.Core.Interfaces
{
    public interface ITeamService
    {
        Task<List<TeamResponseDto>> GetAllAsync();
        Task<TeamResponseDto> CreateAsync(CreateTeamDto request);
        Task<TeamResponseDto> UpdateAsync(int teamId, CreateTeamDto request);
        Task DeleteAsync(int teamId);
    }
}