using System.ComponentModel.DataAnnotations;
namespace MacBot.ConsoleApp.Models
{
    public class BotMessage
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public int MessageId { get; set; }
        public long ChatId { get; set; }
        public BotMessageType MessageType { get; set; } = BotMessageType.Text;
        public bool IsLast {  get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public BotMessage() { }

        public BotMessage(int messageId,  long chatId, BotMessageType botMessageType)
        {
            MessageId = messageId;
            ChatId = chatId;
            MessageType = botMessageType;
        }
    }
}
