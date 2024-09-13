using MacBot.ConsoleApp.Repository;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace MacBot.ConsoleApp.Pages
{
    public class ImagePage : Page
    {
        public string? ImageLink {  get; set; }

        public Stream? ImageStream { get; set; }

        public ImagePage(bool hasPreviousPage, PageName? previousPage, string? imageLink, string text, List<string> buttons) 
        {
            HasPreviousPage = hasPreviousPage;
            PreviousPage = previousPage;
            Text = text;
            Buttons = buttons;
            ImageLink = imageLink;
        }

        public ImagePage() { }

        public override async Task<Message> Show(ITelegramBotClient client, long chatId, IReplyMarkup keyboard)
        {
            return await client.SendPhotoAsync
                (
                    chatId: chatId,
                    photo: InputFile.FromStream(GetImage(ImageLink)),
                    caption: Text,
                    replyMarkup: keyboard
                );
        }

        public async Task<Message> ShowStream(ITelegramBotClient client, long chatId, IReplyMarkup keyboard)
        {
            return await client.SendPhotoAsync
                (
                    chatId: chatId,
                    photo: InputFile.FromStream(ImageStream),
                    caption: Text,
                    replyMarkup: keyboard
                );
        }

        public async Task<Message> ShowStream(ITelegramBotClient client, long chatId)
        {
            return await client.SendPhotoAsync
                (
                    chatId: chatId,
                    photo: InputFile.FromStream(ImageStream),
                    caption: Text
                );
        }

        public async Task<Message> RemoveKeyboard(ITelegramBotClient client, long chatId, int messageId)
        {
            
            return await client.EditMessageReplyMarkupAsync
                (
                    chatId: chatId,
                    messageId: messageId,
                    replyMarkup: null
                );
        }

        public async Task<Message> Show(ITelegramBotClient client, long chatId)
        {
            return await client.SendPhotoAsync
                (
                    chatId: chatId,
                    photo: InputFile.FromStream(GetImage(ImageLink))
                );
        }

        public override async Task<Message> GoToPreviousPage(ITelegramBotClient client, long chatId, IReplyMarkup keyboard, Page previousPage)
        {
            if (!HasPreviousPage) { throw new Exception("Incorrect method call"); } 
            return await previousPage.Show(client, chatId, keyboard);
        }

        private FileStream? GetImage(string? image)
        {
            try
            {
                return new FileStream(image, FileMode.Open, FileAccess.Read);
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        ~ImagePage()
        {
            ImageStream?.Dispose();
        }
    }
}
