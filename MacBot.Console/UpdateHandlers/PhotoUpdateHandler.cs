using MacBot.ConsoleApp.Services;

using SimpleLogger;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace MacBot.ConsoleApp.UpdateHandlers
{
    public class PhotoUpdateHandler : IUpdateHandler
    {
        private string _newDeckName;

        private Logger _logger;
        private IObjectStorageService _objectStorageService;

        public PhotoUpdateHandler(IObjectStorageService objectStorageService, Logger logger)
        {
            _logger = logger;
            _objectStorageService = objectStorageService;
        }
        public async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
        {
            await HandlePhotoUpdates(client, update, cancellationToken);
        }

        private async Task HandlePhotoUpdates(ITelegramBotClient client, Update update, CancellationToken token)
        {
            try
            {
                var fileId = update.Message.Photo[^1].FileId;
                var file = await client.GetFileAsync(fileId);
                var folderName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp");

                if (!Directory.Exists(folderName))
                {
                    Directory.CreateDirectory(folderName);
                }

                var filePath = Path.Combine(folderName, file.FilePath!);

                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                using (var saveImageStream = new FileStream(filePath, FileMode.Create))
                {

                    await client.DownloadFileAsync(file.FilePath!, saveImageStream, token);
                }

                await _objectStorageService.UploadFileAsync(_newDeckName, filePath);
            }
            catch (Exception e)
            {
                _logger.Error($"Возникла ошибка при обработке сообщения с картинкой", e);
            }
        }
    }
}
