namespace MacBot.ConsoleApp.Models
{
    public class InviteCode
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public BotUser? Master {  get; set; }
        public Guid? MasterId { get; set; }
        public string? Code { get; set; }

        public InviteCode(BotUser master, string code)
        {
            Master = master;
            MasterId = master.Id;
            Code = code;
        }

        public InviteCode() { }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != GetType()) return false;
            var other = obj as InviteCode;
            if (GetHashCode() != other.GetHashCode()) return false;
            return Code == other.Code;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode() + MasterId.GetHashCode();
        }

        public override string ToString()
        {
            return Code.ToString();
        }
    }
}
