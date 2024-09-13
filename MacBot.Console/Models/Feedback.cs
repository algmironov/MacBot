namespace MacBot.ConsoleApp.Models
{
    public class Feedback
    {
        public Guid Id { get; set; } = new Guid();
        public string Name { get; set; }
        public long ChatId { get; set; }
        public DateTime Date { get; set; }
        public string Text { get; set; }
        public bool Read {  get; set; } = false;

        public Feedback(long chatId, string name, string text)
        {
            Date = DateTime.Now;
            Name = name;
            ChatId = chatId;
            Text = text;
        }
    }
}
