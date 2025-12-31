using FileStorage.Application.DTOs;
using FileStorage.Application.Interfaces;
using FileStorage.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileStorage.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IJwtTokenService _token;
        private readonly IConfiguration _config;
        private readonly List<LoginDto> _users;
        public AuthController(IJwtTokenService token, IConfiguration config)
        {
            _token = token;
            _config = config;
            _users = _config.GetSection("Users").Get<List<LoginDto>>() ?? new List<LoginDto>();
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login([FromBody] LoginDto login)
        {
            try
            {
                var user = _users.FirstOrDefault(u => u.Username == login.Username && u.Password == login.Password);
                if (user == default)
                    throw new Exception("Invalid username or password");

                return Ok(_token.GenerateToken(user.Username, user.Role??Role.Admin.ToString()));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
