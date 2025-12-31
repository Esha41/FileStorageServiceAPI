using FileStorage.Application.Models;
using FileStorage.Domain.Entities;

namespace FileStorage.Application.Interfaces
{
    public interface IFileStorageService
    {
        Task<StoredObject> UploadFile(Stream fileStream, string originalName, string contentType, string[] tags, string userId);
        Task<FileDownloadResponseDto> DownloadFile(string id);
        Task<bool> SoftDeleteFileById(string id);
        Task<bool> HardDeleteFileById(string id);
    }
}
