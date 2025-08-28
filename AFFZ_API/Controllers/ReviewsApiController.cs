using AFFZ_API.Interfaces;
using AFFZ_API.Models;
using AFFZ_API.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text.Json;

namespace AFFZ_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsApiController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly ILogger<ReviewsApiController> _logger;
        private readonly IEmailService _emailService;
        public ReviewsApiController(MyDbContext context, ILogger<ReviewsApiController> logger, IEmailService emailService)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
        }

        [HttpGet("GetAllReviews")]
        public async Task<IActionResult> GetAllReviews(int merchantId)
        {
            try
            {
                var reviews = new List<ReviewViewModel>();

                var param = new SqlParameter("@MerchantId", merchantId);

                reviews = await _context.Set<ReviewViewModel>()
                    .FromSqlRaw("EXEC GetAllReviewsByMerchant @MerchantId", param)
                    .ToListAsync();

                if (!reviews.Any())
                {
                    return NotFound("No reviews found.");
                }

                return Ok(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching reviews.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("GetUserReviewList")]
        public async Task<IActionResult> GetUserReviewList(int userId, string sort = "Newest", int pageSize = 10, int page = 1)
        {
            try
            {
                var reviews = await _context.Review
                    .Include(s => s.Service)
                    .Include(s => s.CUser)
                    .Where(s => s.CustomerId == userId)
                    .ToListAsync();

                reviews = sort switch
                {
                    "Oldest" => reviews.OrderBy(r => r.ReviewDate).ToList(),
                    "Highest" => reviews.OrderByDescending(r => r.Rating).ToList(),
                    "Lowest" => reviews.OrderBy(r => r.Rating).ToList(),
                    _ => reviews.OrderByDescending(r => r.ReviewDate).ToList(),
                };

                var pagedReviews = reviews.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                return Ok(new
                {
                    TotalCount = reviews.Count,
                    Reviews = pagedReviews
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in {nameof(GetUserReviewList)}: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        //[HttpGet("GetUserReviewList")]
        //public async Task<IActionResult> GetUserReviewList(int userId)
        //{
        //    try
        //    {
        //        var _review = await _context.Review
        //            .Include(s => s.Service) // Include related reviews
        //            .Include(s => s.CUser)
        //            .Where(s => s.CustomerId == s.CUser.CustomerId && s.CustomerId == userId)
        //            .ToListAsync();
        //        if (_review == null)
        //        {
        //            return NotFound($"Review not found.");
        //        }

        //        var reviews = _review; // Get reviews related to the service
        //        return Ok(reviews);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Error in {nameof(GetUserReviewList)}: {ex.Message}");
        //        return StatusCode(500, "Internal server error");
        //    }
        //}

        [HttpGet("GetReviewByServiceId")]
        public async Task<IActionResult> GetReviewByServiceId(int serviceId)
        {
            try
            {
                var _review = await _context.Review
                    .Include(s => s.Service) // Include related reviews
                    .FirstOrDefaultAsync(s => s.ServiceId == serviceId);

                if (_review == null)
                {
                    return NotFound($"Review with Service ID {serviceId} not found.");
                }

                var reviews = _review; // Get reviews related to the service
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in {nameof(GetReviewByServiceId)}: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("CreateReview")]
        public async Task<IActionResult> CreateOrUpdateReview([FromBody] ReviewCreate _review)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var existingReview = await _context.Review
                    .FirstOrDefaultAsync(r => r.ServiceId == _review.ServiceId && r.CustomerId == _review.CustomerId && r.merchantId == _review.merchantId && r.RFDFU == _review.RFDFU);

                if (existingReview != null)
                {
                    // Update existing review
                    existingReview.ReviewText = _review.ReviewText;
                    existingReview.Rating = _review.Rating;
                    existingReview.ReviewDate = _review.ReviewDate;

                    _context.Review.Update(existingReview);
                    await _context.SaveChangesAsync();
                    await SendNotificationEmailToMerchant(_review);
                    return Ok(new
                    {
                        message = "Review updated successfully",
                        review = existingReview
                    });
                }
                else
                {
                    // Create a new review
                    Review review = new Review
                    {
                        ServiceId = _review.ServiceId,
                        CustomerId = _review.CustomerId,
                        merchantId = _review.merchantId,
                        RFDFU = _review.RFDFU,
                        ReviewText = _review.ReviewText,
                        Rating = _review.Rating,
                        ReviewDate = _review.ReviewDate,
                        CUser = await _context.Customers.FirstOrDefaultAsync(x => x.CustomerId == _review.CustomerId),
                        Service = await _context.Services.FirstOrDefaultAsync(x => x.ServiceId == _review.ServiceId)
                    };

                    _context.Review.Add(review);
                    await _context.SaveChangesAsync();
                    await SendNotificationEmailToMerchant(_review);
                    return CreatedAtAction(nameof(GetAllReviews), new { id = review.ReviewId }, review);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in {nameof(CreateOrUpdateReview)}: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("CheckIfReviewed")]
        public async Task<IActionResult> CheckIfReviewed(int serviceId, int customerId)
        {
            try
            {
                var review = await _context.Review.FirstOrDefaultAsync(r => r.ServiceId == serviceId && r.CustomerId == customerId);
                return Ok(new { HasReviewed = review != null });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in {nameof(CheckIfReviewed)}: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("GetAllReviewsWithAverageRating")]
        public async Task<IActionResult> GetAllReviewsWithAverageRating(int merchantId)
        {
            try
            {
                var param = new SqlParameter("@MerchantId", merchantId);

                var rawData = await _context.Set<ReviewSummaryDto>()
                    .FromSqlRaw("EXEC GetAllReviewsWithAverageRating @MerchantId", param)
                    .ToListAsync();

                foreach (var item in rawData)
                {
                    if (!string.IsNullOrEmpty(item.ReviewsJson))
                    {
                        item.Reviews = JsonSerializer.Deserialize<List<ReviewDetailDto>>(item.ReviewsJson,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });
                    }
                }

                if (rawData == null || rawData.Count == 0)
                {
                    _logger.LogInformation($"No reviews found for merchantId: {merchantId}");
                    return NotFound("No reviews found.");
                }

                return Ok(rawData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in {nameof(GetAllReviewsWithAverageRating)}");
                return StatusCode(500, "Internal server error");
            }
        }

        private async Task<bool> SendNotificationEmailToMerchant(ReviewCreate notification)
        {
            try
            {
                string RedirectUrlLoc = string.Empty;


                EmailTemplate emailTemplate = new EmailTemplate();
                string userName = string.Empty;
                string EmailAddress = string.Empty;
                string _Message = $"Review-{notification.ReviewText}.<br />Rating Stars - {notification.Rating} out of 5";


                int CID = Convert.ToInt32(notification.CustomerId);
                int MID = Convert.ToInt32(notification.merchantId);
                userName = _context.ProviderUsers.FirstOrDefault(p => p.ProviderId == MID)?.ProviderName;
                string SenderName = _context.Customers.FirstOrDefault(c => c.CustomerId == CID)?.CustomerName;
                EmailAddress = _context.ProviderUsers.FirstOrDefault(p => p.ProviderId == MID)?.Email;
                string EmailTemplate = "<!DOCTYPE html>\n<html>\n<head>\n<style>\nbody { font-family: Arial, sans-serif; background-color: #f8f9fa; margin: 0; padding: 0; }\n.email-container { max-width: 600px; margin: 20px auto; background: #ffffff; border-radius: 10px; box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1); padding: 20px; }\n.header { text-align: center; color: #343a40; margin-bottom: 20px; }\n.header h1 { font-size: 24px; }\n.content { color: #555555; line-height: 1.6; }\n.footer { margin-top: 20px; text-align: center; font-size: 12px; color: #888888; }\n</style>\n</head>\n<body>\n<div class='email-container'>\n<div class='header'><h1>Review</h1></div>\n<div class='content'>\n<p style='font-weight:bold;'>Hello <strong>{{Name}}</strong>,</p>\n<p>" + _Message + "</p>\n</div>\n<div class='footer'><p>&copy; {{CurrentYear}} SmartCenter. All Rights Reserved.</p></div>\n</div>\n</body>\n</html>";
                emailTemplate.Body = EmailTemplate;
                emailTemplate.Subject = $"Customer review for the service.";

                // Replace Placeholders
                string emailBody = emailTemplate.Body
                    .Replace("{{Name}}", userName ?? "Application Merchant")
                    .Replace("{{ResetLink}}", RedirectUrlLoc)
                    .Replace("{{CurrentYear}}", DateTime.UtcNow.Year.ToString());

                // Simulate Email Sending
                _logger.LogInformation("Sending Email to: {Email}, Subject: {Subject}", EmailAddress, emailTemplate.Subject);
                _logger.LogInformation("Email Body: {Body}", emailBody);

                // Use your IEmailService here to send the email
                // Example:
                bool emailSent = await _emailService.SendEmail(EmailAddress, emailTemplate.Subject, emailBody, userName, isHtml: true);

                // Simulated Success
                return emailSent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email for notification.");
                return false;
            }
        }
    }
}
