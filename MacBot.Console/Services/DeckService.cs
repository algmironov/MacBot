using MacBot.ConsoleApp.Models;
using MacBot.ConsoleApp.Repository;

namespace MacBot.ConsoleApp.Services
{
    public class DeckService : IDeckService
    {
        private readonly IDeckRepository _deckRepository;
        private readonly ICardRepository _cardRepository;
        private readonly IObjectStorageService _objectStorageService;
        private bool _isSynchronized;

        public DeckService(IDeckRepository deckRepository, ICardRepository cardRepository, IObjectStorageService objectStorageService)
        {
            _deckRepository = deckRepository;
            _cardRepository = cardRepository;
            _objectStorageService = objectStorageService;
        }

        public async Task SynchronizeDecksWithStorage()
        {
            if (_isSynchronized) return;

            var folders = await _objectStorageService.ListFoldersAsync() ?? [];
            var existingDecks = await _deckRepository.FindAllAsync() ?? [];

            var existingDeckNames = new HashSet<string>(existingDecks.Select(d => d.Name));
            var missingDeckNames = folders.Where(name => !existingDeckNames.Contains(name));

            foreach (var deck in missingDeckNames)
            {
                var cloudCards = await _objectStorageService.ListFilesInFolderAsync(deck);
                var deckName = cloudCards[1].Split('/')[0];


                var newDeck = new Deck(deckName, "");
                await _deckRepository.AddAsync(newDeck);

                cloudCards.RemoveAt(0);
                foreach (var cloudCard in cloudCards)
                {
                    var link = cloudCard.Split('/')[1];
                    var newCard = new Card(link, newDeck);
                    await _cardRepository.SaveAsync(newCard);
                }
            }
            _isSynchronized = true;
        }

        public async Task<List<string>> ListDecksAsync()
        {
            await SynchronizeDecksWithStorage();

            var decks = await _deckRepository.FindAllAsync();

            return decks.Select(deck => deck.Name).ToList();
        }


        public async Task<Deck> GetDeckAsync(string name)
        {
            return await _deckRepository.FindAsync(name);
        }

        public async Task<Deck> GetDeckAsync(Guid id)
        {
            return await _deckRepository.FindAsync(id);
        }

        public async Task<Deck> GetDeckByCardAsync(Card card)
        {
            return await _deckRepository.FindAsync((Guid) card.DeckId!);
        }

        public async Task RemoveDeckAsync(string name)
        {
            var deck = await GetDeckAsync(name);
            await _deckRepository.DeleteAsync(deck.Id);
            _isSynchronized = false;
        }

        public async Task<Dictionary<string, Guid>> GetDecksAsync()
        {
            Dictionary<string, Guid> decks = [];
            var allDecks = await _deckRepository.FindAllAsync();
            foreach (var deck in allDecks)
            {
                decks.TryAdd(deck.Name, deck.Id);
            }
            return decks;
        }
    }
}
