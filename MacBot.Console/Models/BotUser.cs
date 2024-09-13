namespace MacBot.ConsoleApp.Models
{
    public class BotUser
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string? Name { get; set; }
        public long ChatId { get; set; }
        public Role? ActiveRole { get; set; }
        public ICollection<Session> Sessions { get; set; } = new List<Session>();

        public BotUser(string? name, long chatId, Role? activeRole)
        {
            Name = name ?? string.Empty;
            ChatId = chatId;
            ActiveRole = activeRole;
        }

        public void ChangeRole()
        {
            ActiveRole = ActiveRole == Role.Client ? Role.Master : Role.Client;
        }

    }
}
