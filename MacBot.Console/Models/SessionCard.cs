namespace MacBot.ConsoleApp.Models
{
    public class SessionCard
    {
        public Guid SessionId { get; set; } = Guid.NewGuid();
        public Session? Session { get; set; }

        public Guid CardId { get; set; }
        public Card? Card { get; set; }

        public bool IsShown { get; set; } = false;
    }
}
