using System.ComponentModel.DataAnnotations;
namespace RaceDay.Api.Models;
public class User
{
    public int UserID { get; set; }
    [Required, MaxLength(50)] public string FirstName { get; set; } = "";
    [Required, MaxLength(50)] public string LastName { get; set; } = "";
    [Required, EmailAddress, MaxLength(100)] public string Email { get; set; } = "";
    [Required, MaxLength(255)] public string PasswordHash { get; set; } = "";
    [MaxLength(20)] public string? Phone { get; set; }
    [Required, MaxLength(20)] public string Role { get; set; } = "Participant";
    [MaxLength(500)] public string? ProfilePictureUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public List<Event> OrganisedEvents { get; set; } = new();
}
