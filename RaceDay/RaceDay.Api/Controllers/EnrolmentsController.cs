using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
namespace RaceDay.Api.Controllers;
[ApiController, Route("api/enrolments")]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayContext _db;
    public EnrolmentsController(RaceDayContext db) { _db = db; }
    [HttpGet] public async Task<IActionResult> GetAll()
    {
        var id=HttpContext.Session.GetInt32("UserID"); var role=HttpContext.Session.GetString("Role"); if(id==null)return Unauthorized();
        var query=_db.Enrolments.Include(x=>x.Event).Include(x=>x.Participant).Include(x=>x.Category).Include(x=>x.Result).AsQueryable();
        if(role=="Organiser") query=query.Where(x=>x.Event!.OrganizerID==id.Value);
        else if(role=="Participant") query=query.Where(x=>x.ParticipantID==id.Value);
        else return Forbid();
        return Ok(await query.OrderByDescending(x=>x.EnrolmentDate).Select(x=>new { x.EnrolmentID,x.EventID,EventName=x.Event!.EventName,EventDate=x.Event!.EventDate,x.ParticipantID,
            ParticipantName=x.Participant!.FirstName+" "+x.Participant.LastName,x.CategoryID,CategoryName=x.Category!.CategoryName,x.EnrolmentDate,x.Status,
            HasResult=x.Result!=null }).ToListAsync());
    }
    [HttpPost] public async Task<IActionResult> Enrol(EnrolRequest request)
    {
        var id=HttpContext.Session.GetInt32("UserID"); if(id==null)return Unauthorized(); if(HttpContext.Session.GetString("Role")!="Participant")return Forbid();
        var eventItem=await _db.Events.Include(x=>x.EventCategories).FirstOrDefaultAsync(x=>x.EventID==request.EventID);
        if(eventItem==null)return NotFound(new { message="Event not found." }); if(eventItem.Status!="Open")return Conflict(new { message="This event is not open." });
        if(eventItem.EventDate.Date<DateTime.Today)return Conflict(new { message="This event has already taken place." });
        if(!eventItem.EventCategories.Any(x=>x.CategoryID==request.CategoryID))return BadRequest(new { message="Choose a category assigned to this event." });
        var existing=await _db.Enrolments.FirstOrDefaultAsync(x=>x.EventID==request.EventID&&x.ParticipantID==id);
        if(existing!=null && existing.Status=="Confirmed")return Conflict(new { message="You are already enrolled in this event." });
        var count=await _db.Enrolments.CountAsync(x=>x.EventID==request.EventID&&x.Status=="Confirmed"); if(count>=eventItem.Capacity)return Conflict(new { message="This event is full." });
        if(existing!=null)
        {
            // The unique EventID + ParticipantID rule keeps one enrolment row per person/event.
            // A cancelled enrolment can be reactivated instead of inserting a duplicate row.
            existing.CategoryID=request.CategoryID; existing.Status="Confirmed"; existing.EnrolmentDate=DateTime.Now;
            await _db.SaveChangesAsync();
            return Ok(new { existing.EnrolmentID,existing.EventID,existing.CategoryID,existing.Status,message="Enrolment reactivated." });
        }
        var item=new Enrolment { EventID=request.EventID, ParticipantID=id.Value, CategoryID=request.CategoryID, Status="Confirmed" };
        _db.Enrolments.Add(item); await _db.SaveChangesAsync(); return Created("/api/enrolments",new { item.EnrolmentID,item.EventID,item.CategoryID,item.Status });
    }
    [HttpDelete("{id:int}")] public async Task<IActionResult> Cancel(int id)
    {
        var userId=HttpContext.Session.GetInt32("UserID"); if(userId==null)return Unauthorized();
        var item=await _db.Enrolments.Include(x=>x.Event).FirstOrDefaultAsync(x=>x.EnrolmentID==id); if(item==null)return NotFound();
        var role=HttpContext.Session.GetString("Role");
        if(role=="Participant" && item.ParticipantID!=userId.Value)return Forbid();
        if(role=="Organiser" && item.Event!.OrganizerID!=userId.Value)return Forbid();
        if(role!="Participant"&&role!="Organiser")return Forbid();
        if(item.Event!.EventDate.Date<=DateTime.Today)return Conflict(new { message="An enrolment cannot be cancelled on or after the event date." });
        if(item.Status=="Cancelled")return NoContent(); item.Status="Cancelled"; await _db.SaveChangesAsync(); return NoContent();
    }
}
