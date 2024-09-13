using MacBot.ConsoleApp.Models;
using MacBot.ConsoleApp.Repository.Database;

using Microsoft.EntityFrameworkCore;

namespace MacBot.ConsoleApp.Repository
{
    public class DeckRepository : IDeckRepository
    {
        private readonly BotDbContext _context;

        public DeckRepository(BotDbContext context)
        {
            _context = context;
        }

        public async Task<Deck> FindAsync(string name)
        {
            return _context.Decks.Where(x => x.Name == name).FirstOrDefault();
        }

        public async Task<Deck> FindAsync(Guid id)
        {
            return await _context.Decks.FindAsync(id);
        }

        public async Task AddAsync(Deck deck)
        {
            await _context.Decks.AddAsync(deck);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Deck>> FindAllAsync()
        {
            return await _context.Decks.ToListAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var deck = await _context.Decks.FirstOrDefaultAsync(x => x.Id == id);
            if (deck != null){
                 _context.Decks.Remove(deck);
                await _context.SaveChangesAsync();
            }
            
        }

        public async Task<Deck> GetDeckWithCards(Guid id)
        {
            return await _context.Decks
                .Include(d => d.Cards)
                .FirstOrDefaultAsync(d => d.Id == id);
        }
    }
}
