using System.ComponentModel.DataAnnotations;

namespace FileStorage.Domain.Entities
{
    public class StoredObject
    {
        [Key]  
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Key { get; set; } = null!;
        public string OriginalName { get; set; } = null!;
        public long SizeBytes { get; set; }
        public string ContentType { get; set; } = null!;
        public string Checksum { get; set; } = null!;
        public string? Tags { get; set; }  
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? DeletedAtUtc { get; set; }
        public int Version { get; set; }
        public Guid CreatedByUserId { get; set; }
    }
}
