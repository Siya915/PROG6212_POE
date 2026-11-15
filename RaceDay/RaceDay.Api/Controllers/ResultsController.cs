using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
namespace RaceDay.Api.Controllers;
[ApiController, Route("api/results")]
public class ResultsController : ControllerBase
{
    private readonly RaceDayContext _db;
    public ResultsController(RaceDayContext db) { _db = db; }
    [HttpGet] public async Task<IActionResult> GetAll()
    {
        var id=HttpContext.Session.GetInt32("UserID"); var role=HttpContext.Session.GetString("Role"); if(id==null)return Unauthorized();
        var query=_db.Results.Include(x=>x.Enrolment).ThenInclude(x=>x!.Event).Include(x=>x.Enrolment).ThenInclude(x=>x!.Participant).Include(x=>x.Enrolment).ThenInclude(x=>x!.Category).AsQueryable();
        if(role=="Participant")query=query.Where(x=>x.Enrolment!.ParticipantID==id.Value);
        else if(role=="Organiser")query=query.Where(x=>x.Enrolment!.Event!.OrganizerID==id.Value);
        else return Forbid();
        return Ok(await query.OrderBy(x=>x.Enrolment!.Event!.EventDate).Select(x=>new { x.ResultID,x.EnrolmentID,EventID=x.Enrolment!.EventID,
            EventName=x.Enrolment.Event!.EventName,ParticipantName=x.Enrolment.Participant!.FirstName+" "+x.Enrolment.Participant.LastName,
            CategoryName=x.Enrolment.Category!.CategoryName,x.FinishTime,x.Position,x.TotalFinishers,x.Points,x.RecordedAt,EventDate=x.Enrolment.Event!.EventDate }).ToListAsync());
    }
    [HttpPost("{enrolmentId:int}")] public async Task<IActionResult> Add(int enrolmentId, CreateResultDto request)
    {
        var id=HttpContext.Session.GetInt32("UserID"); if(id==null)return Unauthorized(); if(HttpContext.Session.GetString("Role")!="Organiser")return Forbid();
        var enrolment=await _db.Enrolments.Include(x=>x.Event).FirstOrDefaultAsync(x=>x.EnrolmentID==enrolmentId);
        if(enrolment==null)return NotFound(new { message="Enrolment not found." }); if(enrolment.Event!.OrganizerID!=id.Value)return Forbid();
        if(enrolment.Event.EventDate.Date>DateTime.Today)return Conflict(new { message="Results can only be recorded after the event date." });
        if(enrolment.Status!="Confirmed")return Conflict(new { message="A cancelled enrolment cannot receive a result." });
        if(await _db.Results.AnyAsync(x=>x.EnrolmentID==enrolmentId))return Conflict(new { message="A result already exists for this enrolment." });
        if(request.Position.HasValue&&request.Position.Value<=0 || request.TotalFinishers.HasValue&&request.Position.HasValue&&request.TotalFinishers.Value<request.Position.Value || request.Points.HasValue&&request.Points.Value<0)return BadRequest(new { message="Position must be positive, total finishers cannot be below the position, and points cannot be negative." });
        var result=new Result { EnrolmentID=enrolmentId,FinishTime=request.FinishTime,Position=request.Position,TotalFinishers=request.TotalFinishers,Points=request.Points };
        _db.Results.Add(result); enrolment.Event.Status="Completed"; await _db.SaveChangesAsync(); return Created("/api/results",result);
    }
}
