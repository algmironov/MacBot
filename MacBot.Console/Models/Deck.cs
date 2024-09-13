namespace MacBot.ConsoleApp.Models
{
    public class Deck
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public ICollection<Card> Cards { get; set; } = new List<Card>();

        public Deck(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}
