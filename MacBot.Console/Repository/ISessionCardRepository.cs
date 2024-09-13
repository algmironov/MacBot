using MacBot.ConsoleApp.Models;

namespace MacBot.ConsoleApp.Repository
{
    public interface ISessionCardRepository
    {
        Task AddAsync(SessionCard sessionCard);
        Task RemoveAsync(Guid sessionId, Guid cardId);
        Task<List<SessionCard>> GetSessionCardsAsync(Guid sessionId);
        Task<List<SessionCard>> GetCardsInSessionAsync(Guid cardId);
        Task<SessionCard> TryGetBySession(Guid sessionId);
        Task<SessionCard> GetBySessionAndCard(Guid sessionId, Guid cardId);
        Task Update(SessionCard sessionCard);
    }
}
