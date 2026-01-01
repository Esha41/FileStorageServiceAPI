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

        public async Task<(IEnumerable<StoredObject> Items, int TotalCount)> GetAllFiles(string? name, string? tag, string? contentType, DateTime? dateFrom, DateTime? dateTo, int pageNumber, int pageSize)
        {
            var query = _context.StoredObjects.AsQueryable();
            query = query.Where(f => f.DeletedAtUtc == null);

            if (!string.IsNullOrEmpty(name))
                query = query.Where(f => f.OriginalName.Contains(name));

            if (!string.IsNullOrEmpty(tag))
                query = query.Where(f => f.Tags.Contains(tag));

            if (!string.IsNullOrEmpty(contentType))
                query = query.Where(f => f.ContentType.Contains(contentType));

            if (dateFrom.HasValue)
                query = query.Where(f => f.CreatedAtUtc >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(f => f.CreatedAtUtc <= dateTo.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(f => f.CreatedAtUtc)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
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

        public async Task SoftDeleteFileById(StoredObject stored)
        {
            stored.DeletedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
