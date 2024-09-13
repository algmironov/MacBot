namespace MacBot.ConsoleApp.Models
{
    public class Session
    {
        public Guid SessionId { get; private set; } = Guid.NewGuid();
        public BotUser? Master { get; private set; }
        public Guid MasterId { get; private set; }
        public BotUser? Client { get; set; }
        public Guid ClientId { get; set; }  
        public DateTime Date { get; private set; }
        public TimeSpan Duration { get; private set; }
        public ICollection<SessionCard>? ChoosenCards { get; set; } = new List<SessionCard>();
        public bool IsActive { get; private set; } = true;
        public Session() { }
        public Session(BotUser master, BotUser client)
        {
            Master = master;
            Client = client;
            Date = DateTime.Now;
        }

        public void SetDuration() 
        { 
            Duration = TimeSpan.FromMinutes((DateTime.Now - Date).Minutes); 
        }

        public void EndSession()
        {
            IsActive = false;
            SetDuration();
        }
    }
}
