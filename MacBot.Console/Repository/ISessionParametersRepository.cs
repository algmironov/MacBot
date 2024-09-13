using MacBot.ConsoleApp.Models;

namespace MacBot.ConsoleApp.Repository
{
    public interface ISessionParametersRepository
    {
        Task<IEnumerable<SessionParameters>> GetAllAsync();
        Task<SessionParameters> GetByIdAsync(Guid id);
        Task AddAsync(SessionParameters sessionParameters);
        Task UpdateAsync(SessionParameters sessionParameters);
        Task DeleteAsync(Guid id);
        Task<SessionParameters> GetByMasterId(Guid masterId);
    }
}
