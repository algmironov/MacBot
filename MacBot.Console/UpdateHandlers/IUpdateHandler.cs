using Telegram.Bot;
using Telegram.Bot.Types;

namespace MacBot.ConsoleApp.UpdateHandlers
{
    public interface IUpdateHandler
    {
        Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken);
    }
}
