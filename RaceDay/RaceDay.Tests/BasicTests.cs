using System.Net;
using System.Net.Http.Json;
using BCrypt.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RaceDay.Api.Data;
using Xunit;

namespace RaceDay.Tests;

public class BasicTests
{
    [Fact]
    public void PasswordHash_IsNotTheOriginalPassword()
    {
        const string password = "Participant123!";
        string hash = BCrypt.HashPassword(password);
        Assert.NotEqual(password, hash);
        Assert.True(BCrypt.Verify(password, hash));
    }

    [Theory]
    [InlineData("Participant", true)]
    [InlineData("Organiser", true)]
    [InlineData("Administrator", false)]
    [InlineData("", false)]
    public void OnlySupportedRolesAreAllowed(string role, bool expected)
    {
        bool isAllowed = role == "Participant" || role == "Organiser";
        Assert.Equal(expected, isAllowed);
    }

    [Fact]
    public async Task ParticipantCanRegisterAndLoginButCannotCreateEvent()
    {
        using var factory = new RaceDayApiFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });

        var registration = await client.PostAsJsonAsync("/api/auth/register", new
        {
            FirstName = "Test", LastName = "Runner", Email = "runner@example.com",
            Password = "Runner123!", Phone = "0711111111", Role = "Participant"
        });
        Assert.Equal(HttpStatusCode.Created, registration.StatusCode);

        var login = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "runner@example.com", Password = "Runner123!"
        });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        var createEvent = await client.PostAsJsonAsync("/api/events", new
        {
            EventName = "Not Allowed Run", EventDate = DateTime.Today.AddDays(30),
            StartTime = TimeSpan.FromHours(7), Venue = "Test Park", Description = "Test",
            Capacity = 100
        });
        Assert.Equal(HttpStatusCode.Forbidden, createEvent.StatusCode);
    }

    [Fact]
    public async Task OrganiserNeedsInviteCodeAndCanCreateOwnEvent()
    {
        using var factory = new RaceDayApiFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });

        var rejected = await client.PostAsJsonAsync("/api/auth/register", new
        {
            FirstName = "Test", LastName = "Organiser", Email = "bad-organiser@example.com",
            Password = "Organiser123!", Role = "Organiser", OrganizerInviteCode = "wrong-code"
        });
        Assert.Equal(HttpStatusCode.Forbidden, rejected.StatusCode);

        var registration = await client.PostAsJsonAsync("/api/auth/register", new
        {
            FirstName = "Test", LastName = "Organiser", Email = "organiser@example.com",
            Password = "Organiser123!", Role = "Organiser", OrganizerInviteCode = "test-invite"
        });
        Assert.Equal(HttpStatusCode.Created, registration.StatusCode);

        var login = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "organiser@example.com", Password = "Organiser123!"
        });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        var created = await client.PostAsJsonAsync("/api/events", new
        {
            EventName = "Test Community Race", EventDate = DateTime.Today.AddDays(40),
            StartTime = TimeSpan.FromHours(7), Venue = "Community Stadium", Description = "API test event",
            Capacity = 250
        });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        var mine = await client.GetAsync("/api/events/mine");
        Assert.Equal(HttpStatusCode.OK, mine.StatusCode);
    }
}

public class RaceDayApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = "RaceDayTests_" + Guid.NewGuid().ToString("N");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:CreateOnStartup"] = "false",
                ["Security:OrganizerInviteCode"] = "test-invite",
                ["Docker:DisableHttpsRedirection"] = "true"
            });
        });
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<RaceDayContext>>();
            services.RemoveAll<RaceDayContext>();
            services.AddDbContext<RaceDayContext>(options => options.UseInMemoryDatabase(_databaseName));
        });
    }
}
