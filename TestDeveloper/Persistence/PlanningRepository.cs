using System.Collections.Concurrent;
using Microsoft.Data.SqlClient;
using TestDeveloper.Domain;

namespace TestDeveloper.Persistence;

public class PlanningRepository
{
    private readonly string _connectionString;
    private static readonly ConcurrentBag<Planning> _inMemoryStore = new();
    private static int _nextInMemoryId = 1;

    public PlanningRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task SimpanAsync(Planning planning)
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            SimpanInMemory(planning);
            return;
        }

        try
        {
            using var koneksi = new SqlConnection(_connectionString);
            await koneksi.OpenAsync();
            using var transaksi = await koneksi.BeginTransactionAsync();

            try
            {
                var query = @"
                    INSERT INTO Planning (RequestCode, CandidateToken, CreatedAt, Status)
                    OUTPUT INSERTED.PlanningId
                    VALUES (@RequestCode, @CandidateToken, @CreatedAt, @Status)";

                int planningId;
                using (var command = new SqlCommand(query, koneksi, (SqlTransaction)transaksi))
                {
                    command.Parameters.AddWithValue("@RequestCode", planning.RequestCode);
                    command.Parameters.AddWithValue("@CandidateToken", planning.CandidateToken);
                    command.Parameters.AddWithValue("@CreatedAt", planning.CreatedAt);
                    command.Parameters.AddWithValue("@Status", planning.Status);

                    var result = await command.ExecuteScalarAsync();
                    planningId = Convert.ToInt32(result);
                }

                var querySlot = @"
                    INSERT INTO PlanningSlot (PlanningId, SlotOrder, SlotName, OriginalQuantity, BalancedQuantity, IsActive)
                    VALUES (@PlanningId, @SlotOrder, @SlotName, @OriginalQuantity, @BalancedQuantity, @IsActive)";

                foreach (var slot in planning.Slots)
                {
                    using var command = new SqlCommand(querySlot, koneksi, (SqlTransaction)transaksi);
                    command.Parameters.AddWithValue("@PlanningId", planningId);
                    command.Parameters.AddWithValue("@SlotOrder", slot.SlotOrder);
                    command.Parameters.AddWithValue("@SlotName", slot.SlotName);
                    command.Parameters.AddWithValue("@OriginalQuantity", slot.OriginalQuantity);
                    command.Parameters.AddWithValue("@BalancedQuantity", slot.BalancedQuantity);
                    command.Parameters.AddWithValue("@IsActive", slot.IsActive);

                    await command.ExecuteNonQueryAsync();
                }

                await transaksi.CommitAsync();
                planning.PlanningId = planningId;
                SimpanInMemory(planning);
            }
            catch
            {
                await transaksi.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex) when (ex is SqlException || ex is InvalidOperationException)
        {
            // Fallback for local development when SQL server is offline or table is not created yet
            SimpanInMemory(planning);
        }
    }

    public async Task<Planning?> GetByRequestCodeAsync(string requestCode)
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            return _inMemoryStore.FirstOrDefault(p => p.RequestCode.Equals(requestCode, StringComparison.OrdinalIgnoreCase));
        }

        try
        {
            using var koneksi = new SqlConnection(_connectionString);
            await koneksi.OpenAsync();

            var query = "SELECT PlanningId, RequestCode, CandidateToken, CreatedAt, Status FROM Planning WHERE RequestCode = @RequestCode";
            Planning? planning = null;

            using (var cmd = new SqlCommand(query, koneksi))
            {
                cmd.Parameters.AddWithValue("@RequestCode", requestCode);
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    planning = new Planning
                    {
                        PlanningId = reader.GetInt32(0),
                        RequestCode = reader.GetString(1),
                        CandidateToken = reader.GetString(2),
                        CreatedAt = reader.GetDateTime(3),
                        Status = reader.GetString(4)
                    };
                }
            }

            if (planning != null)
            {
                planning.Slots = await GetSlotsForPlanningAsync(koneksi, planning.PlanningId);
                return planning;
            }
        }
        catch (Exception ex) when (ex is SqlException || ex is InvalidOperationException)
        {
            // Fallback
        }

        return _inMemoryStore.FirstOrDefault(p => p.RequestCode.Equals(requestCode, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<Planning?> GetByIdAsync(int planningId)
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            return _inMemoryStore.FirstOrDefault(p => p.PlanningId == planningId);
        }

        try
        {
            using var koneksi = new SqlConnection(_connectionString);
            await koneksi.OpenAsync();

            var query = "SELECT PlanningId, RequestCode, CandidateToken, CreatedAt, Status FROM Planning WHERE PlanningId = @PlanningId";
            Planning? planning = null;

            using (var cmd = new SqlCommand(query, koneksi))
            {
                cmd.Parameters.AddWithValue("@PlanningId", planningId);
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    planning = new Planning
                    {
                        PlanningId = reader.GetInt32(0),
                        RequestCode = reader.GetString(1),
                        CandidateToken = reader.GetString(2),
                        CreatedAt = reader.GetDateTime(3),
                        Status = reader.GetString(4)
                    };
                }
            }

            if (planning != null)
            {
                planning.Slots = await GetSlotsForPlanningAsync(koneksi, planning.PlanningId);
                return planning;
            }
        }
        catch (Exception ex) when (ex is SqlException || ex is InvalidOperationException)
        {
            // Fallback
        }

        return _inMemoryStore.FirstOrDefault(p => p.PlanningId == planningId);
    }

    public async Task<List<Planning>> GetAllAsync()
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            return _inMemoryStore.OrderByDescending(p => p.CreatedAt).ToList();
        }

        try
        {
            using var koneksi = new SqlConnection(_connectionString);
            await koneksi.OpenAsync();

            var list = new List<Planning>();
            var query = "SELECT PlanningId, RequestCode, CandidateToken, CreatedAt, Status FROM Planning ORDER BY CreatedAt DESC";

            using (var cmd = new SqlCommand(query, koneksi))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    list.Add(new Planning
                    {
                        PlanningId = reader.GetInt32(0),
                        RequestCode = reader.GetString(1),
                        CandidateToken = reader.GetString(2),
                        CreatedAt = reader.GetDateTime(3),
                        Status = reader.GetString(4)
                    });
                }
            }

            foreach (var item in list)
            {
                item.Slots = await GetSlotsForPlanningAsync(koneksi, item.PlanningId);
            }

            if (list.Count > 0)
                return list;
        }
        catch (Exception ex) when (ex is SqlException || ex is InvalidOperationException)
        {
            // Fallback
        }

        return _inMemoryStore.OrderByDescending(p => p.CreatedAt).ToList();
    }

    private static async Task<List<PlanningSlot>> GetSlotsForPlanningAsync(SqlConnection koneksi, int planningId)
    {
        var slots = new List<PlanningSlot>();
        var query = "SELECT PlanningId, SlotOrder, SlotName, OriginalQuantity, BalancedQuantity, IsActive FROM PlanningSlot WHERE PlanningId = @PlanningId ORDER BY SlotOrder";

        using var cmd = new SqlCommand(query, koneksi);
        cmd.Parameters.AddWithValue("@PlanningId", planningId);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            slots.Add(new PlanningSlot
            {
                PlanningId = reader.GetInt32(0),
                SlotOrder = reader.GetInt32(1),
                SlotName = reader.GetString(2),
                OriginalQuantity = reader.GetInt32(3),
                BalancedQuantity = reader.GetInt32(4),
                IsActive = reader.GetBoolean(5)
            });
        }

        return slots;
    }

    private static void SimpanInMemory(Planning planning)
    {
        if (planning.PlanningId <= 0)
        {
            planning.PlanningId = Interlocked.Increment(ref _nextInMemoryId);
        }

        var existing = _inMemoryStore.FirstOrDefault(p => p.RequestCode.Equals(planning.RequestCode, StringComparison.OrdinalIgnoreCase));
        if (existing == null)
        {
            _inMemoryStore.Add(planning);
        }
    }
}