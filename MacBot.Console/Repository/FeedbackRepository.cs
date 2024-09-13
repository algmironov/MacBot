using MacBot.ConsoleApp.Models;
using MacBot.ConsoleApp.Repository.Database;

using Microsoft.EntityFrameworkCore;

namespace MacBot.ConsoleApp.Repository
{
    public class FeedbackRepository(BotDbContext context) : IFeedbackRepository
    {
        private readonly BotDbContext _context = context;

        public async Task DeleteAsync(Feedback feedback)
        {
            _context.Feedbacks.Remove(feedback);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAllAsync(Guid chatId)
        {
            var feedbacksFromUser = _context.Feedbacks.Where(fb => fb.Id == chatId).ToList();
            _context.Feedbacks.RemoveRange(feedbacksFromUser);
            await _context.SaveChangesAsync();
        }

        public async Task<Feedback> GetAsync(Guid id)
        {
            return await _context.Feedbacks.FirstAsync(fb => fb.Id == id);
        }

        public async Task<IEnumerable<Feedback>> GetAllAsync()
        {
            return await _context.Feedbacks.ToListAsync();
        }

        public async Task<IEnumerable<Feedback>> GetAllAsync(long chatId)
        {
            var feedbacks = _context.Feedbacks.Where(fb => fb.ChatId == chatId);
            return await feedbacks.ToListAsync();
        }

        public async Task<IEnumerable<Feedback>> GetUnreadAsync()
        {
            return await _context.Feedbacks.Where(fb => !fb.Read).ToListAsync();
        }

        public async Task SaveAsync(Feedback feedback)
        {
            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Feedback feedback)
        {
            _context.Feedbacks.Update(feedback);
            await _context.SaveChangesAsync();
        }
    }
}
