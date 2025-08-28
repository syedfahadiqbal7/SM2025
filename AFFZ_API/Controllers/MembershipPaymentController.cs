using AFFZ_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AFFZ_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembershipPaymentController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly ILogger<MembershipPaymentController> _logger;
        public MembershipPaymentController(MyDbContext context, ILogger<MembershipPaymentController> logger)
        {
            _context = context;
            _logger = logger;
        }
        [HttpPost("sendRequestToSaveMembershipPayment")]
        public async Task<IActionResult> sendRequestToSavePayment(MembershipPaymentHistory saveMembershipPaymentHistory)
        {
            _logger.LogInformation("sendRequestToSavePayment method called with UserId: {UserId}", saveMembershipPaymentHistory.PAYERID);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Set new record to active membership
                saveMembershipPaymentHistory.IsActiveMembership = 1;

                // Deactivate existing memberships for the user
                var existingMemberships = _context.MembershipPaymentHistory
                    .Where(m => m.PAYERID == saveMembershipPaymentHistory.PAYERID && m.IsActiveMembership == 1)
                    .ToList();

                foreach (var membership in existingMemberships)
                {
                    membership.IsActiveMembership = 0;
                    _context.Update(membership);
                }

                // Save new payment history
                _context.Add(saveMembershipPaymentHistory);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Payment Done Successfully.");
                return CreatedAtAction(nameof(sendRequestToSavePayment), new { id = saveMembershipPaymentHistory.ID }, saveMembershipPaymentHistory);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An error occurred while processing the payment request.");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("CheckMemberShip")]
        public async Task<MembershipPaymentHistory> CheckMemberShip(int merchantId)
        {
            _logger.LogInformation("CheckMemberShip method called with merchant Id: {merchantId}", merchantId);
            try
            {
                var res = await _context.MembershipPaymentHistory.Where(x => x.PAYERID == merchantId && x.IsActiveMembership == 1).FirstOrDefaultAsync();
                return res;
            }
            catch (Exception ex)
            {
                // Log the exception details
                // Use your preferred logging framework here. For example:
                _logger.LogError(ex, "An error occurred while processing the discount request.");

                throw;
            }
        }
        //
        [HttpPost("UpdateRequestForDisCountToUserForPaymentDone")]
        public async Task<IActionResult> UpdateRequestForDisCountToUserForPaymentDone(RequestForDisCountToUser updatePaymentStatus)
        {

            _logger.LogInformation("UpdateRequestForDisCountToUserForPaymentDont method called with UserId: {UserId}", updatePaymentStatus.UID);
            try
            {

                _context.Update(updatePaymentStatus);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Payment status updated.");
                return Ok("Payment status updated.");
            }
            catch (Exception ex)
            {
                // Log the exception details
                // Use your preferred logging framework here. For example:
                _logger.LogError(ex, "An error occurred while processing the discount request.");

                return StatusCode(500, ex.Message);
            }
        }
    }
}
