namespace HelpDeskHQ.Core.Common.Interfaces
{
    /// <summary>
    /// Common shape for simple admin CRUD operations (Team, Category, SlaPolicy).
    /// TRequest = the create/update DTO, TResponse = the response DTO.
    /// </summary>
    public interface IAdminCrudService<TRequest, TResponse>
    {
        Task<List<TResponse>> GetAllAsync();
        Task<TResponse> CreateAsync(TRequest request);
        Task<TResponse> UpdateAsync(int id, TRequest request);
        Task DeleteAsync(int id);
    }
}