using AFFZ_API.Models;
using AFFZ_API.Models.Partial;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace AFFZ_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        private readonly MyDbContext _context;

        public WalletController(MyDbContext context)
        {
            _context = context;
        }

        // Get Wallet Points by Customer ID
        [HttpGet("GetWalletPoints")]
        public async Task<ActionResult<SResponse>> GetWalletPoints(int customerId)
        {
            try
            {
                var wallet = await _context.Wallet.FirstOrDefaultAsync(w => w.CustomerID == customerId);

                if (wallet == null)
                {
                    return new SResponse
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = "Wallet not found"
                    };
                }

                return new SResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Data = wallet
                };
            }
            catch (Exception ex)
            {
                return new SResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = $"Exception: {ex.Message}",
                };
            }
        }
        // Use Referral Points for Transaction
        [HttpPost]
        [Route("UsePoints")]
        public async Task<ActionResult<SResponse>> UseReferralPoints(int customerId, decimal pointsToUse)
        {
            try
            {
                var commandText = "EXEC UseReferralPoints @CustomerID, @PointsToUse";
                var customerIdParam = new SqlParameter("@CustomerID", customerId);
                var pointsParam = new SqlParameter("@PointsToUse", pointsToUse);

                await _context.Database.ExecuteSqlRawAsync(commandText, customerIdParam, pointsParam);

                return new SResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = "Points successfully used for transaction."
                };
            }
            catch (Exception ex)
            {
                return new SResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = $"Exception: {ex.Message}",
                };
            }
        }
    }
}
