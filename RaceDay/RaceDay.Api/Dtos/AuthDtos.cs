using System.ComponentModel.DataAnnotations;
namespace RaceDay.Api.Dtos;
public class RegisterDto
{
    [Required, MaxLength(50)] public string FirstName { get; set; } = "";
    [Required, MaxLength(50)] public string LastName { get; set; } = "";
    [Required, EmailAddress, MaxLength(100)] public string Email { get; set; } = "";
    [Required, MinLength(8), RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$", ErrorMessage="Use at least 8 characters with upper/lowercase letters, a number and a symbol.")] public string Password { get; set; } = "";
    [MaxLength(20)] public string? Phone { get; set; }
    public string Role { get; set; } = "Participant";
    public string? OrganizerInviteCode { get; set; }
}
public class LoginDto
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required] public string Password { get; set; } = "";
}
public class CreateEventDto
{
    [Required, MaxLength(100)] public string EventName { get; set; } = "";
    [Required, MaxLength(40)] public string EventType { get; set; } = "Road Race";
    public DateTime EventDate { get; set; }
    public TimeSpan StartTime { get; set; }
    [Required, MaxLength(150)] public string Venue { get; set; } = "";
    [MaxLength(500)] public string? Description { get; set; }
    [Range(1, 100000)] public int Capacity { get; set; }
}
public class CreateCategoryDto
{
    [Required, MaxLength(80)] public string CategoryName { get; set; } = "";
    [MaxLength(250)] public string? Description { get; set; }
}
public class CreateResultDto
{
    [Required] public TimeSpan? FinishTime { get; set; }
    [Required, Range(1, 1000000)] public int? Position { get; set; }
    [Required, Range(1, 10000000)] public int? TotalFinishers { get; set; }
    [Range(0, 1000000)] public int? Points { get; set; }
}
public class EnrolRequest
{
    [Range(1, int.MaxValue)] public int EventID { get; set; }
    [Range(1, int.MaxValue)] public int CategoryID { get; set; }
}
public class ProfileUpdateDto
{
    [Required, MaxLength(50)] public string FirstName { get; set; } = "";
    [Required, MaxLength(50)] public string LastName { get; set; } = "";
    [MaxLength(20)] public string? Phone { get; set; }
}
