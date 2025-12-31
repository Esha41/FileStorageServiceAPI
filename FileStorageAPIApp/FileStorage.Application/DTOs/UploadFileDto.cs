using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace FileStorage.Application.DTOs
{
    public class UploadFileDto
    {
        [Required]
        public IFormFile File { get; set; }

        public string[]? Tags { get; set; }
    }
}
