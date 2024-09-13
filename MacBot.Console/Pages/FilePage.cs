using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace MacBot.ConsoleApp.Pages
{
    public class FilePage : Page
    {
        public Stream? File { get; set; }
        public string FileName { get; set; }
        public override Task<Message> GoToPreviousPage(ITelegramBotClient client, long chatId, IReplyMarkup keyboard, Page previousPage)
        {
            throw new NotImplementedException();
        }

        public override async Task<Message> Show(ITelegramBotClient client, long chatId, IReplyMarkup keyboard)
        {
            return await client.SendDocumentAsync(
                chatId: chatId,
                caption: Text,
                replyMarkup: keyboard,
                document: InputFile.FromStream(File, FileName)
                );
        }
    }
}
