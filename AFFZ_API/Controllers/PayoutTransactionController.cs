using AFFZ_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace AFFZ_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayoutTransactionController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly ILogger<PayoutTransactionController> _logger;

        public PayoutTransactionController(MyDbContext context, ILogger<PayoutTransactionController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("GetAllTransactions")]
        public async Task<ActionResult<List<PayoutTransactionViewModel>>> GetAllTransactions()
        {
            List<PayoutTransactionViewModel> transactions = new List<PayoutTransactionViewModel>();

            try
            {
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "EXEC GetPayoutTransactions";
                    command.CommandType = CommandType.StoredProcedure;
                    _context.Database.OpenConnection();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var merchantId = Convert.ToInt32(reader["MERCHANTID"]);
                            var servicePrice = Convert.ToDecimal(reader["ServicePrice"]);
                            var membershipActive = Convert.ToBoolean(reader["IsMembershipActive"]);
                            var discountRate = membershipActive ? Convert.ToDecimal(reader["DiscountRate"]) : 0;
                            var commission = Convert.ToDecimal(reader["PayoutCommission"]);
                            var finalPayout = servicePrice - (servicePrice * commission / 100);

                            transactions.Add(new PayoutTransactionViewModel
                            {
                                Id = Convert.ToInt32(reader["ID"]),
                                MerchantId = merchantId,
                                MerchantName = reader["MerchantName"].ToString(),
                                PayoutMethod = reader["PayoutMethod"].ToString(),
                                Amount = finalPayout,
                                //Commission = commission,
                                Status = reader["Status"].ToString(),
                                CreatedAt = Convert.ToDateTime(reader["PAYMENTDATETIME"])
                            });
                        }
                    }
                }
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching transactions.");
                return StatusCode(500, "Internal server error.");
            }
        }

        //[HttpPost("ProcessPayout")]
        //public async Task<IActionResult> ProcessPayout([FromBody] PayoutRequestModel request)
        //{
        //    try
        //    {
        //        using (var command = _context.Database.GetDbConnection().CreateCommand())
        //        {
        //            command.CommandText = "EXEC ProcessPayout @MerchantId, @Amount, @PayoutMethod";
        //            command.CommandType = CommandType.StoredProcedure;

        //            command.Parameters.Add(new SqlParameter("@MerchantId", request.MerchantId));
        //            command.Parameters.Add(new SqlParameter("@Amount", request.Amount));
        //            command.Parameters.Add(new SqlParameter("@PayoutMethod", request.PayoutMethod));

        //            _context.Database.OpenConnection();
        //            await command.ExecuteNonQueryAsync();
        //        }

        //        return Ok("Payout processed successfully.");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error processing payout.");
        //        return StatusCode(500, "Internal server error.");
        //    }
        //}
    }
}
