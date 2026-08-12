using TestDeveloper.Domain;
using TestDeveloper.Persistence;

namespace TestDeveloper.Application;

public class PlanningService
{
    private readonly PlanningRepository _repository;

    public PlanningService(PlanningRepository repository)
    {
        _repository = repository;
    }

    public async Task<Planning> ProsesAsync(
        string requestCode,
        string candidateToken,
        List<PlanningSlot> slots)
    {
        Validasi(requestCode, candidateToken, slots);

        var rencana = slots
            .Select(x => x.OriginalQuantity)
            .ToList();

        var hasil = JadwalProduksi.Seimbangkan(rencana);

        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].BalancedQuantity = hasil[i];
            slots[i].IsActive = slots[i].OriginalQuantity > 0;
        }

        var planning = new Planning
        {
            RequestCode = requestCode,
            CandidateToken = candidateToken,
            CreatedAt = DateTime.UtcNow,
            Status = "SUCCESS",
            Slots = slots
        };

        await _repository.SimpanAsync(planning);

        return planning;
    }

    private static void Validasi(
        string requestCode,
        string candidateToken,
        List<PlanningSlot> slots)
    {
        if (string.IsNullOrWhiteSpace(requestCode))
            throw new ArgumentException(
                "RequestCode tidak boleh kosong");

        if (string.IsNullOrWhiteSpace(candidateToken))
            throw new ArgumentException(
                "CandidateToken tidak boleh kosong.");

        if (slots == null || slots.Count == 0)
            throw new ArgumentException(
                "Slot tidak boleh kosong.");

        foreach (var slot in slots)
        {
            if (slot.OriginalQuantity < 0)
            {
                throw new ArgumentException(
                    $"Quantity pada slot {slot.SlotName} harus bilangan bulat.");
            }
        }
    }
}