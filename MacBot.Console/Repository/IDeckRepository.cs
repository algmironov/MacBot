using MacBot.ConsoleApp.Models;

namespace MacBot.ConsoleApp.Repository
{
    public interface IDeckRepository
    {
        Task<List<Deck>> FindAllAsync();
        Task AddAsync(Deck deck);
        Task<Deck> FindAsync(string name);
        Task<Deck> FindAsync(Guid id);
        Task DeleteAsync(Guid id);
        Task<Deck> GetDeckWithCards(Guid id);
    }
}
