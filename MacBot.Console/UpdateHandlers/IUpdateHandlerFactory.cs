using Telegram.Bot.Types;

namespace MacBot.ConsoleApp.UpdateHandlers
{
    public interface IUpdateHandlerFactory
    {
        IUpdateHandler GetHandler(Update update);
    }
}
