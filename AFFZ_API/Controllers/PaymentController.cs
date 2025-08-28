using AFFZ_API.Interfaces;
using AFFZ_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace AFFZ_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly ILogger<PaymentController> _logger;
        private readonly IPaymentService _paymentService;

        public PaymentController(MyDbContext context, ILogger<PaymentController> logger, IPaymentService paymentService)
        {
            _context = context;
            _logger = logger;
            _paymentService = paymentService;
        }

        [HttpPost("sendRequestToSavePayment")]
        public async Task<IActionResult> sendRequestToSavePayment(PaymentHistory savePaymentHistory)
        {
            _logger.LogInformation("sendRequestToSavePayment method called with UserId: {UserId}", savePaymentHistory.PAYERID);
            try
            {
                // Use the payment service for processing
                var paymentRequest = new PaymentRequest
                {
                    UserId = savePaymentHistory.PAYERID,
                    MerchantId = savePaymentHistory.MERCHANTID,
                    ServiceId = savePaymentHistory.SERVICEID,
                    Amount = decimal.Parse(savePaymentHistory.AMOUNT),
                    PaymentMethod = savePaymentHistory.PAYMENTTYPE,
                    Description = $"Payment for service {savePaymentHistory.SERVICEID}",
                    RFDFU = savePaymentHistory.RFDFU,
                    Quantity = savePaymentHistory.Quantity
                };

                var result = await _paymentService.ProcessPaymentAsync(paymentRequest);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Payment processed successfully via service");
                    return CreatedAtAction(nameof(sendRequestToSavePayment), new { id = result.PaymentRecord?.ID }, result.PaymentRecord);
                }
                else
                {
                    _logger.LogError("Payment processing failed: {Message}", result.Message);
                    return BadRequest(result.Message);
                }
            }
            catch (Exception ex)
            {
                // Log the exception details
                _logger.LogError(ex, "An error occurred while processing the payment request.");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("UpdateRequestForDisCountToUserForPaymentDone")]
        public async Task<IActionResult> UpdateRequestForDisCountToUserForPaymentDone(RequestForDisCountToUser updatePaymentStatus)
        {
            _logger.LogInformation("UpdateRequestForDisCountToUserForPaymentDone method called with UserId: {UserId}", updatePaymentStatus.UID);
            try
            {
                var result = await _paymentService.UpdateDiscountRequestAsync(updatePaymentStatus);

                if (result)
                {
                    _logger.LogInformation("Payment status updated successfully via service");
                    return Ok("Payment status updated.");
                }
                else
                {
                    _logger.LogWarning("Failed to update payment status");
                    return BadRequest("Failed to update payment status.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the discount request.");
                return StatusCode(500, ex.Message);
            }
        }

        // New endpoint using the payment service
        [HttpGet("GetPaymentHistory/{userId}")]
        public async Task<IActionResult> GetPaymentHistory(int userId)
        {
            try
            {
                var paymentHistory = await _paymentService.GetPaymentHistoryAsync(userId);
                if (paymentHistory != null)
                {
                    return Ok(paymentHistory);
                }
                return NotFound("No payment history found for this user.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment history for user {UserId}", userId);
                return StatusCode(500, "Error retrieving payment history.");
            }
        }

        // New endpoint for refunds
        [HttpPost("RefundPayment")]
        public async Task<IActionResult> RefundPayment([FromBody] PaymentRefundRequest refundRequest)
        {
            try
            {
                var result = await _paymentService.RefundPaymentAsync(refundRequest.PaymentId, refundRequest.Amount);
                if (result)
                {
                    return Ok("Payment refunded successfully.");
                }
                return BadRequest("Failed to refund payment.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund for payment {PaymentId}", refundRequest.PaymentId);
                return StatusCode(500, "Error processing refund.");
            }
        }
    }
}
