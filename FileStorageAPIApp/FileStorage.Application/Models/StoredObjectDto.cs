using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStorage.Application.Models
{
    public class StoredObjectDto
    {
        public Guid Id { get; set; }
        public string Key { get; set; } = null!;
        public string OriginalName { get; set; } = null!;
        public long SizeBytes { get; set; }
        public string ContentType { get; set; } = null!;
        public string? Tags { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public int Version { get; set; }
    }
}
