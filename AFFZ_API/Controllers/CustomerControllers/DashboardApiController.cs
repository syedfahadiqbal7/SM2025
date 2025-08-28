using AFFZ_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AFFZ_API.Controllers.CustomerControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardApiController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly ILogger<DashboardApiController> _logger;

        public DashboardApiController(MyDbContext context, ILogger<DashboardApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("GetTotalOrders/{userId}")]
        public async Task<ActionResult<int>> GetTotalOrders(int userId)
        {
            try
            {
                var results = await _context.Database
                     .SqlQueryRaw<int>("EXEC CountStatusAfterPaymentDoneByUId @UId = {0}", userId)
                     .ToListAsync();
                var result = results.FirstOrDefault();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing CountStatusAfterPaymentDoneByUId for UserId: {UserId}", userId);
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("GetTotalSpend/{userId}")]
        public async Task<ActionResult<decimal>> GetTotalSpend(int userId)
        {
            try
            {
                var results = await _context.Database
                    .SqlQueryRaw<decimal>("EXEC GetTotalAmountByCustomer @UserId = {0}", userId)
                    .ToListAsync();
                var result = results.FirstOrDefault();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing GetTotalAmountByCustomer for UserId: {UserId}", userId);
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("GetWalletPoints/{userId}")]
        public async Task<ActionResult<decimal>> GetWalletPoints(int userId)
        {
            try
            {
                var result = await _context.Wallet
                    .Where(w => w.CustomerID == userId)
                    .Select(w => w.Points)
                    .FirstOrDefaultAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Wallet Points for UserId: {UserId}", userId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("GetRecentBookings/{userId}")]
        public async Task<ActionResult<IEnumerable<RecentBookingDto>>> GetRecentBookings(int userId)
        {
            try
            {
                var result = await _context.Database
                    .SqlQueryRaw<RecentBookingDto>(
                        "EXEC RecentBookings @CustomerId = {0}", userId)
                    .ToListAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing RecentBookings for UserId: {UserId}", userId);
                return StatusCode(500, "Internal server error");
            }
        }
    }

    public class RecentBookingDto
    {
        public string ServiceImage { get; set; }
        public string ServiceName { get; set; }
        public DateTime ResponseDateTime { get; set; }
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public string ProfilePicture { get; set; }
    }
}
