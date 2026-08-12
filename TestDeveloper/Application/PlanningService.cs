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

        // Check Idempotency: Jika RequestCode sudah pernah diproses, kembalikan data eksisting (tidak membuat duplikat)
        var existing = await _repository.GetByRequestCodeAsync(requestCode);
        if (existing != null)
        {
            return existing;
        }

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

    public async Task<List<Planning>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Planning?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<Planning?> GetByRequestCodeAsync(string requestCode)
    {
        return await _repository.GetByRequestCodeAsync(requestCode);
    }

    private static void Validasi(
        string requestCode,
        string candidateToken,
        List<PlanningSlot> slots)
    {
        if (string.IsNullOrWhiteSpace(requestCode))
            throw new ArgumentException("RequestCode tidak boleh kosong.");

        if (string.IsNullOrWhiteSpace(candidateToken))
            throw new ArgumentException("CandidateToken tidak boleh kosong.");

        if (slots == null || slots.Count == 0)
            throw new ArgumentException("Slot tidak boleh kosong.");

        var usedOrders = new HashSet<int>();

        foreach (var slot in slots)
        {
            if (string.IsNullOrWhiteSpace(slot.SlotName))
            {
                throw new ArgumentException($"Nama slot tidak boleh kosong (SlotOrder: {slot.SlotOrder}).");
            }

            if (slot.OriginalQuantity < 0)
            {
                throw new ArgumentException($"Quantity pada slot '{slot.SlotName}' tidak boleh negatif.");
            }

            if (usedOrders.Contains(slot.SlotOrder))
            {
                throw new ArgumentException($"SlotOrder {slot.SlotOrder} terdeteksi duplikat.");
            }

            usedOrders.Add(slot.SlotOrder);
        }
    }
}