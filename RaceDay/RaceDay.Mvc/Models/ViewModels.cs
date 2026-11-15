using System.ComponentModel.DataAnnotations;
namespace RaceDay.Mvc.Models;
public class RegisterViewModel
{
    [Required,MaxLength(50)] public string FirstName { get; set; }="";
    [Required,MaxLength(50)] public string LastName { get; set; }="";
    [Required,EmailAddress] public string Email { get; set; }="";
    [Required,MinLength(8),DataType(DataType.Password)] public string Password { get; set; }="";
    [MaxLength(20)] public string? Phone { get; set; }
    [Required] public string Role { get; set; }="Participant";
    [DataType(DataType.Password)] public string? OrganizerInviteCode { get; set; }
}
public class LoginViewModel { [Required,EmailAddress] public string Email { get; set; }=""; [Required,DataType(DataType.Password)] public string Password { get; set; }=""; }
public class EventFormViewModel
{
    public int EventID { get; set; }
    [Required,MaxLength(100)] public string EventName { get; set; }="";
    [Required,MaxLength(40)] public string EventType { get; set; }="Road Race";
    [DataType(DataType.Date)] public DateTime EventDate { get; set; }=DateTime.Today.AddDays(30);
    [DataType(DataType.Time)] public TimeSpan StartTime { get; set; }=TimeSpan.FromHours(7);
    [Required,MaxLength(150)] public string Venue { get; set; }="";
    [MaxLength(500)] public string? Description { get; set; }
    [Range(1,100000)] public int Capacity { get; set; }=100;
    public IFormFile? BannerImage { get; set; }
}
public class CategoryFormViewModel { public int CategoryID { get; set; } [Required,MaxLength(80)] public string CategoryName { get; set; }=""; [MaxLength(250)] public string? Description { get; set; } }
public class ProfileViewModel { [Required,MaxLength(50)] public string FirstName { get; set; }=""; [Required,MaxLength(50)] public string LastName { get; set; }=""; [EmailAddress] public string Email { get; set; }=""; [MaxLength(20)] public string? Phone { get; set; } public string Role { get; set; }=""; public string? ProfilePictureUrl { get; set; } public IFormFile? Picture { get; set; } }
public class EnrolViewModel { public int EventID { get; set; } [Required] public int CategoryID { get; set; } }
public class ResultFormViewModel { public int EnrolmentID { get; set; } [Required,DataType(DataType.Time)] public TimeSpan? FinishTime { get; set; } [Required,Range(1,1000000)] public int? Position { get; set; } [Required,Range(1,10000000)] public int? TotalFinishers { get; set; } [Range(0,1000000)] public int? Points { get; set; } }
public class EventDto
{
    public int EventID { get; set; } public int OrganizerID { get; set; } public string EventName { get; set; }=""; public string EventType { get; set; }="Road Race"; public DateTime EventDate { get; set; } public TimeSpan StartTime { get; set; }
    public string Venue { get; set; }=""; public string? Description { get; set; } public int Capacity { get; set; } public string Status { get; set; }="Open"; public string? BannerImageUrl { get; set; } public int EnrolmentCount { get; set; } public List<CategoryDto> Categories { get; set; }=new();
}
public class CategoryDto { public int CategoryID { get; set; } public string CategoryName { get; set; }=""; public string? Description { get; set; } }
public class EnrolmentDto { public int EnrolmentID { get; set; } public int EventID { get; set; } public string EventName { get; set; }=""; public DateTime EventDate { get; set; } public int ParticipantID { get; set; } public string? ParticipantName { get; set; } public int CategoryID { get; set; } public string CategoryName { get; set; }=""; public DateTime EnrolmentDate { get; set; } public string Status { get; set; }=""; public bool HasResult { get; set; } }
public class ResultDto { public int ResultID { get; set; } public int EnrolmentID { get; set; } public int EventID { get; set; } public string EventName { get; set; }=""; public string? ParticipantName { get; set; } public string CategoryName { get; set; }=""; public TimeSpan? FinishTime { get; set; } public int? Position { get; set; } public int? TotalFinishers { get; set; } public int? Points { get; set; } public DateTime EventDate { get; set; } public DateTime RecordedAt { get; set; } public string PositionLabel { get { if(!Position.HasValue)return "—"; var p=Position.Value; var suffix=(p%100>=11&&p%100<=13)?"th":(p%10==1?"st":p%10==2?"nd":p%10==3?"rd":"th"); return $"{p}{suffix}"+(TotalFinishers.HasValue?$" out of {TotalFinishers.Value} finishers":""); } } }
public class ProfileDto { public int UserID { get; set; } public string FirstName { get; set; }=""; public string LastName { get; set; }=""; public string Email { get; set; }=""; public string? Phone { get; set; } public string Role { get; set; }=""; public string? ProfilePictureUrl { get; set; } }
public class LoginResponse { public int UserID { get; set; } public string FirstName { get; set; }=""; public string LastName { get; set; }=""; public string Email { get; set; }=""; public string Role { get; set; }=""; public string Message { get; set; }=""; }
public class ApiMessage { public string Message { get; set; }=""; public string? ImageUrl { get; set; } }
public class CreateEventRequest { public string EventName { get; set; }=""; public string EventType { get; set; }="Road Race"; public DateTime EventDate { get; set; } public TimeSpan StartTime { get; set; } public string Venue { get; set; }=""; public string? Description { get; set; } public int Capacity { get; set; } }
public class CreateCategoryRequest { public string CategoryName { get; set; }=""; public string? Description { get; set; } }
public class CreateResultRequest { public TimeSpan? FinishTime { get; set; } public int? Position { get; set; } public int? TotalFinishers { get; set; } public int? Points { get; set; } }
public class CreateEnrolRequest { public int EventID { get; set; } public int CategoryID { get; set; } }
public class CreateProfileRequest { public string FirstName { get; set; }=""; public string LastName { get; set; }=""; public string? Phone { get; set; } }
public class CreateRegisterRequest { public string FirstName { get; set; }=""; public string LastName { get; set; }=""; public string Email { get; set; }=""; public string Password { get; set; }=""; public string? Phone { get; set; } public string Role { get; set; }="Participant"; public string? OrganizerInviteCode { get; set; } }
public class CreateLoginRequest { public string Email { get; set; }=""; public string Password { get; set; }=""; }
