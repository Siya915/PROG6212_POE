using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
namespace RaceDay.Api.Controllers;
[ApiController, Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayContext _db;
    public CategoriesController(RaceDayContext db) { _db = db; }
    private bool IsOrganiser => HttpContext.Session.GetString("Role") == "Organiser";
    [HttpGet] public async Task<IActionResult> GetAll() => Ok(await _db.Categories.OrderBy(x=>x.CategoryName).ToListAsync());
    [HttpGet("{id:int}")] public async Task<IActionResult> Get(int id) { var item=await _db.Categories.FindAsync(id); return item==null?NotFound():Ok(item); }
    [HttpPost] public async Task<IActionResult> Create(CreateCategoryDto request)
    {
        if (HttpContext.Session.GetInt32("UserID") == null) return Unauthorized(); if (!IsOrganiser) return Forbid();
        if (await _db.Categories.AnyAsync(x=>x.CategoryName==request.CategoryName.Trim())) return Conflict(new { message="Category already exists." });
        var item=new Category { CategoryName=request.CategoryName.Trim(), Description=request.Description?.Trim() };
        _db.Categories.Add(item); await _db.SaveChangesAsync(); return CreatedAtAction(nameof(Get),new { id=item.CategoryID },item);
    }
    [HttpPut("{id:int}")] public async Task<IActionResult> Update(int id, CreateCategoryDto request)
    {
        if (HttpContext.Session.GetInt32("UserID") == null) return Unauthorized(); if (!IsOrganiser) return Forbid();
        var item=await _db.Categories.FindAsync(id); if(item==null)return NotFound();
        if(await _db.Categories.AnyAsync(x=>x.CategoryID!=id&&x.CategoryName==request.CategoryName.Trim()))return Conflict(new { message="Category name already exists." });
        item.CategoryName=request.CategoryName.Trim(); item.Description=request.Description?.Trim(); await _db.SaveChangesAsync(); return Ok(item);
    }
    [HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int id)
    {
        if (HttpContext.Session.GetInt32("UserID") == null) return Unauthorized(); if (!IsOrganiser) return Forbid();
        var item=await _db.Categories.Include(x=>x.EventCategories).Include(x=>x.Enrolments).FirstOrDefaultAsync(x=>x.CategoryID==id);
        if(item==null)return NotFound(); if(item.EventCategories.Any() || item.Enrolments.Any()) return Conflict(new { message="This category is already in use and cannot be deleted." });
        _db.Categories.Remove(item); await _db.SaveChangesAsync(); return NoContent();
    }
}
