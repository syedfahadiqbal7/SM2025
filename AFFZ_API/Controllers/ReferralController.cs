using AFFZ_API.Models;
using AFFZ_API.Models.Partial;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace AFFZ_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReferralController : ControllerBase
    {
        private readonly MyDbContext _context;

        public ReferralController(MyDbContext context)
        {
            _context = context;
        }

        // Create Referral
        [HttpPost]
        [Route("Create")]
        public async Task<ActionResult<SResponse>> CreateReferral(int referrerCustomerId, int referredCustomerId)
        {
            try
            {
                // Ensure the referred customer has not been referred before
                var existingReferral = await _context.Referral
                    .FirstOrDefaultAsync(r => r.ReferredCustomerID == referredCustomerId);

                if (existingReferral != null)
                {
                    return new SResponse
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Message = "This customer has already been provided a referral code."
                    };
                }

                var referralCode = Guid.NewGuid().ToString();
                var referral = new Referral
                {
                    ReferrerCustomerID = referrerCustomerId,
                    ReferredCustomerID = referredCustomerId,
                    ReferralCode = referralCode,
                    ReferralStatus = "Pending",
                    CreatedDate = DateTime.Now
                };
                _context.Referral.Add(referral);
                await _context.SaveChangesAsync();

                return new SResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = "Referral created successfully",
                    Data = referral
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
        [HttpGet]
        [Route("GetUserReferral")]
        public async Task<ActionResult<SResponse>> GetUserReferral(int CustomerId)
        {
            string ReferralCode = string.Empty;
            try
            {
                ReferralCode = await _context.Referral.Where(r => r.ReferrerCustomerID == CustomerId).Select(x => x.ReferralCode).FirstOrDefaultAsync();
                return new SResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = "Referral created successfully",
                    Data = ReferralCode
                };
            }
            catch (Exception ex)
            {
                ReferralCode = ex.Message;
                return new SResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = $"Exception: {ex.Message}",
                };
            }
        }
        // Update Referral to Completed after first purchase
        [HttpPost]
        [Route("Complete")]
        public async Task<ActionResult<SResponse>> CompleteReferral(int referredCustomerId)
        {
            try
            {
                var referral = await _context.Referral
                    .FirstOrDefaultAsync(r => r.ReferredCustomerID == referredCustomerId && r.ReferralStatus == "Pending");

                if (referral == null)
                {
                    return new SResponse
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Message = "Referral not found or already completed."
                    };
                }

                referral.ReferralStatus = "Completed";
                await _context.SaveChangesAsync();

                return new SResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = "Referral status updated successfully"
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
        [HttpGet]
        [Route("CheckReferral")]
        public async Task<bool> CheckReferral(string referrercode)
        {
            try
            {
                return _context.Referral.Where(r => r.ReferralCode == referrercode).Any();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
