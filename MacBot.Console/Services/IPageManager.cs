using MacBot.ConsoleApp.Pages;
using MacBot.ConsoleApp.Repository;

using Telegram.Bot;

namespace MacBot.ConsoleApp.Services
{
    public interface IPageManager
    {
        Page CreatePage(PageName pageName);
        PageName GetPreviousPageName(PageName pageName);
        Task DeletePage(ITelegramBotClient client, long chatId, int messageId);
    }
}
