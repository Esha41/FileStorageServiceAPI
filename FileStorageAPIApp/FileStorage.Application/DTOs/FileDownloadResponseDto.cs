
namespace FileStorage.Application.DTOs
{
    public class FileDownloadResponseDto
    {
        public Stream Stream { get; init; } = default!;

        public string ContentType { get; init; } = default!;

        public string FileName { get; init; } = default!;
    }
}
