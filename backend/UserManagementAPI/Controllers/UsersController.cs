using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManagementAPI.Data;
using UserManagementAPI.DTOs;
using UserManagementAPI.Models;

namespace UserManagementAPI.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    // GET /api/users — Admin only
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var users = await _context.Users
            .Include(u => u.UserApplications)
                .ThenInclude(ua => ua.Application)
            .OrderBy(u => u.Username)
            .Select(u => new UserResponseDto(
                u.Id,
                u.Username,
                u.IsActive,
                u.Role,
                u.CreatedAt,
                u.UserApplications.Select(ua => ua.Application.Name).ToList()
            ))
            .ToListAsync();

        return Ok(users);
    }

    // GET /api/users/me — Any authenticated user (own profile)
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                  ?? User.FindFirst("sub")
                  ?? throw new Exception("User ID claim missing");

var userId = int.Parse(userIdClaim.Value);

        var user = await _context.Users
            .Include(u => u.UserApplications)
                .ThenInclude(ua => ua.Application)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
            return NotFound(new { message = "User not found." });

        return Ok(new UserResponseDto(
            user.Id,
            user.Username,
            user.IsActive,
            user.Role,
            user.CreatedAt,
            user.UserApplications.Select(ua => ua.Application.Name).ToList()
        ));
    }

    // GET /api/users/{id} — Admin only
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _context.Users
            .Include(u => u.UserApplications)
                .ThenInclude(ua => ua.Application)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
            return NotFound(new { message = "User not found." });

        return Ok(new UserResponseDto(
            user.Id,
            user.Username,
            user.IsActive,
            user.Role,
            user.CreatedAt,
            user.UserApplications.Select(ua => ua.Application.Name).ToList()
        ));
    }

    // POST /api/users — Admin only
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(new { message = "Username and password are required." });

        if (dto.Password.Length < 6)
            return BadRequest(new { message = "Password must be at least 6 characters." });

        var exists = await _context.Users.AnyAsync(u => u.Username == dto.Username);
        if (exists)
            return Conflict(new { message = "Username already taken." });

        var user = new User
        {
            Username = dto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = "User"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = user.Id },
            new UserResponseDto(user.Id, user.Username, user.IsActive, user.Role, user.CreatedAt, new()));
    }

    // PATCH /api/users/{id}/deactivate — Admin only
    [HttpPatch("{id}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null) return NotFound(new { message = "User not found." });
        if (!user.IsActive) return BadRequest(new { message = "User is already deactivated." });

        user.IsActive = false;
        await _context.SaveChangesAsync();
        return Ok(new { message = $"User '{user.Username}' has been deactivated." });
    }

    // PATCH /api/users/{id}/activate — Admin only
    [HttpPatch("{id}/activate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Activate(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null) return NotFound(new { message = "User not found." });
        if (user.IsActive) return BadRequest(new { message = "User is already active." });

        user.IsActive = true;
        await _context.SaveChangesAsync();
        return Ok(new { message = $"User '{user.Username}' has been activated." });
    }

    // POST /api/users/{id}/applications — Admin only
    [HttpPost("{id}/applications")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignApplication(int id, [FromBody] AssignApplicationDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null) return NotFound(new { message = "User not found." });

        var app = await _context.Applications.FindAsync(dto.ApplicationId);
        if (app is null) return NotFound(new { message = "Application not found." });

        var alreadyAssigned = await _context.UserApplications
            .AnyAsync(ua => ua.UserId == id && ua.ApplicationId == dto.ApplicationId);

        if (alreadyAssigned)
            return Conflict(new { message = $"User already has access to '{app.Name}'." });

        _context.UserApplications.Add(new UserApplication { UserId = id, ApplicationId = dto.ApplicationId });
        await _context.SaveChangesAsync();

        return Ok(new { message = $"User '{user.Username}' assigned to '{app.Name}'." });
    }

    // DELETE /api/users/{id}/applications/{appId} — Admin only
    [HttpDelete("{id}/applications/{appId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RevokeApplication(int id, int appId)
    {
        var userApp = await _context.UserApplications
            .FirstOrDefaultAsync(ua => ua.UserId == id && ua.ApplicationId == appId);

        if (userApp is null) return NotFound(new { message = "Assignment not found." });

        _context.UserApplications.Remove(userApp);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Application access revoked." });
    }

    // DELETE /api/users/{id} — Admin only
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null) return NotFound(new { message = "User not found." });

        if (user.Role == "Admin")
            return BadRequest(new { message = "Cannot delete the admin account." });

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return Ok(new { message = $"User '{user.Username}' deleted." });
    }
}