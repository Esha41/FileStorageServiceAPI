
namespace FileStorage.Application.DTOs.Configurations
{
    public class JWTConfig
    {
        public string? Issuer { get; set; }
        public string? Audience { get; set; }
        public string? Key { get; set; }
        public int TokenValidityInMinutes { get; set; }
    }
}
