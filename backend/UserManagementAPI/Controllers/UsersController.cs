using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManagementAPI.Data;
using UserManagementAPI.DTOs;
using UserManagementAPI.Models;

namespace UserManagementAPI.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    // GET /api/users
    [HttpGet]
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
                u.CreatedAt,
                u.UserApplications.Select(ua => ua.Application.Name).ToList()
            ))
            .ToListAsync();

        return Ok(users);
    }

    // GET /api/users/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _context.Users
            .Include(u => u.UserApplications)
                .ThenInclude(ua => ua.Application)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
            return NotFound(new { message = "User not found." });

        var response = new UserResponseDto(
            user.Id,
            user.Username,
            user.IsActive,
            user.CreatedAt,
            user.UserApplications.Select(ua => ua.Application.Name).ToList()
        );

        return Ok(response);
    }

    // POST /api/users
    [HttpPost]
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
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = user.Id },
            new UserResponseDto(user.Id, user.Username, user.IsActive, user.CreatedAt, new()));
    }

    // PATCH /api/users/{id}/deactivate
    [HttpPatch("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null)
            return NotFound(new { message = "User not found." });

        if (!user.IsActive)
            return BadRequest(new { message = "User is already deactivated." });

        user.IsActive = false;
        await _context.SaveChangesAsync();

        return Ok(new { message = $"User '{user.Username}' has been deactivated." });
    }

    // PATCH /api/users/{id}/activate
    [HttpPatch("{id}/activate")]
    public async Task<IActionResult> Activate(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null)
            return NotFound(new { message = "User not found." });

        if (user.IsActive)
            return BadRequest(new { message = "User is already active." });

        user.IsActive = true;
        await _context.SaveChangesAsync();

        return Ok(new { message = $"User '{user.Username}' has been activated." });
    }

    // POST /api/users/{id}/applications
    [HttpPost("{id}/applications")]
    public async Task<IActionResult> AssignApplication(int id, [FromBody] AssignApplicationDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null)
            return NotFound(new { message = "User not found." });

        var app = await _context.Applications.FindAsync(dto.ApplicationId);
        if (app is null)
            return NotFound(new { message = "Application not found." });

        var alreadyAssigned = await _context.UserApplications
            .AnyAsync(ua => ua.UserId == id && ua.ApplicationId == dto.ApplicationId);

        if (alreadyAssigned)
            return Conflict(new { message = $"User already has access to '{app.Name}'." });

        var userApp = new UserApplication { UserId = id, ApplicationId = dto.ApplicationId };
        _context.UserApplications.Add(userApp);
        await _context.SaveChangesAsync();

        return Ok(new { message = $"User '{user.Username}' assigned to '{app.Name}'." });
    }

    // DELETE /api/users/{id}/applications/{appId}
    [HttpDelete("{id}/applications/{appId}")]
    public async Task<IActionResult> RevokeApplication(int id, int appId)
    {
        var userApp = await _context.UserApplications
            .FirstOrDefaultAsync(ua => ua.UserId == id && ua.ApplicationId == appId);

        if (userApp is null)
            return NotFound(new { message = "Assignment not found." });

        _context.UserApplications.Remove(userApp);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Application access revoked." });
    }

    // DELETE /api/users/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null)
            return NotFound(new { message = "User not found." });

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = $"User '{user.Username}' deleted." });
    }
}
