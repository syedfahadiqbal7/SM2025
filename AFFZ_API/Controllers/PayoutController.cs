using AFFZ_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace AFFZ_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayoutController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly ILogger<PayoutController> _logger;

        public PayoutController(MyDbContext context, ILogger<PayoutController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get the Payout Settings
        /// </summary>
        [HttpGet("GetPayoutSettings")]
        public async Task<ActionResult<PayoutSettings>> GetPayoutSettings()
        {
            try
            {
                PayoutSettings payoutSettings = new PayoutSettings();
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "GetPayoutSettings";
                    command.CommandType = CommandType.StoredProcedure;

                    _context.Database.OpenConnection();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            payoutSettings.Id = Convert.ToInt32(reader["Id"]);
                            payoutSettings.PayoutNoOfDays = Convert.ToInt32(reader["PayoutNoOfDays"]);
                            payoutSettings.PayoutsPerMonth = Convert.ToInt32(reader["PayoutsPerMonth"]);
                            payoutSettings.MinimumPayoutAmount = Convert.ToDecimal(reader["MinimumPayoutAmount"]);
                            payoutSettings.PayoutCommission = Convert.ToDecimal(reader["PayoutCommission"]);
                            payoutSettings.IsEnabled = Convert.ToBoolean(reader["IsEnabled"]);
                        }
                    }
                }
                return Ok(payoutSettings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching payout settings.");
                return StatusCode(500, "Internal server error.");
            }
        }

        /// <summary>
        /// Update Payout Settings
        /// </summary>
        [HttpPost("UpdatePayoutSettings")]
        public async Task<IActionResult> UpdatePayoutSettings([FromBody] PayoutSettings settings)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync("EXEC UpdatePayoutSettings @PayoutNoOfDays, @PayoutsPerMonth, @MinimumPayoutAmount, @PayoutCommission, @IsEnabled, @AutoProcess",
                    new SqlParameter("@PayoutNoOfDays", settings.PayoutNoOfDays),
                    new SqlParameter("@PayoutsPerMonth", settings.PayoutsPerMonth),
                    new SqlParameter("@MinimumPayoutAmount", settings.MinimumPayoutAmount),
                    new SqlParameter("@PayoutCommission", settings.PayoutCommission),
                    new SqlParameter("@IsEnabled", settings.IsEnabled),
                    new SqlParameter("@AutoProcess", settings.AutoProcess));

                return Ok("Payout settings updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payout settings.");
                return StatusCode(500, "Internal server error.");
            }
        }

        /// <summary>
        /// Create a Payout Transaction
        /// </summary>
        [HttpPost("CreatePayoutTransaction")]
        public async Task<IActionResult> CreatePayoutTransaction([FromBody] PayoutTransaction transaction)
        {
            try
            {
                var minPayoutParam = new SqlParameter("@MinPayoutAmount", SqlDbType.Decimal)
                {
                    Direction = ParameterDirection.Output,
                    Precision = 18,
                    Scale = 2
                };

                await _context.Database.ExecuteSqlRawAsync("EXEC InsertPayoutTransaction @MerchantId, @PayoutMethod, @Amount",
                    new SqlParameter("@MerchantId", transaction.MerchantId),
                    new SqlParameter("@PayoutMethod", transaction.PayoutMethod),
                    new SqlParameter("@Amount", transaction.Amount));

                return Ok("Payout transaction created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payout transaction.");
                return StatusCode(500, "Internal server error.");
            }
        }

        /// <summary>
        /// Update Payout Transaction Status
        /// </summary>
        [HttpPost("UpdatePayoutStatus")]
        public async Task<IActionResult> UpdatePayoutStatus(int id, string status)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync("EXEC UpdatePayoutStatus @Id, @Status",
                    new SqlParameter("@Id", id),
                    new SqlParameter("@Status", status));

                return Ok("Payout status updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payout status.");
                return StatusCode(500, "Internal server error.");
            }
        }

        /// <summary>
        /// Get All Payout Transactions
        /// </summary>
        [HttpGet("GetPayoutTransactions")]
        public async Task<ActionResult<List<PayoutTransactionViewModel>>> GetPayoutTransactions()
        {
            try
            {
                var transactions = new List<PayoutTransactionViewModel>();

                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "GetPayoutTransactions";
                    command.CommandType = CommandType.StoredProcedure;
                    _context.Database.OpenConnection();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            transactions.Add(new PayoutTransactionViewModel
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                MerchantId = Convert.ToInt32(reader["MerchantId"]),
                                MerchantName = reader["MerchantName"].ToString(),
                                PayoutMethod = reader["PayoutMethod"].ToString(),
                                Amount = Convert.ToDecimal(reader["Amount"]),
                                Status = reader["Status"].ToString(),
                                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                            });
                        }
                    }
                }

                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching payout transactions.");
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
