using System;

namespace FileStorage.Application.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateToken(string userId, string email);
    }
}
