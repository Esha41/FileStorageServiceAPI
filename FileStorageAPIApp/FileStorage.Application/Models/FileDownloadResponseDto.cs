using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStorage.Application.Models
{
    public class FileDownloadResponseDto
    {
        public Stream Stream { get; init; } = default!;

        public string ContentType { get; init; } = default!;

        public string FileName { get; init; } = default!;
    }
}
