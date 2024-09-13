using MacBot.ConsoleApp.Models;
using MacBot.ConsoleApp.Repository.Database;

using Microsoft.EntityFrameworkCore;

namespace MacBot.ConsoleApp.Repository
{
    public class SessionCardRepository : ISessionCardRepository
    {
        private readonly BotDbContext _context;

        public SessionCardRepository(BotDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(SessionCard sessionCard)
        {
            await _context.SessionCards.AddAsync(sessionCard);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(Guid sessionId, Guid cardId)
        {
            var sessionCard = await _context.SessionCards.FindAsync(sessionId, cardId);
            if (sessionCard != null)
            {
                _context.SessionCards.Remove(sessionCard);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<SessionCard>> GetSessionCardsAsync(Guid sessionId)
        {
            return await _context.SessionCards
                .Where(sc => sc.SessionId == sessionId)
                .ToListAsync();
        }

        public async Task<List<SessionCard>> GetCardsInSessionAsync(Guid cardId)
        {
            return await _context.SessionCards
                .Where(sc => sc.CardId == cardId)
                .ToListAsync();
        }

        public async Task<SessionCard> TryGetBySession(Guid sessionId)
        {
            var sessionCard = await _context.SessionCards.FirstOrDefaultAsync(sc => sc.SessionId == sessionId);
            return sessionCard!;
        }

        public async Task<SessionCard> GetBySessionAndCard(Guid sessionId, Guid cardId)
        {
            return await _context.SessionCards
            .Include(sc => sc.Session)
            .Include(sc => sc.Card)
            .FirstOrDefaultAsync(sc => sc.SessionId == sessionId && sc.CardId == cardId);
        }

        public async Task Update(SessionCard sessionCard)
        {
            _context.SessionCards.Update(sessionCard);
            await _context.SaveChangesAsync();
        }
    }
}
