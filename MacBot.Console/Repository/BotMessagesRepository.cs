using MacBot.ConsoleApp.Models;
using MacBot.ConsoleApp.Repository.Database;

using Microsoft.EntityFrameworkCore;

namespace MacBot.ConsoleApp.Repository
{
    public class BotMessagesRepository : IBotMessagesRepository
    {
        private readonly BotDbContext _context;

        public BotMessagesRepository(BotDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(BotMessage botMessage)
        {
            await _context.BotMessages.AddAsync(botMessage);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(BotMessage botMessage)
        {
            var message = await _context.BotMessages.FindAsync(botMessage.Id);
            if (message != null)
            {
                _context.BotMessages.Remove(message);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<BotMessage>> GetAllAsync()
        {
            return await _context.BotMessages.ToListAsync();
        }

        public async Task<IEnumerable<BotMessage>> GetAllByChatId(long chatId)
        {
            return await _context
                .BotMessages
                .Where(msg => msg.ChatId == chatId)
                .ToListAsync();
        }

        public async Task UpdateAsync(BotMessage botMessage)
        {
             _context.BotMessages.Update(botMessage);
            await _context.SaveChangesAsync();
        }

        public async Task AddOrUpdateAsync(BotMessage botMessage)
        {
            var existingBotMessage = await _context.BotMessages
                .FirstOrDefaultAsync(bm => bm.Id == botMessage.Id);

            if (existingBotMessage != null)
            {
                existingBotMessage.MessageId = botMessage.MessageId;
                existingBotMessage.ChatId = botMessage.ChatId;
                existingBotMessage.MessageType = botMessage.MessageType;
                existingBotMessage.IsLast = botMessage.IsLast;
                existingBotMessage.IsDeleted = botMessage.IsDeleted;

                _context.BotMessages.Update(existingBotMessage);
            }
            else
            {
                await _context.BotMessages.AddAsync(botMessage);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<BotMessage> GetLast(long chatId)
        {
            var messages = await GetAllByChatId(chatId);
            return messages.LastOrDefault();
        }

        public async Task<IEnumerable<BotMessage>> GetAllShownByChatId(long chatId)
        {
            return await _context
                .BotMessages
                .Where(msg => msg.ChatId == chatId)
                .Where(msg => msg.IsDeleted == false)
                .ToListAsync();
        }

        public async Task ClearDeletedMesages(long chatId)
        {
            var messages = await GetAllByChatId(chatId);
            var toDelete = messages.Where(msg => msg.IsDeleted);
            foreach (var message in toDelete)
            {
                await DeleteAsync(message);
            }
        }
    }
}
