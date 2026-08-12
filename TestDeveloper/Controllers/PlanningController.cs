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
    public async Task<IActionResult> Create(
        CreatePlanningRequest request)
    {
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

            var hasil = await _service.ProsesAsync(
                request.RequestCode,
                request.CandidateToken,
                slots);

            return Ok(hasil);
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
                message = ex.Message
            });
        }
    }
}