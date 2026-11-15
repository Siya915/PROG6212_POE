namespace RaceDay.Api.Models;
public class Result
{
    public int ResultID { get; set; }
    public int EnrolmentID { get; set; }
    public TimeSpan? FinishTime { get; set; }
    public int? Position { get; set; }
    public int? TotalFinishers { get; set; }
    public int? Points { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.Now;
    public Enrolment? Enrolment { get; set; }
}
