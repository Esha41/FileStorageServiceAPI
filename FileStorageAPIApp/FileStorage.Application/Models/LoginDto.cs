
using System.Text.Json.Serialization;

namespace FileStorage.Application.Models
{
    public class LoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
        [JsonIgnore]
        public string? Role { get; set; }
    }
}
