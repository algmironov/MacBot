using MacBot.ConsoleApp.Models;

namespace MacBot.ConsoleApp.Repository
{
    public interface IUserRepository
    {
        Task<IEnumerable<BotUser>> GetAllAsync();
        Task<BotUser> GetByIdAsync(Guid id);
        Task AddAsync(BotUser botUser);
        Task UpdateAsync(BotUser botUser);
        Task DeleteAsync(Guid id);
        Task<BotUser> GetByChatIdAsync(long  chatId);
    }
}
