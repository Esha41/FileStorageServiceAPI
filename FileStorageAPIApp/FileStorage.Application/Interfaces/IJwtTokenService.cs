using FileStorage.Domain.Enums;

namespace FileStorage.Application.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateToken(string username, string role);
    }
}
