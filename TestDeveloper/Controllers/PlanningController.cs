using Microsoft.AspNetCore.Mvc;
using TestDeveloper.Application;
using TestDeveloper.Domain;
using TestDeveloper.Models;

namespace TestDeveloper.Controllers;

[ApiController]
[Route("api/planning")]
public class PlanningController : ControllerBase
{
    private readonly PlanningService _service;

    public PlanningController(PlanningService service)
    {
        _service = service;
    }


    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePlanningRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                status = "ERROR",
                message = "Format data atau tipe data angka tidak valid."
            });
        }

        try
        {
            var slots = request.Slots
                .Select(x => new PlanningSlot
                {
                    SlotOrder = x.SlotOrder,
                    SlotName = x.SlotName,
                    OriginalQuantity = x.OriginalQuantity
                })
                .ToList();

            var result = await _service.ProsesAsync(
                request.RequestCode,
                request.CandidateToken,
                slots);

            return Ok(ToResponseDto(result));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                status = "ERROR",
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                status = "ERROR",
                message = "Terjadi kesalahan saat memproses planning: " + ex.Message
            });
        }
    }


    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _service.GetAllAsync();
        var response = list.Select(ToResponseDto).ToList();
        return Ok(response);
    }


    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var planning = await _service.GetByIdAsync(id);
        if (planning == null)
        {
            return NotFound(new
            {
                status = "ERROR",
                message = $"Transaksi planning dengan ID {id} tidak ditemukan."
            });
        }

        return Ok(ToResponseDto(planning));
    }


    [HttpGet("code/{requestCode}")]
    public async Task<IActionResult> GetByRequestCode(string requestCode)
    {
        var planning = await _service.GetByRequestCodeAsync(requestCode);
        if (planning == null)
        {
            return NotFound(new
            {
                status = "ERROR",
                message = $"Transaksi planning dengan RequestCode '{requestCode}' tidak ditemukan."
            });
        }

        return Ok(ToResponseDto(planning));
    }

    private static object ToResponseDto(Planning planning)
    {
        var totalOriginal = planning.Slots.Sum(s => s.OriginalQuantity);
        var totalBalanced = planning.Slots.Sum(s => s.BalancedQuantity);

        return new
        {
            planningId = planning.PlanningId,
            requestCode = planning.RequestCode,
            candidateToken = planning.CandidateToken,
            createdAt = planning.CreatedAt,
            status = planning.Status,
            totalOriginalQuantity = totalOriginal,
            totalBalancedQuantity = totalBalanced,
            slots = planning.Slots.Select(s => new
            {
                slotOrder = s.SlotOrder,
                slotName = s.SlotName,
                originalQuantity = s.OriginalQuantity,
                balancedQuantity = s.BalancedQuantity,
                isActive = s.IsActive
            })
        };
    }
}