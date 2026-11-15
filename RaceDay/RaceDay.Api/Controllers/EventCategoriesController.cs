using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
namespace RaceDay.Api.Controllers;
[ApiController, Route("api/events/{eventId:int}/categories")]
public class EventCategoriesController : ControllerBase
{
    private readonly RaceDayContext _db;
    public EventCategoriesController(RaceDayContext db) { _db = db; }
    [HttpPost("{categoryId:int}")] public async Task<IActionResult> Assign(int eventId, int categoryId)
    {
        var userId=HttpContext.Session.GetInt32("UserID"); if(userId==null)return Unauthorized(); if(HttpContext.Session.GetString("Role")!="Organiser")return Forbid();
        var eventItem=await _db.Events.FindAsync(eventId); if(eventItem==null)return NotFound(new { message="Event not found." });
        if(eventItem.OrganizerID!=userId.Value)return Forbid();
        if(!await _db.Categories.AnyAsync(x=>x.CategoryID==categoryId))return NotFound(new { message="Category not found." });
        if(await _db.EventCategories.AnyAsync(x=>x.EventID==eventId&&x.CategoryID==categoryId))return Conflict(new { message="Category already assigned." });
        _db.EventCategories.Add(new EventCategory { EventID=eventId, CategoryID=categoryId }); await _db.SaveChangesAsync(); return Ok(new { message="Category assigned." });
    }
    [HttpDelete("{categoryId:int}")] public async Task<IActionResult> Remove(int eventId, int categoryId)
    {
        var userId=HttpContext.Session.GetInt32("UserID"); if(userId==null)return Unauthorized(); if(HttpContext.Session.GetString("Role")!="Organiser")return Forbid();
        var eventItem=await _db.Events.FindAsync(eventId); if(eventItem==null)return NotFound(); if(eventItem.OrganizerID!=userId.Value)return Forbid();
        var link=await _db.EventCategories.FindAsync(eventId,categoryId); if(link==null)return NotFound();
        if(await _db.Enrolments.AnyAsync(x=>x.EventID==eventId&&x.CategoryID==categoryId&&x.Status=="Confirmed"))return Conflict(new { message="A category selected by a participant cannot be removed." });
        _db.EventCategories.Remove(link); await _db.SaveChangesAsync(); return NoContent();
    }
}
