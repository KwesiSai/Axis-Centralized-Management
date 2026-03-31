using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserManagementAPI.Data;
using UserManagementAPI.DTOs;
using UserManagementAPI.Models;

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

    // POST /api/auth/register-admin
    // One-time endpoint — blocked once an admin already exists
    [HttpPost("register-admin")]
    public async Task<IActionResult> RegisterAdmin([FromBody] RegisterAdminDto dto)
    {
        // Validate secret key to prevent random people hitting this endpoint
        var expectedSecret = _config["AdminRegistration:SecretKey"];
        if (string.IsNullOrEmpty(expectedSecret) || dto.SecretKey != expectedSecret)
            return Unauthorized(new { message = "Invalid secret key." });

        // Block if admin already exists
        var adminExists = await _context.Users.AnyAsync(u => u.Role == "Admin");

        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(new { message = "Username and password are required." });

        if (dto.Password.Length < 6)
            return BadRequest(new { message = "Password must be at least 6 characters." });

        var usernameExists = await _context.Users.AnyAsync(u => u.Username == dto.Username);
        if (usernameExists)
            return Conflict(new { message = "Username already taken." });

        var admin = new User
        {
            Username = dto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = "Admin",
            IsActive = true
        };

        _context.Users.Add(admin);
        await _context.SaveChangesAsync();

        return Ok(new { message = $"Admin account '{dto.Username}' created successfully." });
    }

    // POST /api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _context.Users
            .Include(u => u.UserApplications)
                .ThenInclude(ua => ua.Application)
            .FirstOrDefaultAsync(u => u.Username == dto.Username);

        if (user is null || !user.IsActive)
            return Unauthorized(new { message = "Invalid credentials or account is inactive." });

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid credentials or account is inactive." });

        // Admin bypasses application access check
        if (user.Role != "Admin")
        {
            var app = await _context.Applications
                .FirstOrDefaultAsync(a => a.Name == dto.Application);

            if (app is null)
                return BadRequest(new { message = $"Application '{dto.Application}' does not exist." });

            var hasAccess = user.UserApplications.Any(ua => ua.ApplicationId == app.Id);
            if (!hasAccess)
                return Unauthorized(new { message = $"Access denied to '{dto.Application}'." });
        }

        var token = GenerateJwt(user.Id, user.Username, user.Role, dto.Application);
        var appNames = user.UserApplications.Select(ua => ua.Application.Name).ToList();

        return Ok(new LoginResponseDto(token, user.Username, user.Role, appNames));
    }

    private string GenerateJwt(int userId, string username, string role, string application)
    {
        var jwtKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not configured.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, username),
            new Claim(ClaimTypes.Role, role),
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