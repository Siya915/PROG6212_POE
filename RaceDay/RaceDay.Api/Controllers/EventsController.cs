using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly RaceDayContext _db;
    public EventsController(RaceDayContext db) { _db = db; }

    private static object ToSummary(Event x) => new { x.EventID, x.OrganizerID, x.EventName, x.EventType, x.EventDate, x.StartTime, x.Venue, x.Description, x.Capacity, x.Status, x.BannerImageUrl,
        EnrolmentCount = x.Enrolments.Count(e => e.Status == "Confirmed"),
        Categories = x.EventCategories.Select(c => new { c.CategoryID, c.Category!.CategoryName }) };

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? status, [FromQuery] string? eventType)
    {
        var query = _db.Events.Include(x => x.EventCategories).ThenInclude(x => x.Category).Include(x => x.Enrolments).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.EventName.Contains(search) || x.Venue.Contains(search));
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == status);
        if (!string.IsNullOrWhiteSpace(eventType)) query = query.Where(x => x.EventType == eventType);
        return Ok(await query.OrderBy(x => x.EventDate).Select(x => new { x.EventID, x.OrganizerID, x.EventName, x.EventType, x.EventDate, x.StartTime, x.Venue, x.Description, x.Capacity, x.Status, x.BannerImageUrl,
            EnrolmentCount = x.Enrolments.Count(e => e.Status == "Confirmed"),
            Categories = x.EventCategories.Select(c => new { c.CategoryID, c.Category!.CategoryName }) }).ToListAsync());
    }

    [HttpGet("mine")]
    public async Task<IActionResult> Mine()
    {
        var id = HttpContext.Session.GetInt32("UserID");
        if (id == null) return Unauthorized();
        if (HttpContext.Session.GetString("Role") != "Organiser") return Forbid();
        var events = await _db.Events.Where(x => x.OrganizerID == id).Include(x => x.EventCategories).ThenInclude(x => x.Category).Include(x => x.Enrolments).OrderBy(x => x.EventDate).ToListAsync();
        return Ok(events.Select(ToSummary));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var item = await _db.Events.Include(x => x.EventCategories).ThenInclude(x => x.Category).Include(x => x.Enrolments).FirstOrDefaultAsync(x => x.EventID == id);
        if (item == null) return NotFound();
        return Ok(ToSummary(item));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateEventDto request)
    {
        var userId = HttpContext.Session.GetInt32("UserID");
        if (userId == null) return Unauthorized();
        if (HttpContext.Session.GetString("Role") != "Organiser") return Forbid();
        var item = new Event { OrganizerID = userId.Value, EventName=request.EventName.Trim(), EventType=request.EventType.Trim(), EventDate=request.EventDate.Date,
            StartTime=request.StartTime, Venue=request.Venue.Trim(), Description=request.Description?.Trim(), Capacity=request.Capacity, Status="Open" };
        _db.Events.Add(item); await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id=item.EventID }, ToSummary(item));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CreateEventDto request)
    {
        var userId = HttpContext.Session.GetInt32("UserID");
        if (userId == null) return Unauthorized();
        if (HttpContext.Session.GetString("Role") != "Organiser") return Forbid();
        var item = await _db.Events.FindAsync(id);
        if (item == null) return NotFound();
        if (item.OrganizerID != userId.Value) return Forbid();
        var confirmedCount=await _db.Enrolments.CountAsync(x=>x.EventID==id&&x.Status=="Confirmed");
        if(request.Capacity<confirmedCount)return Conflict(new { message="Capacity cannot be lower than the number of confirmed enrolments." });
        item.EventName=request.EventName.Trim(); item.EventType=request.EventType.Trim(); item.EventDate=request.EventDate.Date; item.StartTime=request.StartTime;
        item.Venue=request.Venue.Trim(); item.Description=request.Description?.Trim(); item.Capacity=request.Capacity;
        await _db.SaveChangesAsync(); return Ok(new { message="Event updated." });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = HttpContext.Session.GetInt32("UserID");
        if (userId == null) return Unauthorized();
        if (HttpContext.Session.GetString("Role") != "Organiser") return Forbid();
        var item = await _db.Events.Include(x=>x.Enrolments).FirstOrDefaultAsync(x=>x.EventID==id);
        if (item == null) return NotFound();
        if (item.OrganizerID != userId.Value) return Forbid();
        if (item.Enrolments.Any()) return Conflict(new { message="An event with enrolments cannot be deleted. Close or complete it instead." });
        _db.Events.Remove(item); await _db.SaveChangesAsync(); return NoContent();
    }
}
