using AFFZ_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AFFZ_API.Controllers.MerchantControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(MyDbContext context, ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetDashboardStatistics(int merchantId)
        {
            try
            {
                var result = _context.MiniDashBoardData
           .FromSqlRaw("EXEC [dbo].[GetDashboardStatistics] @MerchantId",
               new SqlParameter("@MerchantId", merchantId))
           .AsEnumerable().FirstOrDefault();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetDashboardStatistics");
                return StatusCode(500, new
                {
                    message = "An error occurred while retrieving dashboard statistics.",
                    error = ex.Message
                });
            }
        }

        [HttpGet("GetTotalRevenueAsync")]
        public async Task<decimal> GetTotalRevenueAsync(int mId)
        {
            try
            {
                var totalRevenueParam = new SqlParameter("@MID", mId);

                var result = (await _context.TotalRevenueResults
                    .FromSqlRaw("EXEC [dbo].[GetTotalRevenueByMerchantId] @MID", totalRevenueParam)
                    .ToListAsync())
                    .FirstOrDefault();

                return result?.TotalRevenue ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetTotalRevenueAsync");
                throw;
            }
        }

        [HttpGet("GetMiniDashboardStats")]
        public async Task<IActionResult> GetMiniDashboardStats(int merchantId)
        {
            try
            {
                var requestsReceived = (await _context.MiniDashboardResults
                    .FromSqlRaw("EXEC [dbo].[GetTotalRequestsReceived] @MerchantId", new SqlParameter("@MerchantId", merchantId))
                    .ToListAsync())
                    .FirstOrDefault();

                var eligibilityRequests = (await _context.MiniDashboardResults
                    .FromSqlRaw("EXEC [dbo].[GetTotalEligibilityRequests] @MerchantId", new SqlParameter("@MerchantId", merchantId))
                    .ToListAsync())
                    .FirstOrDefault();

                var docVerification = (await _context.MiniDashboardResults
                    .FromSqlRaw("EXEC [dbo].[GetTotalCustomersForDocVerification] @MerchantId", new SqlParameter("@MerchantId", merchantId))
                    .ToListAsync())
                    .FirstOrDefault();

                var verifiedWaiting = (await _context.MiniDashboardResults
                    .FromSqlRaw("EXEC [dbo].[GetTotalCustomersDocsVerifiedWaiting] @MerchantId", new SqlParameter("@MerchantId", merchantId))
                    .ToListAsync())
                    .FirstOrDefault();

                var customersServiceStarted = (await _context.MiniDashboardResults
                    .FromSqlRaw("EXEC [dbo].[GetTotalCustomersServiceStarted] @MerchantId", new SqlParameter("@MerchantId", merchantId))
                    .ToListAsync())
                    .FirstOrDefault();

                return Ok(new
                {
                    RequestsReceived = requestsReceived?.Count ?? 0,
                    EligibilityRequests = eligibilityRequests?.Count ?? 0,
                    DocumentVerification = docVerification?.Count ?? 0,
                    DocsVerifiedWaiting = verifiedWaiting?.Count ?? 0,
                    CustomersServiceStarted = customersServiceStarted?.Count ?? 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetMiniDashboardStats");
                return StatusCode(500, new { message = "An error occurred while retrieving mini dashboard stats.", error = ex.Message });
            }
        }

        [HttpGet("GetTotalRevenueLastWeekAsync")]
        public async Task<decimal> GetTotalRevenueLastWeekAsync(int mId)
        {
            try
            {
                var startOfWeek = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
                var endOfWeek = startOfWeek.AddDays(7).AddSeconds(-1);

                var totalRevenue = (await _context.PaymentHistories
                    .AsNoTracking()
                    .Where(p => p.MERCHANTID == mId && p.PAYMENTDATETIME >= startOfWeek && p.PAYMENTDATETIME <= endOfWeek)
                    .ToListAsync())
                    .Sum(p => decimal.TryParse(p.AMOUNT, out var amount) ? amount : 0);

                return totalRevenue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetTotalRevenueLastWeekAsync");
                throw;
            }
        }

        [HttpGet("GetTopRevenueServiceAsync")]
        public async Task<TopServiceRevenueDto> GetTopRevenueServiceAsync(int mid)
        {
            try
            {
                var payments = await _context.PaymentHistories
                                .Include(p => p.Service)
                                .Where(p => p.MERCHANTID == mid)
                                .ToListAsync();

                var topService = payments
                    .GroupBy(p => new { p.SERVICEID, p.Service.SID })
                    .Select(g => new
                    {
                        g.Key.SERVICEID,
                        g.Key.SID,
                        TotalRevenue = g.Sum(p => decimal.TryParse(p.AMOUNT, out decimal amount) ? amount : 0)
                    })
                    .Join(_context.ServicesLists,
                        grouped => grouped.SID,
                        serviceList => serviceList.ServiceListID,
                        (grouped, serviceList) => new TopServiceRevenueDto
                        {
                            ServiceId = grouped.SERVICEID,
                            ServiceName = serviceList.ServiceName,
                            TotalRevenue = grouped.TotalRevenue
                        })
                    .OrderByDescending(s => s.TotalRevenue)
                    .FirstOrDefault();

                return topService;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetTopRevenueServiceAsync");
                throw;
            }
        }

        [HttpGet("GetRecentTransactionsAsync")]
        public async Task<IActionResult> GetRecentTransactionsAsync(int mId, int count)
        {
            try
            {
                //var paymentHistoriesWithCustomerNames = await (
                //    from payment in _context.PaymentHistories.AsNoTracking()
                //    join customer in _context.Customers.AsNoTracking()
                //        on payment.PAYERID equals customer.CustomerId
                //    where payment.MERCHANTID == mId
                //    orderby payment.PAYMENTDATETIME descending
                //    select new
                //    {
                //        payment.ID,
                //        payment.PAYMENTTYPE,
                //        payment.AMOUNT,
                //        payment.PAYERID,
                //        payment.MERCHANTID,
                //        payment.ISPAYMENTSUCCESS,
                //        payment.Quantity,
                //        payment.SERVICEID,
                //        payment.PAYMENTDATETIME,
                //        CustomerName = customer.CustomerName
                //    }
                //).Take(count).ToListAsync();



                var allResults = await _context.GetRecentTransaction
    .FromSqlRaw("EXEC [dbo].[RecentTransactionDetails] @MerchantId", new SqlParameter("@MerchantId", mId))
    .ToListAsync();

                var paymentHistoriesWithCustomerNames = allResults
                    .Take(count)
                    .ToList();


                return Ok(paymentHistoriesWithCustomerNames);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetRecentTransactionsAsync");
                return StatusCode(500, new { message = "An error occurred while retrieving recent transactions.", error = ex.Message });
            }
        }
    }
    public class MiniDashboardResult
    {
        public int Count { get; set; }
    }

    public class TotalRevenueResult
    {
        public decimal? TotalRevenue { get; set; }
    }
    public class GetRecentTransaction
    {
        public DateTime Date { get; set; }
        public string ServiceName { get; set; }
        public string ServiceImage { get; set; }
        public string CustomerName { get; set; }
        public decimal AmountToPay { get; set; }
        public bool Status { get; set; }
    }
    public class MiniDashBoardData
    {
        public int TotalUsersServed { get; set; }
        public int TotalSuccessRequests { get; set; }
        public int TotalFailedRequests { get; set; }
        public int TotalInProgressRequests { get; set; }
    }
}
