using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;
namespace RaceDay.Api.Data;
public class RaceDayContext : DbContext
{
    public RaceDayContext(DbContextOptions<RaceDayContext> options) : base(options) { }
    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<EventCategory> EventCategories => Set<EventCategory>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<User>().Property(x => x.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");
        modelBuilder.Entity<User>().ToTable(t => t.HasCheckConstraint("CK_Users_Role", "[Role] IN ('Participant','Organiser')"));

        modelBuilder.Entity<Event>().Property(x => x.EventDate).HasColumnType("date");
        modelBuilder.Entity<Event>().Property(x => x.StartTime).HasColumnType("time(0)");
        modelBuilder.Entity<Event>().Property(x => x.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");
        modelBuilder.Entity<Event>().Property(x => x.Status).HasDefaultValue("Open");
        modelBuilder.Entity<Event>().Property(x => x.EventType).HasDefaultValue("Road Race");
        modelBuilder.Entity<Event>().ToTable(t => {
            t.HasCheckConstraint("CK_Events_Capacity", "[Capacity] > 0");
            t.HasCheckConstraint("CK_Events_Status", "[Status] IN ('Open','Closed','Completed')");
        });
        modelBuilder.Entity<Event>().HasOne(x => x.Organizer).WithMany(x => x.OrganisedEvents)
            .HasForeignKey(x => x.OrganizerID).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Category>().HasIndex(x => x.CategoryName).IsUnique();
        modelBuilder.Entity<EventCategory>().HasKey(x => new { x.EventID, x.CategoryID });
        modelBuilder.Entity<EventCategory>().HasOne(x => x.Event).WithMany(x => x.EventCategories).HasForeignKey(x => x.EventID);
        modelBuilder.Entity<EventCategory>().HasOne(x => x.Category).WithMany(x => x.EventCategories).HasForeignKey(x => x.CategoryID);

        modelBuilder.Entity<Enrolment>().HasIndex(x => new { x.EventID, x.ParticipantID }).IsUnique();
        modelBuilder.Entity<Enrolment>().Property(x => x.EnrolmentDate).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");
        modelBuilder.Entity<Enrolment>().Property(x => x.Status).HasDefaultValue("Confirmed");
        modelBuilder.Entity<Enrolment>().ToTable(t => t.HasCheckConstraint("CK_Enrolments_Status", "[Status] IN ('Confirmed','Cancelled')"));
        modelBuilder.Entity<Enrolment>().HasOne(x => x.Event).WithMany(x => x.Enrolments).HasForeignKey(x => x.EventID);
        modelBuilder.Entity<Enrolment>().HasOne(x => x.Participant).WithMany().HasForeignKey(x => x.ParticipantID).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Enrolment>().HasOne(x => x.Category).WithMany(x => x.Enrolments).HasForeignKey(x => x.CategoryID).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Result>().HasIndex(x => x.EnrolmentID).IsUnique();
        modelBuilder.Entity<Result>().Property(x => x.RecordedAt).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");
        modelBuilder.Entity<Result>().Property(x => x.FinishTime).HasColumnType("time(0)");
        modelBuilder.Entity<Result>().ToTable(t => {
            t.HasCheckConstraint("CK_Results_Position", "[Position] IS NULL OR [Position] > 0");
            t.HasCheckConstraint("CK_Results_TotalFinishers", "[TotalFinishers] IS NULL OR [TotalFinishers] >= [Position]");
            t.HasCheckConstraint("CK_Results_Points", "[Points] IS NULL OR [Points] >= 0");
        });
        modelBuilder.Entity<Result>().HasOne(x => x.Enrolment).WithOne(x => x.Result).HasForeignKey<Result>(x => x.EnrolmentID);
    }
}
