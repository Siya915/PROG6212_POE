using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
namespace RaceDay.Mvc.Controllers;
public class ParticipantController : Controller
{
    private readonly ApiClient _api; public ParticipantController(ApiClient api){_api=api;}
    private bool Allowed()=>HttpContext.Session.GetString("Role")=="Participant";
    private IActionResult Denied()=>string.IsNullOrWhiteSpace(HttpContext.Session.GetString("Role"))?RedirectToAction("Login","Auth"):View("AccessDenied");
    public async Task<IActionResult> Index(){if(!Allowed())return Denied();var enrol=await _api.GetAsync<List<EnrolmentDto>>("api/enrolments");var events=await _api.GetAsync<List<EventDto>>("api/events");ViewBag.Enrolments=enrol.Data??new();ViewBag.Events=events.Data??new();return View(enrol.Data??new());}
    public async Task<IActionResult> MyEvents(){if(!Allowed())return Denied();var r=await _api.GetAsync<List<EnrolmentDto>>("api/enrolments");ViewBag.Error=r.Error;return View(r.Data??new());}
    public async Task<IActionResult> Results(){if(!Allowed())return Denied();var r=await _api.GetAsync<List<ResultDto>>("api/results");ViewBag.Error=r.Error;return View(r.Data??new());}
    [HttpPost] public async Task<IActionResult> CancelEnrolment(int id){if(!Allowed())return Denied();var r=await _api.DeleteAsync($"api/enrolments/{id}");TempData[r.Success?"Success":"Error"]=r.Success?"Enrolment cancelled.":r.Error;return RedirectToAction(nameof(MyEvents));}
    [HttpGet] public async Task<IActionResult> Profile(){if(!Allowed())return Denied();var r=await _api.GetAsync<ProfileDto>("api/users/profile");if(!r.Success||r.Data==null)return RedirectToAction("Login","Auth");return View(new ProfileViewModel{FirstName=r.Data.FirstName,LastName=r.Data.LastName,Email=r.Data.Email,Phone=r.Data.Phone,Role=r.Data.Role,ProfilePictureUrl=r.Data.ProfilePictureUrl});}
    [HttpPost] public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        if(!Allowed())return Denied();
        if(!ModelState.IsValid){return View(model);}
        var update=await _api.PutAsync<CreateProfileRequest,ApiMessage>("api/users/profile",new(){FirstName=model.FirstName,LastName=model.LastName,Phone=model.Phone});
        if(!update.Success){ModelState.AddModelError("",update.Error??"Profile update failed.");return View(model);}
        if(model.Picture!=null&&model.Picture.Length>0){var image=await _api.UploadAsync<ApiMessage>("api/files/profile-picture",model.Picture);if(!image.Success)TempData["Error"]="Profile updated, but picture upload failed: "+image.Error;else model.ProfilePictureUrl=image.Data?.ImageUrl;}
        TempData["Success"]="Profile updated.";return RedirectToAction(nameof(Profile));
    }
}
