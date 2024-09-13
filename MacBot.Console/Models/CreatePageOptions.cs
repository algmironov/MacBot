using Telegram.Bot;
using Telegram.Bot.Types;

namespace MacBot.ConsoleApp.Models
{
    public class CreatePageOptions
    {
        public ITelegramBotClient? Client { get; set; }
        public Update? Update { get; set; }
        public BotUser? User { get; set; }
        public BotUser? SessionClient { get; set; }
        public BotMessage? ClientMessage { get; set; }
        public BotMessage?  Message { get; set; }
        public Session? Session { get; set; }
        public long ChatId { get; set; }
        public string? MessageData { get; set; }

        public CreatePageOptions(ITelegramBotClient? client, Update? update, BotUser? user, BotMessage? message, long chatId, string? messageData)
        {
            Client = client;
            Update = update;
            User = user;
            Message = message;
            ChatId = chatId;
            MessageData = messageData;
        }

        public CreatePageOptions()
        {
            
        }
    }
}
