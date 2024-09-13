using MacBot.ConsoleApp.Models;

namespace MacBot.ConsoleApp.Repository
{
    public interface IBotMessagesRepository
    {
        Task AddAsync(BotMessage botMessage);
        Task UpdateAsync(BotMessage botMessage);
        Task DeleteAsync(BotMessage botMessage);
        Task<IEnumerable<BotMessage>> GetAllAsync();
        Task<IEnumerable<BotMessage>> GetAllByChatId(long chatId);
        Task<IEnumerable<BotMessage>> GetAllShownByChatId(long chatId);
        Task AddOrUpdateAsync(BotMessage botMessage);
        Task<BotMessage> GetLast(long chatId);
        Task ClearDeletedMesages(long chatId);
    }
}
