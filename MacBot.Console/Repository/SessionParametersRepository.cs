using MacBot.ConsoleApp.Models;
using MacBot.ConsoleApp.Repository.Database;

using Microsoft.EntityFrameworkCore;

namespace MacBot.ConsoleApp.Repository
{
    public class SessionParametersRepository : ISessionParametersRepository
    {
        private readonly BotDbContext _context;

        public SessionParametersRepository(BotDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SessionParameters>> GetAllAsync()
        {
            return await _context.SessionParameters.ToListAsync();
        }

        public async Task<SessionParameters> GetByIdAsync(Guid id)
        {
            return await _context.SessionParameters.FindAsync(id);
        }

        public async Task AddAsync(SessionParameters sessionParameters)
        {
            await _context.SessionParameters.AddAsync(sessionParameters);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(SessionParameters sessionParameters)
        {
            var existingSessionParameters = await _context.SessionParameters.FindAsync(sessionParameters.Id);

            if (existingSessionParameters != null)
            {
                existingSessionParameters.MasterId = sessionParameters.MasterId;
                existingSessionParameters.DeckId = sessionParameters.DeckId;
                existingSessionParameters.CardsToShowCount = sessionParameters.CardsToShowCount;
                existingSessionParameters.Duration = sessionParameters.Duration;

                _context.SessionParameters.Update(existingSessionParameters);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            var sessionParameters = await _context.SessionParameters.FindAsync(id);

            if (sessionParameters != null)
            {
                _context.SessionParameters.Remove(sessionParameters);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<SessionParameters> GetByMasterId(Guid masterId)
        {
            return await _context.SessionParameters
                .OrderByDescending(sp => sp.CreationTime)
                .FirstOrDefaultAsync(sp => sp.MasterId == masterId);
        }

    }
}
