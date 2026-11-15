using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
namespace RaceDay.Mvc.Controllers;
public class EventsController : Controller
{
    private readonly ApiClient _api; public EventsController(ApiClient api){_api=api;}
    public async Task<IActionResult> Index(string? search,string? eventType,string? status)
    {
        var query=new List<string>();
        if(!string.IsNullOrWhiteSpace(search))query.Add("search="+Uri.EscapeDataString(search));
        if(!string.IsNullOrWhiteSpace(eventType))query.Add("eventType="+Uri.EscapeDataString(eventType));
        if(!string.IsNullOrWhiteSpace(status))query.Add("status="+Uri.EscapeDataString(status));
        var path="api/events"+(query.Count==0?"":"?"+string.Join("&",query));
        var r=await _api.GetAsync<List<EventDto>>(path); ViewBag.Error=r.Error; ViewBag.Search=search; ViewBag.EventType=eventType; ViewBag.Status=status; return View(r.Data??new());
    }
    public async Task<IActionResult> Details(int id)
    { var r=await _api.GetAsync<EventDto>($"api/events/{id}"); if(!r.Success||r.Data==null)return NotFound(); return View(r.Data); }
    [HttpPost] public async Task<IActionResult> Enrol(EnrolViewModel model)
    {
        if(HttpContext.Session.GetString("Role")!="Participant")return string.IsNullOrWhiteSpace(HttpContext.Session.GetString("Role"))?RedirectToAction("Login","Auth"):View("AccessDenied");
        var r=await _api.PostAsync<CreateEnrolRequest,object>("api/enrolments",new(){EventID=model.EventID,CategoryID=model.CategoryID});
        TempData[r.Success?"Success":"Error"]=r.Success?"Your enrolment has been recorded.":r.Error; return RedirectToAction("Details",new{id=model.EventID});
    }
}
