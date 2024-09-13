namespace MacBot.ConsoleApp.Services
{
    public interface IObjectStorageService
    {
        Task CreateFolderAsync(string folderName);
        Task<Stream> GetFileAsync(string folderName, string fileName);
        Task<List<string>> ListFilesInFolderAsync(string folderName);
        Task<List<string>> ListFoldersAsync();
        Task UploadFileAsync(string folderName, string fileName);
    }
}