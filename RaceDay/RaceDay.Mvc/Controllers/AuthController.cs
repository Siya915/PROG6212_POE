using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
namespace RaceDay.Mvc.Controllers;
public class AuthController : Controller
{
    private readonly ApiClient _api; public AuthController(ApiClient api){_api=api;}
    [HttpGet] public IActionResult Login()=>View(new LoginViewModel());
    [HttpPost] public async Task<IActionResult> Login(LoginViewModel model)
    {
        if(!ModelState.IsValid)return View(model);
        var result=await _api.PostAsync<CreateLoginRequest,LoginResponse>("api/auth/login",new(){Email=model.Email,Password=model.Password});
        if(!result.Success||result.Data==null){ModelState.AddModelError("",result.Error??"Login failed.");return View(model);}
        HttpContext.Session.SetInt32("UserID",result.Data.UserID); HttpContext.Session.SetString("Role",result.Data.Role);
        HttpContext.Session.SetString("FullName",result.Data.FirstName+" "+result.Data.LastName);
        return RedirectToAction("Index",result.Data.Role=="Organiser"?"Organiser":"Participant");
    }
    [HttpGet] public IActionResult Register()=>View(new RegisterViewModel());
    [HttpPost] public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if(!ModelState.IsValid)return View(model);
        var result=await _api.PostAsync<CreateRegisterRequest,object>("api/auth/register",new(){FirstName=model.FirstName,LastName=model.LastName,Email=model.Email,Password=model.Password,Phone=model.Phone,Role=model.Role,OrganizerInviteCode=model.OrganizerInviteCode});
        if(!result.Success){ModelState.AddModelError("",result.Error??"Registration failed.");return View(model);}
        TempData["Success"]="Account created. You can now log in."; return RedirectToAction(nameof(Login));
    }
    [HttpPost] public async Task<IActionResult> Logout()
    {
        await _api.PostAsync<object,object>("api/auth/logout",new{}); HttpContext.Session.Clear(); return RedirectToAction("Index","Home");
    }
}
