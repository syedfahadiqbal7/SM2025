using AFFZ_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AFFZ_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SlabsController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly ILogger<SlabsController> _logger;

        public SlabsController(MyDbContext context, ILogger<SlabsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Slab>>> GetSlabs()
        {
            try
            {
                return await _context.Slab.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching the plan.");
                throw;
            }
        }
        [HttpGet("GetDefaultApplicableSlab")]
        public async Task<ActionResult<Slab>> GetDefaultApplicableSlab(decimal amount, int? merchantId = null)
        {
            if (amount <= 0)
            {
                _logger.LogWarning("Invalid amount received: {Amount}", amount);
                return BadRequest("Amount must be greater than zero.");
            }

            try
            {
                _logger.LogInformation("Fetching applicable slab for amount: {Amount}, MerchantId: {MerchantId}", amount, merchantId);

                // Convert the amount to double for comparison
                double amountAsDouble = Convert.ToDouble(amount);

                // First, check if merchant has an active membership
                if (merchantId.HasValue && merchantId.Value > 0)
                {
                    var activeMembership = await _context.MembershipPaymentHistory
                        .Where(m => m.PAYERID == merchantId.Value && m.IsActiveMembership == 1)
                        .OrderByDescending(m => m.PAYMENTDATETIME)
                        .FirstOrDefaultAsync();

                    if (activeMembership != null)
                    {
                        // Get the membership plan name
                        var membershipPlan = await _context.MembershipPlans
                            .FirstOrDefaultAsync(mp => mp.Id == activeMembership.MembershipId);

                        if (membershipPlan != null)
                        {
                            // Try to find slab based on membership plan name
                            var membershipSlab = await _context.Slab
                                .Where(s => s.LowerLimit <= amountAsDouble &&
                                          s.UpperLimit >= amountAsDouble &&
                                          s.SlabName == membershipPlan.Name &&
                                          s.IsDefaultSlab == false)
                                .OrderBy(s => s.LowerLimit)
                                .FirstOrDefaultAsync();

                            if (membershipSlab != null)
                            {
                                _logger.LogInformation("Membership slab found for amount: {Amount}, MerchantId: {MerchantId}, SlabID: {SlabID}, SlabName: {SlabName}",
                                    amount, merchantId, membershipSlab.SlabID, membershipSlab.SlabName);
                                return Ok(membershipSlab);
                            }
                        }
                    }
                }

                // Fallback to default slab if no membership or membership slab found
                var defaultSlab = await _context.Slab
                    .Where(s => s.LowerLimit <= amountAsDouble && s.UpperLimit >= amountAsDouble && s.IsDefaultSlab == true)
                    .OrderBy(s => s.LowerLimit)
                    .FirstOrDefaultAsync();

                if (defaultSlab == null)
                {
                    _logger.LogWarning("No applicable slab found for amount: {Amount}", amount);
                    return NotFound("No applicable slab found.");
                }

                _logger.LogInformation("Default slab found for amount: {Amount}, SlabID: {SlabID}", amount, defaultSlab.SlabID);
                return Ok(defaultSlab);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching the applicable slab for amount: {Amount}", amount);
                return StatusCode(500, "An error occurred while fetching the applicable slab.");
            }
        }
        [HttpGet("GetSlabs")]
        public async Task<ActionResult<Slab>> GetSlabs(int id)
        {
            try
            {
                var plan = await _context.Slab.FindAsync(id);
                if (plan == null) return NotFound();
                return plan;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching the plan with id" + id + ".");
                throw;
            }
        }

        [HttpPost]
        public async Task<ActionResult<Slab>> CreatePlan(Slab plan)
        {
            try
            {
                _context.Slab.Add(plan);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetSlabs), new { id = plan.SlabID }, plan);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the Slab.");
                throw;
            }
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> UpdateSlab(int id, Slab plan)
        {
            try
            {
                if (id != plan.SlabID) return BadRequest();
                _context.Entry(plan).State = EntityState.Modified;
                plan.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the Slab.");
                throw;
            }
        }

        [HttpGet("DeleteSlab")]
        public async Task<IActionResult> DeleteSlab(int id)
        {
            try
            {
                var plan = await _context.Slab.FindAsync(id);
                if (plan == null) return NotFound();
                _context.Slab.Remove(plan);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while removing the Slab.");

                throw;
            }
        }
        [HttpPost("RequestAmountChange")]
        public async Task<IActionResult> RequestAmountChange(AmountChangeRequestModel request)
        {
            if (request == null || request.ServiceId <= 0 || request.RequestedAmount <= 0)
            {
                _logger.LogWarning("Invalid amount change request received.");
                return BadRequest("Invalid request parameters.");
            }

            try
            {
                // Check if the service exists in the database
                var service = await _context.Services.FindAsync(request.ServiceId);
                if (service == null)
                {
                    _logger.LogWarning("Service with ID {ServiceId} not found.", request.ServiceId);
                    return Ok(new { Message = "Service not found." });
                }

                // Create a new amount change request entry (assuming a new table for this purpose)
                var amountChangeRequest = new AmountChangeRequests
                {
                    ServiceId = request.ServiceId,
                    RequestedAmount = request.RequestedAmount,
                    Status = "Pending", // Initial status
                    RequestedAt = DateTime.UtcNow,
                    ProviderId = request.ProviderId
                };

                _context.AmountChangeRequests.Add(amountChangeRequest);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Amount change request created successfully for Service ID: {ServiceId}", request.ServiceId);
                return Ok(new { Message = "Amount change request created successfully.", RequestId = amountChangeRequest.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the amount change request.");
                return StatusCode(500, "An error occurred while processing the request.");
            }
        }
        [HttpGet("GetApplicableSlab")]
        public async Task<ActionResult<Slab>> GetApplicableSlab(decimal amount)
        {
            if (amount <= 0)
            {
                _logger.LogWarning("Invalid amount received: {Amount}", amount);
                return BadRequest("Amount must be greater than zero.");
            }

            try
            {
                _logger.LogInformation("Fetching applicable slab for amount: {Amount}", amount);

                // Convert the amount to double for comparison
                double amountAsDouble = Convert.ToDouble(amount);

                var applicableSlab = await _context.Slab
                    .Where(s => s.LowerLimit <= amountAsDouble && s.UpperLimit >= amountAsDouble)
                    .OrderBy(s => s.LowerLimit)
                    .FirstOrDefaultAsync();

                if (applicableSlab == null)
                {
                    _logger.LogWarning("No applicable slab found for amount: {Amount}", amount);
                    return NotFound("No applicable slab found.");
                }

                _logger.LogInformation("Applicable slab found for amount: {Amount}, SlabID: {SlabID}", amount, applicableSlab.SlabID);
                return Ok(applicableSlab);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching the applicable slab for amount: {Amount}", amount);
                return StatusCode(500, "An error occurred while fetching the applicable slab.");
            }
        }
        [HttpGet("GetMembershipApplicableSlab")]
        public async Task<ActionResult<Slab>> GetMembershipApplicableSlab(decimal amount, int merchantId)
        {
            if (amount <= 0)
            {
                _logger.LogWarning("Invalid amount received: {Amount}", amount);
                return BadRequest("Amount must be greater than zero.");
            }

            if (merchantId <= 0)
            {
                _logger.LogWarning("Invalid merchant ID received: {MerchantId}", merchantId);
                return BadRequest("Merchant ID must be greater than zero.");
            }

            try
            {
                _logger.LogInformation("Fetching membership slab for amount: {Amount}, MerchantId: {MerchantId}", amount, merchantId);

                // Convert the amount to double for comparison
                double amountAsDouble = Convert.ToDouble(amount);

                // Check if merchant has an active membership
                var activeMembership = await _context.MembershipPaymentHistory
                    .Where(m => m.PAYERID == merchantId && m.IsActiveMembership == 1)
                    .OrderByDescending(m => m.PAYMENTDATETIME)
                    .FirstOrDefaultAsync();

                if (activeMembership != null)
                {
                    // Get the membership plan name
                    var membershipPlan = await _context.MembershipPlans
                        .FirstOrDefaultAsync(mp => mp.Id == activeMembership.MembershipId);

                    if (membershipPlan != null)
                    {
                        // Try to find slab based on membership plan name
                        var membershipSlab = await _context.Slab
                            .Where(s => s.LowerLimit <= amountAsDouble &&
                                      s.UpperLimit >= amountAsDouble &&
                                      s.IsDefaultSlab == false)
                            .OrderBy(s => s.LowerLimit)
                            .FirstOrDefaultAsync();

                        if (membershipSlab != null)
                        {
                            _logger.LogInformation("Membership slab found for amount: {Amount}, MerchantId: {MerchantId}, SlabID: {SlabID}, SlabName: {SlabName}",
                                amount, merchantId, membershipSlab.SlabID, membershipSlab.SlabName);
                            return Ok(membershipSlab);
                        }
                        else
                        {
                            _logger.LogWarning("No membership slab found for amount: {Amount}, Membership: {MembershipName}",
                                amount, membershipPlan.Name);
                        }
                    }
                }
                else
                {
                    _logger.LogInformation("No active membership found for merchant: {MerchantId}", merchantId);
                }

                // Return null if no membership slab found
                return NotFound("No membership slab found for this merchant.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching the membership applicable slab for amount: {Amount}, MerchantId: {MerchantId}", amount, merchantId);
                return StatusCode(500, "An error occurred while fetching the membership applicable slab.");
            }
        }

    }
    public class AmountChangeRequestModel
    {
        public int ServiceId { get; set; }
        public int ProviderId { get; set; }
        public decimal RequestedAmount { get; set; }
    }
}
