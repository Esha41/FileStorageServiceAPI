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
        private readonly ILogger<AuthController> _logger;

        public AuthController(IJwtTokenService token, IConfiguration config, ILogger<AuthController> logger)
        {
            _token = token;
            _config = config;
            _logger = logger;
            _users = _config.GetSection("Users").Get<List<LoginDto>>() ?? new List<LoginDto>();
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login([FromBody] LoginDto login)
        {
            _logger.LogInformation("Login attempt received. Username={Username}", login.Username);

            try
            {
                var user = _users.FirstOrDefault(u => u.Username == login.Username && u.Password == login.Password);
                if (user == null)
                {
                    _logger.LogWarning("Login failed. Invalid credentials. Username={Username}", login.Username);
                    throw new Exception("Invalid username or password");
                }

                var token = _token.GenerateToken(user.Username, user.Role ?? Role.Admin.ToString());
                _logger.LogInformation("Login successful. Username={Username}, Role={Role}", user.Username, user.Role);

                return Ok(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error. Username={Username}", login.Username);
                return BadRequest(ex.Message);
            }
        }
    }
}
