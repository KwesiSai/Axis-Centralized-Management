using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserManagementAPI.Data;
using UserManagementAPI.DTOs;

namespace UserManagementAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    // POST /api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        // 1. Find user
        var user = await _context.Users
            .Include(u => u.UserApplications)
                .ThenInclude(ua => ua.Application)
            .FirstOrDefaultAsync(u => u.Username == dto.Username);

        if (user is null || !user.IsActive)
            return Unauthorized(new { message = "Invalid credentials or account is inactive." });

        // 2. Verify password
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid credentials or account is inactive." });

        // 3. Check application access
        var app = await _context.Applications
            .FirstOrDefaultAsync(a => a.Name == dto.Application);

        if (app is null)
            return BadRequest(new { message = $"Application '{dto.Application}' does not exist." });

        var hasAccess = user.UserApplications.Any(ua => ua.ApplicationId == app.Id);
        if (!hasAccess)
            return Unauthorized(new { message = $"Access denied to '{dto.Application}'." });

        // 4. Issue JWT
        var token = GenerateJwt(user.Id, user.Username, dto.Application);

        var appNames = user.UserApplications.Select(ua => ua.Application.Name).ToList();

        return Ok(new LoginResponseDto(token, user.Username, appNames));
    }

    private string GenerateJwt(int userId, string username, string application)
    {
        var jwtKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not configured.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, username),
            new Claim("application", application),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
