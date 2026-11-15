using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly RaceDayContext _db;
    private readonly IConfiguration _configuration;
    public AuthController(RaceDayContext db, IConfiguration configuration) { _db = db; _configuration = configuration; }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(x => x.Email == email))
            return Conflict(new { message = "Email is already registered." });

        var role = string.IsNullOrWhiteSpace(request.Role) ? "Participant" : request.Role.Trim();
        if (role != "Participant" && role != "Organiser")
            return BadRequest(new { message = "Choose Participant or Organiser." });

        // Organiser registration is protected by a private invite code.
        // Participants can register without this code.
        if (role == "Organiser")
        {
            var requiredCode = _configuration["Security:OrganizerInviteCode"];
            if (string.IsNullOrWhiteSpace(requiredCode) || request.OrganizerInviteCode != requiredCode)
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "A valid Organiser invitation code is required." });
        }

        var user = new User {
            FirstName = request.FirstName.Trim(), LastName = request.LastName.Trim(),
            Email = email, Phone = request.Phone?.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password), Role = role
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return StatusCode(StatusCodes.Status201Created, new { user.UserID, user.FirstName, user.LastName, user.Email, user.Role });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.SingleOrDefaultAsync(x => x.Email == email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Email or password is incorrect." });

        HttpContext.Session.Clear();
        HttpContext.Session.SetInt32("UserID", user.UserID);
        HttpContext.Session.SetString("Role", user.Role);
        HttpContext.Session.SetString("Email", user.Email);
        return Ok(new { message = "Login successful.", user.UserID, user.FirstName, user.LastName, user.Email, user.Role });
    }

    [HttpPost("logout")]
    public IActionResult Logout() { HttpContext.Session.Clear(); return Ok(new { message = "You have logged out." }); }

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var id = HttpContext.Session.GetInt32("UserID");
        if (id == null) return Unauthorized(new { message = "Please log in." });
        var user = await _db.Users.FindAsync(id.Value);
        if (user == null) return Unauthorized();
        return Ok(new { user.UserID, user.FirstName, user.LastName, user.Email, user.Phone, user.Role, user.ProfilePictureUrl });
    }
}
