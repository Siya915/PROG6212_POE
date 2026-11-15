using System.ComponentModel.DataAnnotations;
namespace RaceDay.Api.Models;
public class Event
{
    public int EventID { get; set; }
    public int OrganizerID { get; set; }
    [Required, MaxLength(100)] public string EventName { get; set; } = "";
    [Required, MaxLength(40)] public string EventType { get; set; } = "Road Race";
    public DateTime EventDate { get; set; }
    public TimeSpan StartTime { get; set; }
    [Required, MaxLength(150)] public string Venue { get; set; } = "";
    [MaxLength(500)] public string? Description { get; set; }
    public int Capacity { get; set; }
    [MaxLength(20)] public string Status { get; set; } = "Open";
    [MaxLength(500)] public string? BannerImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public User? Organizer { get; set; }
    public List<EventCategory> EventCategories { get; set; } = new();
    public List<Enrolment> Enrolments { get; set; } = new();
}
