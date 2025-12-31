
using FileStorage.Domain.Entities;

namespace FileStorage.Domain.Interfaces
{
    public interface IFileStorageRepository
    {
        Task AddFile(StoredObject entity);
        Task<StoredObject?> GetFileById(string id);
        Task SoftDeleteFileById(string id);
        Task HardDeleteFileById(StoredObject stored);
    }
}
