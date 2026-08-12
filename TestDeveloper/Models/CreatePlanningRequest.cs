namespace TestDeveloper.Models;

public class CreatePlanningRequest
{
    public string RequestCode { get; set; } = string.Empty;

    public string CandidateToken { get; set; } = string.Empty;

    public List<PlanningSlotRequest> Slots { get; set; } = new();
}

public class PlanningSlotRequest
{
    public int SlotOrder { get; set; }

    public string SlotName { get; set; } = string.Empty;

    public int OriginalQuantity { get; set; }
}