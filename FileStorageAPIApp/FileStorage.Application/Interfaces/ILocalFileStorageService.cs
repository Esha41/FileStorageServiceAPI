using FileStorage.Domain.Entities;

namespace FileStorage.Application.Interfaces
{
    public interface ILocalFileStorageService
    {
        Task<StoredObject> UploadFile(Stream fileStream, string originalName, string contentType, string[] tags, string userId);
        Task<Stream> DownloadFile(string key);
        bool DeleteFile(string key);
    }
}
