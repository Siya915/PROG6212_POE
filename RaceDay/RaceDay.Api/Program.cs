using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Api.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<RaceDayContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("RaceDayConnection")));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".RaceDay.Api.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});
builder.Services.AddScoped<IImageStorageService, ImageStorageService>();

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(handler => handler.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new { message = "An unexpected server error occurred. Please try again." });
    }));
}
if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Swagger:Enabled")) { app.UseSwagger(); app.UseSwaggerUI(); }
if (!app.Environment.IsDevelopment() && !app.Configuration.GetValue<bool>("Docker:DisableHttpsRedirection")) app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.MapControllers();

if (app.Configuration.GetValue<bool>("Database:CreateOnStartup"))
{
    // SQL Server in Docker can take a short time to start. Retry instead of failing immediately.
    Exception? lastError = null;
    for (var attempt = 1; attempt <= 20; attempt++)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<RaceDayContext>();
            db.Database.EnsureCreated();
            SeedData(db);
            lastError = null;
            break;
        }
        catch (Exception ex)
        {
            lastError = ex;
            await Task.Delay(TimeSpan.FromSeconds(3));
        }
    }
    if (lastError != null) throw new InvalidOperationException("RaceDay could not connect to SQL Server after 20 attempts. Check the connection string and SQL Server status.", lastError);
}
app.Run();

static void SeedData(RaceDayContext db)
{
    if (!db.Users.Any())
    {
        db.Users.AddRange(
            new User { FirstName="Amanda", LastName="Mokoena", Email="amanda@raceday.co.za", PasswordHash=BCrypt.Net.BCrypt.HashPassword("Organizer123!"), Phone="0712345678", Role="Organiser" },
            new User { FirstName="Brian", LastName="Nkosi", Email="brian@raceday.co.za", PasswordHash=BCrypt.Net.BCrypt.HashPassword("Organizer456!"), Phone="0723456789", Role="Organiser" },
            new User { FirstName="Siyabonga", LastName="Madiba", Email="siya@example.com", PasswordHash=BCrypt.Net.BCrypt.HashPassword("Participant123!"), Phone="0734567890", Role="Participant" },
            new User { FirstName="Lerato", LastName="Khumalo", Email="lerato@example.com", PasswordHash=BCrypt.Net.BCrypt.HashPassword("Participant456!"), Phone="0745678901", Role="Participant" });
        db.SaveChanges();
    }
    if (!db.Categories.Any())
    {
        db.Categories.AddRange(
            new Category { CategoryName="5 km Fun Run", Description="Short community running event" },
            new Category { CategoryName="10 km Road Race", Description="Competitive 10 kilometre road race" },
            new Category { CategoryName="21 km Half Marathon", Description="Half marathon road race" });
        db.SaveChanges();
    }
    if (!db.Events.Any())
    {
        db.Events.AddRange(
            new Event { OrganizerID=1, EventName="Kgapane Community Run", EventType="Fun Run", EventDate=new DateTime(2026,10,10), StartTime=TimeSpan.FromHours(7), Venue="Kgapane Stadium", Description="A friendly community race for all fitness levels.", Capacity=500, Status="Completed" },
            new Event { OrganizerID=2, EventName="Polokwane Spring 10K", EventType="Road Race", EventDate=new DateTime(2027,4,10), StartTime=TimeSpan.FromHours(6.5), Venue="Peter Mokaba Stadium", Description="A timed 10 kilometre road race.", Capacity=750, Status="Open" },
            new Event { OrganizerID=1, EventName="Limpopo Half Marathon", EventType="Half Marathon", EventDate=new DateTime(2027,5,15), StartTime=TimeSpan.FromHours(6), Venue="Polokwane Civic Centre", Description="A challenging half marathon event.", Capacity=1000, Status="Open" });
        db.SaveChanges();
    }
    if (!db.EventCategories.Any())
    {
        db.EventCategories.AddRange(new EventCategory { EventID=1, CategoryID=1 }, new EventCategory { EventID=2, CategoryID=1 }, new EventCategory { EventID=2, CategoryID=2 }, new EventCategory { EventID=3, CategoryID=2 }, new EventCategory { EventID=3, CategoryID=3 });
        db.SaveChanges();
    }
    if (!db.Enrolments.Any())
    {
        db.Enrolments.AddRange(new Enrolment { EventID=1, ParticipantID=3, CategoryID=1, Status="Confirmed" }, new Enrolment { EventID=2, ParticipantID=3, CategoryID=2, Status="Confirmed" }, new Enrolment { EventID=2, ParticipantID=4, CategoryID=1, Status="Confirmed" });
        db.SaveChanges();
    }
    if (!db.Results.Any())
    {
        db.Results.Add(new Result { EnrolmentID=1, FinishTime=TimeSpan.FromMinutes(28)+TimeSpan.FromSeconds(42), Position=4, TotalFinishers=120, Points=80 });
        db.SaveChanges();
    }
}

public partial class Program { }
