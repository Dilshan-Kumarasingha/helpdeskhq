using HelpDeskHQ.Core.DTOs.Admin;

namespace HelpDeskHQ.Core.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryResponseDto>> GetAllAsync();
        Task<CategoryResponseDto> CreateAsync(CreateCategoryDto request);
        Task<CategoryResponseDto> UpdateAsync(int categoryId, CreateCategoryDto request);
        Task DeleteAsync(int categoryId);
    }
}