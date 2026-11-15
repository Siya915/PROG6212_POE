using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
namespace RaceDay.Mvc.Controllers;
public class HomeController : Controller
{
    private readonly ApiClient _api;
    public HomeController(ApiClient api) { _api=api; }
    public async Task<IActionResult> Index(string? search,string? eventType,string? status)
    {
        var query=new List<string>();
        if(!string.IsNullOrWhiteSpace(search))query.Add("search="+Uri.EscapeDataString(search));
        if(!string.IsNullOrWhiteSpace(eventType))query.Add("eventType="+Uri.EscapeDataString(eventType));
        if(!string.IsNullOrWhiteSpace(status))query.Add("status="+Uri.EscapeDataString(status));
        var path="api/events"+(query.Count==0?"":"?"+string.Join("&",query));
        var result=await _api.GetAsync<List<EventDto>>(path);
        ViewBag.Search=search; ViewBag.EventType=eventType; ViewBag.Status=status; ViewBag.Error=result.Error;
        return View(result.Data??new List<EventDto>());
    }
    public IActionResult Error()=>View();
}
