using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
namespace RaceDay.Api.Controllers;
[ApiController, Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly RaceDayContext _db;
    public UsersController(RaceDayContext db) { _db = db; }
    [HttpGet("profile")] public async Task<IActionResult> Profile()
    {
        var id=HttpContext.Session.GetInt32("UserID"); if(id==null)return Unauthorized();
        var user=await _db.Users.FindAsync(id.Value); if(user==null)return NotFound();
        return Ok(new { user.UserID,user.FirstName,user.LastName,user.Email,user.Phone,user.Role,user.ProfilePictureUrl });
    }
    [HttpPut("profile")] public async Task<IActionResult> UpdateProfile(ProfileUpdateDto request)
    {
        var id=HttpContext.Session.GetInt32("UserID"); if(id==null)return Unauthorized();
        var user=await _db.Users.FindAsync(id.Value); if(user==null)return NotFound();
        user.FirstName=request.FirstName.Trim(); user.LastName=request.LastName.Trim(); user.Phone=request.Phone?.Trim();
        await _db.SaveChangesAsync(); return Ok(new { message="Profile updated." });
    }
}
