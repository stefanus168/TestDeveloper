namespace TestDeveloper.Domain;

public class PlanningSlot
{
    public int PlanningId { get; set; }

    public int SlotOrder { get; set; }

    public string SlotName { get; set; } = string.Empty;

    public int OriginalQuantity { get; set; }

    public int BalancedQuantity { get; set; }

    public bool IsActive { get; set; }

    public Planning Planning { get; set; } = null!;
}