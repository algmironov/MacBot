using MacBot.ConsoleApp.Models;
using MacBot.ConsoleApp.Repository.Database;

using Microsoft.EntityFrameworkCore;

namespace MacBot.ConsoleApp.Repository
{
    public class CardRepository : ICardRepository
    {
        private BotDbContext _context;
        public CardRepository(BotDbContext context)
        {
            _context = context;
        }

        public async Task DeleteAsync(Guid id)
        {
            var card = await _context.Cards.FindAsync(id);
            if (card != null)
            {
                _context.Cards.Remove(card);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Card> TryGetAsync(Guid id)
        {
            return await _context.Cards.FindAsync(id);
        }

        public async Task<Card> TryGetAsync(Deck deck, int number)
        {
            var deckId = deck.Id;
            var cards = await _context.Cards.Where(card => card.DeckId == deckId).ToListAsync();

            return cards.First(x => int.Parse(Path.GetFileNameWithoutExtension(x.Link.Split("_").Last())) == number);
        }

        public async Task SaveAsync(Card card)
        {
            await _context.Cards.AddAsync(card);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Card card)
        {
            _context.Cards.Update(card);

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Card>> GetAllAsync()
        {
            return await _context.Cards.ToListAsync();
        }

        public async Task<Card> GetByLink(string link)
        {
            return await _context.Cards.Where(card => card.Link == link).FirstAsync();
        }

        public async Task<List<Card>> GetAllBySessionCards(IEnumerable<SessionCard> sessionCards)
        {
            List<Card> result = new List<Card>();
            foreach (SessionCard sessionCard in sessionCards)
            {
                result.Add(await TryGetAsync(sessionCard.CardId));
            }
            return result;
        }

        public async Task<List<Card>> GetAllByDeckNameAsync(string deckName)
        {
            return await _context.Cards
                .Include(card => card.Deck)
                .Where(card => card.Deck!.Name == deckName).ToListAsync();
        }
    }
}
