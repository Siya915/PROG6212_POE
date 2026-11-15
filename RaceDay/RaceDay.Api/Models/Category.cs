using System.ComponentModel.DataAnnotations;
namespace RaceDay.Api.Models;
public class Category
{
    public int CategoryID { get; set; }
    [Required, MaxLength(80)] public string CategoryName { get; set; } = "";
    [MaxLength(250)] public string? Description { get; set; }
    public List<EventCategory> EventCategories { get; set; } = new();
    public List<Enrolment> Enrolments { get; set; } = new();
}
