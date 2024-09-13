namespace MacBot.ConsoleApp.Models
{
    public class SessionParameters
    {
        public Guid Id { get; set; } = Guid.NewGuid(); 
        public Guid? MasterId { get; set; }
        public Guid? DeckId {  get; set; } 
        public int? CardsToShowCount { get; set; }
        public int? Duration { get; set; }
        public DateTime CreationTime { get; set; } = DateTime.Now;

        public SessionParameters() { }

        public SessionParameters(Guid masterId)
        {
            MasterId = masterId;
        }

    }
}
