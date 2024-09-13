using MacBot.ConsoleApp.Models;

namespace MacBot.ConsoleApp.Repository
{
    public interface ICardRepository
    {
        Task<Card> TryGetAsync(Guid id);
        Task<Card> TryGetAsync(Deck deck, int number);
        Task UpdateAsync(Card card);
        Task SaveAsync(Card card);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<Card>> GetAllAsync();
        Task<Card> GetByLink(string link);
        Task<List<Card>> GetAllBySessionCards(IEnumerable<SessionCard> sessionCards);
        Task<List<Card>> GetAllByDeckNameAsync(string deckName);
    }
}
