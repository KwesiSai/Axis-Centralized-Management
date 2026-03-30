using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManagementAPI.Data;
using UserManagementAPI.DTOs;

namespace UserManagementAPI.Controllers;

[ApiController]
[Route("api/applications")]
public class ApplicationsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ApplicationsController(AppDbContext context)
    {
        _context = context;
    }

    // GET /api/applications
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var apps = await _context.Applications
            .OrderBy(a => a.Name)
            .Select(a => new ApplicationResponseDto(a.Id, a.Name, a.Description))
            .ToListAsync();

        return Ok(apps);
    }
}
