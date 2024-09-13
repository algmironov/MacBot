using MacBot.ConsoleApp.Models;

namespace MacBot.ConsoleApp.Services
{
    public interface IDeckService
    {
        Task SynchronizeDecksWithStorage();
        Task<Deck> GetDeckAsync(string name);
        Task<Deck> GetDeckAsync(Guid id);
        Task RemoveDeckAsync(string name);
        Task<Dictionary<string, Guid>> GetDecksAsync();
        Task<List<string>> ListDecksAsync();
        Task<Deck> GetDeckByCardAsync(Card card);
    }
}
