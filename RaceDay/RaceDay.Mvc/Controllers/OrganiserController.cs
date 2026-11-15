using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
namespace RaceDay.Mvc.Controllers;
public class OrganiserController : Controller
{
    private readonly ApiClient _api; public OrganiserController(ApiClient api){_api=api;}
    private bool Allowed()=>HttpContext.Session.GetString("Role")=="Organiser";
    private IActionResult Denied()=>string.IsNullOrWhiteSpace(HttpContext.Session.GetString("Role"))?RedirectToAction("Login","Auth"):View("AccessDenied");
    public async Task<IActionResult> Index()
    {
        if(!Allowed())return Denied(); var events=await _api.GetAsync<List<EventDto>>("api/events/mine"); var enrol=await _api.GetAsync<List<EnrolmentDto>>("api/enrolments");
        ViewBag.Enrolments=enrol.Data??new(); ViewBag.Error=events.Error; return View(events.Data??new());
    }
    public async Task<IActionResult> Events(){if(!Allowed())return Denied();var r=await _api.GetAsync<List<EventDto>>("api/events/mine");ViewBag.Error=r.Error;return View(r.Data??new());}
    [HttpGet] public IActionResult CreateEvent(){if(!Allowed())return Denied();return View("EventForm",new EventFormViewModel());}
    [HttpPost] public async Task<IActionResult> CreateEvent(EventFormViewModel model)
    {
        if(!Allowed())return Denied(); if(!ModelState.IsValid)return View("EventForm",model);
        var r=await _api.PostAsync<CreateEventRequest,EventDto>("api/events",ToRequest(model));
        if(!r.Success||r.Data==null){ModelState.AddModelError("",r.Error??"Could not create event.");return View("EventForm",model);}
        if(model.BannerImage!=null&&model.BannerImage.Length>0){var upload=await _api.UploadAsync<ApiMessage>($"api/files/events/{r.Data.EventID}/banner",model.BannerImage);if(!upload.Success)TempData["Error"]="Event created, but banner upload failed: "+upload.Error;}
        TempData["Success"]="Event created successfully."; return RedirectToAction(nameof(Events));
    }
    [HttpGet] public async Task<IActionResult> EditEvent(int id)
    {
        if(!Allowed())return Denied();var r=await _api.GetAsync<EventDto>($"api/events/{id}");if(!r.Success||r.Data==null)return NotFound();
        return View("EventForm",new EventFormViewModel{EventID=id,EventName=r.Data.EventName,EventType=r.Data.EventType,EventDate=r.Data.EventDate,StartTime=r.Data.StartTime,Venue=r.Data.Venue,Description=r.Data.Description,Capacity=r.Data.Capacity});
    }
    [HttpPost] public async Task<IActionResult> EditEvent(EventFormViewModel model)
    {
        if(!Allowed())return Denied();if(!ModelState.IsValid)return View("EventForm",model);
        var r=await _api.PutAsync<CreateEventRequest,ApiMessage>($"api/events/{model.EventID}",ToRequest(model));
        if(!r.Success){ModelState.AddModelError("",r.Error??"Could not update event.");return View("EventForm",model);}
        if(model.BannerImage!=null&&model.BannerImage.Length>0){var upload=await _api.UploadAsync<ApiMessage>($"api/files/events/{model.EventID}/banner",model.BannerImage);if(!upload.Success)TempData["Error"]="Event updated, but banner upload failed: "+upload.Error;}
        TempData["Success"]="Event updated.";return RedirectToAction(nameof(Events));
    }
    [HttpPost] public async Task<IActionResult> DeleteEvent(int id){if(!Allowed())return Denied();var r=await _api.DeleteAsync($"api/events/{id}");TempData[r.Success?"Success":"Error"]=r.Success?"Event deleted.":r.Error;return RedirectToAction(nameof(Events));}

