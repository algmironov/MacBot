using MacBot.ConsoleApp.Repository;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace MacBot.ConsoleApp.Pages
{
    public abstract class Page
    {
        public bool HasPreviousPage;
        public PageName? PreviousPage;
        public string? Text;
        public List<string>? Buttons;
        public IReplyMarkup? Keyboard;
        public abstract Task<Message> Show(ITelegramBotClient client, long chatId, IReplyMarkup keyboard);
        public abstract Task<Message> GoToPreviousPage(ITelegramBotClient client, long chatId, IReplyMarkup keyboard, Page previousPage);
    }
}
