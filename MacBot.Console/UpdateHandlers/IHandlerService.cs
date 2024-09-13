using MacBot.ConsoleApp.Models;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace MacBot.ConsoleApp.UpdateHandlers
{
    public interface IHandlerService
    {
        Task RemoveUserMessage(ITelegramBotClient client, Update update);
        Task<bool> HasActiveSession(BotUser user);
        Task<BotUser> GetOrAddBotUser(Update update);
        Task UpdateUserRole(BotUser user);
        long GetChatId(Update update);
        Task SetLastMessage(BotMessage botMessage);
        Task<bool> IsLastMessageEditable(long chatId);
        Task<BotMessage> GetLast(long chatId);
        Dictionary<string, string> GetButtonsWithCallbacks(List<string> buttons, string prefix);
        Task DeleteLastMessage(ITelegramBotClient client, BotMessage botMessage);
        Task DeleteImageMessages(ITelegramBotClient client, List<BotMessage> botMessages);
        Task ClearChat(ITelegramBotClient client, long chatId);
        Task RemoveImagePageKeyboard(ITelegramBotClient client, long chatId, int messageId);
        bool IsOwner(long chatId);
    }
}
