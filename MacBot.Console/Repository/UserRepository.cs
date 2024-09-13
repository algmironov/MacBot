using MacBot.ConsoleApp.Models;
using MacBot.ConsoleApp.Repository.Database;

using Microsoft.EntityFrameworkCore;

namespace MacBot.ConsoleApp.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly BotDbContext _context;

        public UserRepository(BotDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BotUser>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<BotUser> GetByIdAsync(Guid id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task AddAsync(BotUser botUser)
        {
            await _context.Users.AddAsync(botUser);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(BotUser botUser)
        {
            _context.Users.Update(botUser);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var botUser = await _context.Users.FindAsync(id);
            if (botUser != null)
            {
                _context.Users.Remove(botUser);
                await _context.SaveChangesAsync();
            }
        }

        public Task<BotUser?> GetByChatIdAsync(long chatId)
        {
            return _context.Users.Where(x => x.ChatId == chatId).FirstOrDefaultAsync();
        }
    }
}
