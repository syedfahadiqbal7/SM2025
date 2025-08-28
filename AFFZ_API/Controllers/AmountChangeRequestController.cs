
using AFFZ_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AFFZ_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AmountChangeRequestController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly ILogger<AmountChangeRequestController> _logger;

        public AmountChangeRequestController(MyDbContext context, ILogger<AmountChangeRequestController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("AmountChangeRequestExist")]
        public async Task<IActionResult> Exists(int serviceId, int providerId)
        {
            try
            {
                bool exists = await _context.AmountChangeRequests.AnyAsync(e => e.ServiceId == serviceId && e.ProviderId == providerId && e.Status == "Pending");
                return Ok(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence of AmountChangeRequest for ServiceId: {ServiceId}, ProviderId: {ProviderId}", serviceId, providerId);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpGet("GetByMerchantId")]
        public async Task<IActionResult> GetByMerchantId(int merchantId)
        {
            try
            {
                var requests = await _context.AmountChangeRequests.Where(e => e.ProviderId == merchantId).ToListAsync();
                return Ok(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching AmountChangeRequests for MerchantId: {MerchantId}", merchantId);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpGet("GetByServiceId")]
        public async Task<IActionResult> GetByServiceId(int requestId)
        {
            try
            {
                var request = await _context.AmountChangeRequests.FirstOrDefaultAsync(e => e.ServiceId == requestId);
                if (request == null)
                {
                    _logger.LogWarning("No AmountChangeRequest found for RequestId: {RequestId}", requestId);
                    return NotFound("Request not found.");
                }
                return Ok(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching AmountChangeRequest for RequestId: {RequestId}", requestId);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        //[HttpPost("Approve")]
        //public async Task<IActionResult> Approve(int requestId)
        //{
        //    try
        //    {
        //        var request = await _context.AmountChangeRequests.FirstOrDefaultAsync(e => e.Id == requestId);
        //        if (request == null)
        //        {
        //            _logger.LogWarning("No AmountChangeRequest found to approve for RequestId: {RequestId}", requestId);
        //            return NotFound("Request not found.");
        //        }

        //        request.Status = "Approved";
        //        request.ApprovedAt = DateTime.UtcNow;
        //        _context.AmountChangeRequests.Update(request);
        //        await _context.SaveChangesAsync();

        //        _logger.LogInformation("AmountChangeRequest approved for RequestId: {RequestId}", requestId);
        //        return Ok("Request approved successfully.");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error approving AmountChangeRequest for RequestId: {RequestId}", requestId);
        //        return StatusCode(500, "An unexpected error occurred.");
        //    }
        //}
        [HttpPost("Approve")]
        public async Task<IActionResult> Approve(int requestId)
        {
            try
            {
                var request = await _context.AmountChangeRequests.FirstOrDefaultAsync(e => e.Id == requestId);
                if (request == null)
                {
                    _logger.LogWarning("No AmountChangeRequest found to approve for RequestId: {RequestId}", requestId);
                    return NotFound("Request not found.");
                }

                var service = await _context.Services.FirstOrDefaultAsync(s => s.ServiceId == request.ServiceId);
                if (service == null)
                {
                    _logger.LogWarning("Service not found for ServiceId: {ServiceId}", request.ServiceId);
                    return NotFound("Associated service not found.");
                }

                // Update request status to approved
                request.Status = "Approved";
                request.ApprovedAt = DateTime.UtcNow;
                _context.AmountChangeRequests.Update(request);


                // Calculate DeductionValue based on DeductionType
                if (service.DeductionType == "Percentage")
                {
                    // Calculate the percentage of RequestedAmount from ServicePrice
                    decimal? merchantPercentage = (request.RequestedAmount / service.ServicePrice) * 100;

                    // Update ServiceAmountPaidToAdmin to the requested amount
                    service.ServiceAmountPaidToAdmin = (int)request.RequestedAmount;

                    // Calculate DeductionValue (ServicePrice - RequestedAmount)
                    service.DeductionValue = (decimal)(service.ServicePrice - request.RequestedAmount);
                    _logger.LogInformation("Request approved and service updated for RequestId: {RequestId}, Merchant Percentage: {MerchantPercentage}%, DeductionValue: {DeductionValue}",
                    requestId, merchantPercentage, service.DeductionValue);
                }
                else if (service.DeductionType == "Fix")
                {
                    service.DeductionValue = (decimal)(service.ServicePrice - request.RequestedAmount);
                }
                _context.Services.Update(service);
                await _context.SaveChangesAsync();
                var notification = new Notification
                {
                    UserId = "0",
                    Message = $"Admin has approved your request for Amount change request for the Service[{request.ServiceId}].",
                    MerchantId = request.ProviderId.ToString(),
                    RedirectToActionUrl = $"/MerchantService/MerchantServiceIndex",
                    MessageFromId = 0,
                    SenderType = "Admin",
                    SID = Convert.ToInt32(request.ServiceId),
                    NotificationType = "Alert"
                };
                notification.CreatedDate = DateTime.UtcNow.ToLocalTime();
                notification.ReadDate = new DateTime(1900, 1, 1);

                notification.IsRead = false;
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Request approved successfully.",
                    deductionValue = service.DeductionValue.ToString("C")
                });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database update error while approving AmountChangeRequest for RequestId: {RequestId}", requestId);
                return StatusCode(500, "A database error occurred.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while approving AmountChangeRequest for RequestId: {RequestId}", requestId);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
        [HttpPost("Reject")]
        public async Task<IActionResult> Reject(int requestId, string reason)
        {
            try
            {
                var request = await _context.AmountChangeRequests.FirstOrDefaultAsync(e => e.Id == requestId);
                if (request == null)
                {
                    _logger.LogWarning("No AmountChangeRequest found to reject for RequestId: {RequestId}", requestId);
                    return NotFound("Request not found.");
                }

                request.Status = "Rejected";
                request.RejectionReason = reason;
                request.RejectedAt = DateTime.UtcNow;
                _context.AmountChangeRequests.Update(request);
                await _context.SaveChangesAsync();
                var notification = new Notification
                {
                    UserId = "0",
                    Message = $"AmountChangeRequest rejected. Reason: {reason} for the Service[{request.ServiceId}].",
                    MerchantId = request.ProviderId.ToString(),
                    RedirectToActionUrl = $"/MerchantService/MerchantServiceIndex",
                    MessageFromId = 0,
                    SenderType = "Admin",
                    SID = Convert.ToInt32(request.ServiceId),
                    NotificationType = "Alert"
                };
                notification.CreatedDate = DateTime.UtcNow.ToLocalTime();
                notification.ReadDate = new DateTime(1900, 1, 1);

                notification.IsRead = false;
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
                _logger.LogInformation("AmountChangeRequest rejected for RequestId: {RequestId} with reason: {Reason}", requestId, reason);
                return Ok("Request rejected successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting AmountChangeRequest for RequestId: {RequestId}", requestId);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
    }
}
