using MacBot.ConsoleApp.Models;
using MacBot.ConsoleApp.Repository.Database;

using Microsoft.EntityFrameworkCore;

namespace MacBot.ConsoleApp.Repository
{
    public class SessionRepository : ISessionRepository
    {
        private readonly BotDbContext _context;

        public SessionRepository(BotDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Session session)
        {
            await _context.Sessions.AddAsync(session);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var session = await _context.Sessions.FindAsync(id);
            if (session != null)
            {
                _context.Sessions.Remove(session);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Session> GetActiveByUser(BotUser user)
        {
            return await _context.Sessions
                .Where(s => s.IsActive)
                .Where(s => s.MasterId == user.Id || s.ClientId == user.Id)
                .Include(s => s.ChoosenCards)
                .FirstOrDefaultAsync();
        }

        public async Task<Session> GetActiveSession(BotUser master)
        {
            return await _context.Sessions
                .Where(s => s.MasterId == master.Id)
                .Where(s => s.IsActive)
                .OrderByDescending(s => s.Date)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Session>> GetAllAsync()
        {
            return await _context.Sessions.ToListAsync();
        }

        public async Task<IEnumerable<Session>> GetAllByMaster(BotUser master)
        {
            return await _context.Sessions
                .Where(m => m.Master.Id == master.Id)
                .ToListAsync(); 
        }

        public async Task<IEnumerable<Session>> GetAllForExport(BotUser master)
        {
            return await _context.Sessions
                .Where(m => m.Master.Id == master.Id)
                .Include(s => s.Client)
                .Include(s => s.ChoosenCards)
                .ThenInclude(sc => sc.Card)
                .ToListAsync();
        }

        public async Task<IEnumerable<Session>> GetAllByMasterAndClient(BotUser master, BotUser client)
        {
            return await _context.Sessions
                .Where(s => s.MasterId == master.Id && s.ClientId == client.Id)
                .ToListAsync();
        }

        public async Task<Session> GetByIdAsync(Guid id)
        {
            return await _context.Sessions.FindAsync(id);
        }

        public async Task UpdateAsync(Session session)
        {
            _context.Sessions.Update(session);
            await _context.SaveChangesAsync();
        }
    }
}
