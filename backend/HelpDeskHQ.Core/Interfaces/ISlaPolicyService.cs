using HelpDeskHQ.Core.DTOs.Admin;

namespace HelpDeskHQ.Core.Interfaces
{
    public interface ISlaPolicyService
    {
        Task<List<SlaPolicyResponseDto>> GetAllAsync();
        Task<SlaPolicyResponseDto> CreateAsync(CreateSlaPolicyDto request);
        Task<SlaPolicyResponseDto> UpdateAsync(int policyId, CreateSlaPolicyDto request);
        Task DeleteAsync(int policyId);
    }
}