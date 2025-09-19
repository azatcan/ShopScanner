using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserService.Domain.Entities;
using UserService.Domain.Services;

namespace UserService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<User> userManager, RoleManager<AppRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest model)
        {
            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            if (!await _roleManager.RoleExistsAsync("User"))
                await _roleManager.CreateAsync(new AppRole("User"));

            await _userManager.AddToRoleAsync(user, "User");

            return Ok("Kullanıcı başarıyla oluşturuldu.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized("Geçersiz kullanıcı adı veya şifre.");

            var token = await GenerateJwtToken(user);
            return Ok(new { Token = token });
        }

        private async Task<string> GenerateJwtToken(User user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:DurationInMinutes"]));

            var token = new JwtSecurityToken(
                _configuration["JwtSettings:Issuer"],
                _configuration["JwtSettings:Audience"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public record RegisterRequest(string Name, string Email, string Password);
    public record LoginRequest(string Email, string Password);
}
