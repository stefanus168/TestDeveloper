namespace TestDeveloper.Domain;

public class Planning
{
    public int PlanningId { get; set; }

    public string RequestCode { get; set; } = string.Empty;

    public string CandidateToken { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public string Status { get; set; } = string.Empty;

    public List<PlanningSlot> Slots { get; set; } = new();
}