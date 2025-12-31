using FileStorage.Application.DTOs;
using FileStorage.Domain.Entities;

namespace FileStorage.Application.Interfaces
{
    public interface IFileStorageService
    {
        Task<StoredObject> UploadFile(Stream fileStream, string originalName, string contentType, string[] tags, string userId);
        Task<(IEnumerable<StoredObject> Items, int TotalCount)> GetAllFiles(FilesQueryDto filesFilter);
        Task<FileDownloadResponseDto> DownloadFile(string id);
        Task<bool> SoftDeleteFileById(string id);
        Task<bool> HardDeleteFileById(string id);
    }
}
