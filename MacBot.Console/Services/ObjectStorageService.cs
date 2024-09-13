using Amazon.S3;
using Amazon.S3.Model;

using SimpleLogger;

namespace MacBot.ConsoleApp.Services
{
    public class ObjectStorageService : IObjectStorageService
    {
        private readonly AmazonS3Client _s3Client;
        private readonly string _bucketName = "mac-bot-cards";
        private readonly Logger _logger;

        public ObjectStorageService(string accessKey, string secretKey, string serviceUrl, Logger logger)
        {
            var config = new AmazonS3Config
            {
                ServiceURL = serviceUrl,
                ForcePathStyle = true
            };

            _s3Client = new AmazonS3Client(accessKey, secretKey, config);
            _logger = logger;
        }

        public async Task CreateFolderAsync(string folderName)
        {
            try
            {
                var request = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = folderName + "/",
                    ContentBody = string.Empty
                };

                await _s3Client.PutObjectAsync(request);
            }
            catch (Exception ex)
            {
                _logger.Error($"Возникла ошибка при создании папки в облаке", ex);
            }
        }

        public async Task UploadFileAsync(string folderName, string filePath)
        {
            try
            {
                var fileName = Path.GetFileName(filePath);
                var request = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = folderName + "/" + fileName,
                    FilePath = filePath
                };

                await _s3Client.PutObjectAsync(request);
            }
            catch (Exception ex)
            {
                _logger.Error($"Возникла ошибка при сохранении файла в облаке", ex);
            }
        }

        public async Task<List<string>> ListFoldersAsync()
        {
            var request = new ListObjectsV2Request
            {
                BucketName = _bucketName,
                Delimiter = "/"
            };
            var folders = new List<string>();

            try
            {
                var response = await _s3Client.ListObjectsV2Async(request);



                foreach (var prefix in response.CommonPrefixes)
                {
                    folders.Add(prefix.TrimEnd('/'));
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Произошла ошибка при получении списка папок в облаке", ex);
            }

            return folders;
        }

        public async Task<List<string>> ListFilesInFolderAsync(string folderName)
        {
            var request = new ListObjectsV2Request
            {
                BucketName = _bucketName,
                Prefix = folderName + "/"
            };
            var files = new List<string>();

            try
            {
                var response = await _s3Client.ListObjectsV2Async(request);

                foreach (var s3Object in response.S3Objects)
                {
                    files.Add(s3Object.Key);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Произошла ошибка при получении списка файлов в папке в облаке", ex);
            }

            return files;
        }

        public async Task<Stream> GetFileAsync(string folderName, string fileName)
        {
            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = folderName + "/" + fileName
                };

                var response = await _s3Client.GetObjectAsync(request);
                return response.ResponseStream;
            }
            catch (Exception ex)
            {
                _logger.Error($"Произошла ошибка при получении файла из облака", ex);
                return null;
            }
        }
    }
}
