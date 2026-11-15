namespace RaceDay.Api.Models;
public class EventCategory
{
    public int EventID { get; set; }
    public int CategoryID { get; set; }
    public Event? Event { get; set; }
    public Category? Category { get; set; }
}
