using FileStorage.Application.Interfaces;
using FileStorage.Application.Models.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FileStorage.Infrastructure.Security
{
    public class JwtTokenService: IJwtTokenService
    {
        private readonly IConfiguration _configuration;
       // private readonly List<LoginModel> _users;
        public JwtTokenService(IConfiguration config)
        {
            _configuration = config;
          //  _users = _configuration.GetSection("Users").Get<List<LoginModel>>() ?? new List<LoginModel>();
        }

        public string GenerateToken(string username, string password)
        {
            //var user = _users.FirstOrDefault(u => u.Username == username && u.Password == password);
            //if (user == null) return null;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
              //  new Claim(ClaimTypes.Role, user.Role)
            };

            var jwtSettings = _configuration.GetSection("JwtSettings").Get<JWTConfig>();
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings?.Issuer,
                audience: jwtSettings?.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(jwtSettings.TokenValidityInMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
