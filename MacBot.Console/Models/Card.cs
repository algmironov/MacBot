using System.ComponentModel.DataAnnotations;

namespace MacBot.ConsoleApp.Models
{
    public class Card
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        public string Link { get; private set; }
        
        public Guid? DeckId { get; set; }
        public Deck? Deck { get; set; }

        public ICollection<SessionCard>? SessionCards { get; set; } = new List<SessionCard>();

        public Card(string link, Deck deck)
        {
            Link = link;
            Deck = deck;
            DeckId = Deck.Id;
        }

        public Card() { }
    }
}
