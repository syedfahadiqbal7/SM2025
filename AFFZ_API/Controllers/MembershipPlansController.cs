using AFFZ_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class MembershipPlansController : ControllerBase
{
    private readonly MyDbContext _context;
    private readonly ILogger<MembershipPlansController> _logger;

    public MembershipPlansController(MyDbContext context, ILogger<MembershipPlansController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/MembershipPlans
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MembershipPlan>>> GetPlans()
    {
        try
        {
            var plans = await _context.MembershipPlans
                .Include(mp => mp.MembershipPlanSlabs)
                .ThenInclude(mps => mps.Slab)
                .ToListAsync();

            return Ok(plans);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching the plans.");
            return StatusCode(500, "Internal server error.");
        }
    }

    // GET: api/MembershipPlans/5
    [HttpGet("{id}")]
    public async Task<ActionResult<MembershipPlan>> GetPlan(int id)
    {
        try
        {
            var plan = await _context.MembershipPlans
                .Include(mp => mp.MembershipPlanSlabs)
                .ThenInclude(mps => mps.Slab)
                .FirstOrDefaultAsync(mp => mp.Id == id);

            if (plan == null)
            {
                return NotFound("Plan not found.");
            }

            return Ok(plan);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while fetching the plan with id {id}.");
            return StatusCode(500, "Internal server error.");
        }
    }

    // POST: api/MembershipPlans
    [HttpPost("CreatePlan")]
    public async Task<ActionResult<int>> CreatePlan(MembershipPlan plan)
    {
        try
        {
            _context.MembershipPlans.Add(plan);
            await _context.SaveChangesAsync();

            // Return the created plan's ID
            return Ok(plan.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating the plan.");
            return StatusCode(500, "Internal server error.");
        }
    }

    [HttpPost("AddSlabsToPlan/{planId}")]
    public async Task<IActionResult> AddSlabsToPlan(int planId, [FromBody] List<int> slabIds)
    {
        try
        {
            var plan = await _context.MembershipPlans.Include(p => p.MembershipPlanSlabs).FirstOrDefaultAsync(p => p.Id == planId);

            if (plan == null)
            {
                return NotFound("Plan not found.");
            }

            // Clear existing slabs and add new ones
            plan.MembershipPlanSlabs.Clear();
            foreach (var slabId in slabIds)
            {
                plan.MembershipPlanSlabs.Add(new MembershipPlanSlabs { SlabId = slabId });
            }

            await _context.SaveChangesAsync();
            return Ok("Slabs updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while adding slabs to plan {planId}.");
            return StatusCode(500, "Internal server error.");
        }
    }
    // POST: api/MembershipPlans/5
    [HttpPost("{id}")]
    public async Task<IActionResult> UpdatePlan(int id, MembershipPlan plan)
    {
        if (id != plan.Id)
        {
            return BadRequest("Plan ID mismatch.");
        }
        try
        {
            var existingPlan = await _context.MembershipPlans
                .Include(p => p.MembershipPlanSlabs)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (existingPlan == null)
            {
                return NotFound("Plan not found.");
            }
            // Update basic properties of the plan
            existingPlan.Name = plan.Name;
            existingPlan.Price = plan.Price;
            existingPlan.Duration = plan.Duration;
            existingPlan.ServicesLimit = plan.ServicesLimit;
            existingPlan.StaffLimit = plan.StaffLimit;
            existingPlan.AppointmentsLimit = plan.AppointmentsLimit;
            existingPlan.Gallery = plan.Gallery;
            existingPlan.AdditionalServices = plan.AdditionalServices;
            existingPlan.UpdatedAt = DateTime.UtcNow;

            // Note: Do NOT update MembershipPlanSlabs here.
            // The slabs will be updated through a separate API call (`AddSlabsToPlan`).

            await _context.SaveChangesAsync();

            return Ok("Membership plan updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating the plan.");
            return StatusCode(500, "Internal server error.");
        }
    }
    // DELETE: api/MembershipPlans/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePlan(int id)
    {
        try
        {
            var plan = await _context.MembershipPlans
                .Include(mp => mp.MembershipPlanSlabs)
                .FirstOrDefaultAsync(mp => mp.Id == id);

            if (plan == null)
            {
                return NotFound("Plan not found.");
            }

            // Remove related slabs
            _context.MembershipPlanSlabs.RemoveRange(plan.MembershipPlanSlabs);
            _context.MembershipPlans.Remove(plan);

            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting the plan.");
            return StatusCode(500, "Internal server error.");
        }
    }
}