    public async Task<IActionResult> Categories(){if(!Allowed())return Denied();var r=await _api.GetAsync<List<CategoryDto>>("api/categories");ViewBag.Error=r.Error;return View(r.Data??new());}
    [HttpPost] public async Task<IActionResult> CreateCategory(CategoryFormViewModel model){if(!Allowed())return Denied();if(!ModelState.IsValid)return RedirectToAction(nameof(Categories));var r=await _api.PostAsync<CreateCategoryRequest,CategoryDto>("api/categories",new(){CategoryName=model.CategoryName,Description=model.Description});TempData[r.Success?"Success":"Error"]=r.Success?"Category added.":r.Error;return RedirectToAction(nameof(Categories));}
    [HttpPost] public async Task<IActionResult> DeleteCategory(int id){if(!Allowed())return Denied();var r=await _api.DeleteAsync($"api/categories/{id}");TempData[r.Success?"Success":"Error"]=r.Success?"Category deleted.":r.Error;return RedirectToAction(nameof(Categories));}
    public async Task<IActionResult> ManageEventCategories(int id)
    {
        if(!Allowed())return Denied();
        var eventResult=await _api.GetAsync<EventDto>($"api/events/{id}");
        if(!eventResult.Success||eventResult.Data==null)return NotFound();
        var categoryResult=await _api.GetAsync<List<CategoryDto>>("api/categories");
        ViewBag.Event=eventResult.Data; ViewBag.AllCategories=categoryResult.Data??new();
        return View(eventResult.Data.Categories);
    }
    [HttpPost] public async Task<IActionResult> AssignCategory(int eventId,int categoryId){if(!Allowed())return Denied();var r=await _api.PostAsync<object,ApiMessage>($"api/events/{eventId}/categories/{categoryId}",new{});TempData[r.Success?"Success":"Error"]=r.Success?"Category assigned to event.":r.Error;return RedirectToAction(nameof(ManageEventCategories),new{id=eventId});}
    [HttpPost] public async Task<IActionResult> RemoveEventCategory(int eventId,int categoryId){if(!Allowed())return Denied();var r=await _api.DeleteAsync($"api/events/{eventId}/categories/{categoryId}");TempData[r.Success?"Success":"Error"]=r.Success?"Category removed from event.":r.Error;return RedirectToAction(nameof(ManageEventCategories),new{id=eventId});}

    public async Task<IActionResult> Enrolments(){if(!Allowed())return Denied();var r=await _api.GetAsync<List<EnrolmentDto>>("api/enrolments");ViewBag.Error=r.Error;return View(r.Data??new());}
    [HttpPost] public async Task<IActionResult> CancelEnrolment(int id){if(!Allowed())return Denied();var r=await _api.DeleteAsync($"api/enrolments/{id}");TempData[r.Success?"Success":"Error"]=r.Success?"Enrolment cancelled.":r.Error;return RedirectToAction(nameof(Enrolments));}
    public async Task<IActionResult> Results(){if(!Allowed())return Denied();var r=await _api.GetAsync<List<ResultDto>>("api/results");ViewBag.Error=r.Error;return View(r.Data??new());}
    [HttpGet] public async Task<IActionResult> AddResult(int enrolmentId){if(!Allowed())return Denied();var enrol=await _api.GetAsync<List<EnrolmentDto>>("api/enrolments");var selected=enrol.Data?.FirstOrDefault(x=>x.EnrolmentID==enrolmentId);if(selected==null)return NotFound();ViewBag.Enrolment=selected;return View(new ResultFormViewModel{EnrolmentID=enrolmentId});}
    [HttpPost] public async Task<IActionResult> AddResult(ResultFormViewModel model){if(!Allowed())return Denied();if(!ModelState.IsValid){var list=await _api.GetAsync<List<EnrolmentDto>>("api/enrolments");ViewBag.Enrolment=list.Data?.FirstOrDefault(x=>x.EnrolmentID==model.EnrolmentID);return View(model);}var r=await _api.PostAsync<CreateResultRequest,ApiMessage>($"api/results/{model.EnrolmentID}",new(){FinishTime=model.FinishTime,Position=model.Position,TotalFinishers=model.TotalFinishers,Points=model.Points});TempData[r.Success?"Success":"Error"]=r.Success?"Result recorded.":r.Error;return RedirectToAction(nameof(Enrolments));}
    private static CreateEventRequest ToRequest(EventFormViewModel m)=>new(){EventName=m.EventName,EventType=m.EventType,EventDate=m.EventDate,StartTime=m.StartTime,Venue=m.Venue,Description=m.Description,Capacity=m.Capacity};
}
