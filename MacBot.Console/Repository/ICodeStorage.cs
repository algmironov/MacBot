using MacBot.ConsoleApp.Models;

namespace MacBot.ConsoleApp.Repository
{
    public interface ICodeStorage
    {
        Task<IEnumerable<InviteCode>> GetAllAsync();
        Task<InviteCode> GetByIdAsync(Guid id);
        Task AddAsync(InviteCode inviteCode);
        Task UpdateAsync(InviteCode inviteCode);
        Task DeleteAsync(Guid id);
        Task<Guid> GetCreatorAsync(string code);
    }
}
