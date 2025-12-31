
namespace FileStorage.Application.DTOs
{
    public class FilesQueryDto
    {
        public string? Name { get; set; }
        public string? Tag { get; set; }
        public string? ContentType { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
