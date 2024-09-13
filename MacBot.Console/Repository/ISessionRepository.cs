using MacBot.ConsoleApp.Models;

namespace MacBot.ConsoleApp.Repository
{
    public interface ISessionRepository
    {
        Task<IEnumerable<Session>> GetAllAsync();
        Task<Session> GetByIdAsync(Guid id);
        Task AddAsync(Session session);
        Task UpdateAsync(Session session);
        Task DeleteAsync(Guid id);

        Task<IEnumerable<Session>> GetAllByMaster(BotUser master);
        Task<IEnumerable<Session>>  GetAllForExport(BotUser master);
        Task<IEnumerable<Session>> GetAllByMasterAndClient(BotUser master, BotUser client);
        Task<Session> GetActiveByUser(BotUser user);
        Task<Session> GetActiveSession(BotUser master);
    }
}
