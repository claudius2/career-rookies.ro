namespace CareerRookies.Web.Models;

public class WorkshopSpeaker
{
    public int WorkshopId { get; set; }
    public Workshop Workshop { get; set; } = null!;

    public int SpeakerId { get; set; }
    public Speaker Speaker { get; set; } = null!;

    public int SortOrder { get; set; }
}
