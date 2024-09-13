using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MacBot.ConsoleApp.Pages
{
    public class TextPage : Page
    {
        public override async Task<Message> GoToPreviousPage(ITelegramBotClient client, long chatId, IReplyMarkup keyboard, Page previousPage)
        {
            return await previousPage.Show(client, chatId, keyboard);
        }

        public override async Task<Message> Show(ITelegramBotClient client, long chatId, IReplyMarkup keyboard)
        {
            return await client.SendTextMessageAsync
                (
                    chatId: chatId,
                    text: Text,
                    replyMarkup: keyboard
                );
        }

        public async Task<Message> Show(ITelegramBotClient client, long chatId)
        {
            return await client.SendTextMessageAsync
                (
                    chatId: chatId,
                    text: Text
                );
        }

        public async Task<Message> Update(ITelegramBotClient client, long chatId, int messageId, IReplyMarkup keyboard)
        {
            try
            {
                return await client.EditMessageTextAsync
                        (
                            chatId: chatId,
                            messageId: messageId,
                            text: Text,
                            replyMarkup: keyboard as InlineKeyboardMarkup
                        );
            }
            catch (Exception)
            {
                return await Show(
                    client: client,
                    chatId: chatId,
                    keyboard: keyboard as InlineKeyboardMarkup
                    );
            }
        }

        public async Task<Message> Update(ITelegramBotClient client, long chatId, int messageId, IReplyMarkup keyboard, ParseMode parseMode)
        {
            return await client.EditMessageTextAsync
                (
                    chatId: chatId,
                    messageId: messageId,
                    text: Text,
                    replyMarkup: keyboard as InlineKeyboardMarkup,
                    parseMode: parseMode
                );
        }

    }
}
