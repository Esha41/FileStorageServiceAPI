using FileStorage.Domain.Entities;
using FileStorage.Domain.Interfaces;
using FileStorage.Infrastructure.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace FileStorage.Infrastructure.Repositories
{
    public class FileStorageRepository : IFileStorageRepository
    {
        private readonly ApplicationDbContext _context;

        public FileStorageRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddFile(StoredObject entity)
        {
            await _context.StoredObjects.AddAsync(entity);
            _context.SaveChanges();
        }

        public async Task<StoredObject?> GetFileById(string id)
        {
            return await _context.StoredObjects.Where(x => x.Id == Guid.Parse(id)).FirstOrDefaultAsync();
        }

        public async Task HardDeleteFileById(StoredObject stored)
        {
            _context.StoredObjects.Remove(stored);
            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteFileById(string id)
        {
            var stored = await _context.StoredObjects.FindAsync(Guid.Parse(id));
            stored.DeletedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
