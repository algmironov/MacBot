using MacBot.ConsoleApp.Models;

namespace MacBot.ConsoleApp.Repository
{
    public interface IFeedbackRepository
    {
        Task SaveAsync(Feedback feedback);
        Task DeleteAsync(Feedback feedback);
        Task<Feedback> GetAsync(Guid id);
        Task UpdateAsync(Feedback feedback);
        Task<IEnumerable<Feedback>> GetAllAsync();
        Task<IEnumerable<Feedback>> GetAllAsync(long chatId);
        Task DeleteAllAsync(Guid chatId);
        Task<IEnumerable<Feedback>> GetUnreadAsync();
    }
}
