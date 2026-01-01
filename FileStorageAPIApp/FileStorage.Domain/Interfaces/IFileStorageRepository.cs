using FileStorage.Domain.Entities;

namespace FileStorage.Domain.Interfaces
{
    public interface IFileStorageRepository
    {
        Task AddFile(StoredObject entity);
        Task<(IEnumerable<StoredObject> Items, int TotalCount)> GetAllFiles(string? name,string? tag,string? contentType,DateTime? dateFrom, DateTime? dateTo, int pageNumber,int pageSize);
        Task<StoredObject?> GetFileById(string id);
        Task SoftDeleteFileById(StoredObject stored);
        Task HardDeleteFileById(StoredObject stored);
    }
}
