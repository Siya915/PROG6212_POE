using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Services;
namespace RaceDay.Api.Controllers;
[ApiController, Route("api/files")]
public class FilesController : ControllerBase
{
    private static readonly string[] AllowedTypes={"image/jpeg","image/png","image/webp"};
    private readonly RaceDayContext _db; private readonly IImageStorageService _storage; private readonly IConfiguration _configuration;
    public FilesController(RaceDayContext db,IImageStorageService storage,IConfiguration configuration){_db=db;_storage=storage;_configuration=configuration;}

    [HttpPost("profile-picture")]
    [RequestSizeLimit(5_000_000)]
    public async Task<IActionResult> UploadProfilePicture(IFormFile file,CancellationToken token)
    {
        var userId=HttpContext.Session.GetInt32("UserID"); if(userId==null)return Unauthorized();
        if(!IsValid(file))return BadRequest(new { message="Upload a JPG, PNG or WEBP image up to 5 MB." });
        var user=await _db.Users.FindAsync(new object?[]{userId.Value},token); if(user==null)return NotFound();
        user.ProfilePictureUrl=ToPublicUrl(await _storage.SaveAsync(file,"profiles",token)); await _db.SaveChangesAsync(token);
        return Ok(new { imageUrl=user.ProfilePictureUrl });
    }

    [HttpPost("events/{eventId:int}/banner")]
    [RequestSizeLimit(5_000_000)]
    public async Task<IActionResult> UploadEventBanner(int eventId,IFormFile file,CancellationToken token)
    {
        var userId=HttpContext.Session.GetInt32("UserID"); if(userId==null)return Unauthorized();
        if(HttpContext.Session.GetString("Role")!="Organiser")return Forbid();
        if(!IsValid(file))return BadRequest(new { message="Upload a JPG, PNG or WEBP image up to 5 MB." });
        var item=await _db.Events.FindAsync(new object?[]{eventId},token); if(item==null)return NotFound();
        if(item.OrganizerID!=userId.Value)return Forbid();
        item.BannerImageUrl=ToPublicUrl(await _storage.SaveAsync(file,$"events/{eventId}",token)); await _db.SaveChangesAsync(token);
        return Ok(new { imageUrl=item.BannerImageUrl });
    }

    private string ToPublicUrl(string url)
    {
        if(Uri.TryCreate(url,UriKind.Absolute,out _)) return url;
        var baseUrl=_configuration["ImageStorage:PublicBaseUrl"];
        if(string.IsNullOrWhiteSpace(baseUrl)) baseUrl=$"{Request.Scheme}://{Request.Host}";
        return baseUrl.TrimEnd('/')+url;
    }

    private static bool IsValid(IFormFile? file) => file!=null && file.Length>0 && file.Length<=5_000_000 && AllowedTypes.Contains(file.ContentType,StringComparer.OrdinalIgnoreCase);
}
