using CloudRu.ObjectStorageHelper;

using MacBot.ConsoleApp.Models;

using SimpleLogger;

namespace MacBot.ConsoleApp.Services
{
    internal class CloudService : IObjectStorageService
    {
        private readonly ObjectStorageService _service;
        private readonly Logger _logger;
        public CloudService(ObjectStorageServiceSettings settings, Logger logger)
        {
            _service = new ObjectStorageService(
                accessKey: settings.AccessKey,
                secretKey: settings.SecretKey,
                serviceUrl: settings.ServiceUrl,
                bucketName: "mac-bot-cards"
                );
            _logger = logger;
        }

        public async Task CreateFolderAsync(string folderName)
        {
            try
            {
                await _service.CreateFolderAsync(folderName);
            }
            catch (Exception ex)
            {
                _logger.Error("Ошибка при создании папки в облаке: ", ex);
            }
        }

        public async Task<Stream> GetFileAsync(string folderName, string fileName)
        {
            return await _service.GetFileAsync(folderName, fileName);
        }

        public async Task<List<string>> ListFilesInFolderAsync(string folderName)
        {
            return await _service.ListFilesInFolderAsync(folderName);
        }

        public async Task<List<string>> ListFoldersAsync()
        {
            try
            {
                return await _service.ListFoldersAsync();
            }
            catch (Exception ex)
            {
                _logger.Error("Ошибка при получении списка папок в облаке: ", ex);
                return [];
            }
        }

        public async Task UploadFileAsync(string folderName, string fileName)
        {
            await _service.UploadFileAsync(folderName, fileName);
        }
    }
}
