using AFFZ_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
namespace AFFZ_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MainPageController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly ILogger<MainPageController> _logger;
        public MainPageController(MyDbContext context, ILogger<MainPageController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Home
        [HttpGet("GetCities/{Prefix?}")]
        public IActionResult GetCities(string Prefix = "")
        {
            // Note: you can bind the same list from the database
            List<City> ObjList = new List<City>()
        {
            new City {Id=1,Name="Sharjah" },
            new City {Id=2,Name="Dubai" },
            new City {Id=3,Name="Abu Dhabi" },
            new City {Id=4,Name="Fujairah" },
            new City {Id=5,Name="Ajman" },
            new City {Id=6,Name="Ras al Khaimah" },
            new City {Id=7,Name="Umm al-Quwain" }
        };

            // Conditional check for the prefix
            // Convert Prefix to lowercase to make the search case-insensitive
            Prefix = Prefix?.ToLower();

            // Conditional check for the prefix with case-insensitive comparison
            var result = string.IsNullOrEmpty(Prefix)
                ? ObjList.Select(c => new { c.Name })
                : ObjList.Where(c => c.Name.ToLower().StartsWith(Prefix)).Select(c => new { c.Name });

            string json = JsonConvert.SerializeObject(result);
            return Ok(json);
        }

        [HttpGet("GetCategories/{Prefix?}")]
        public async Task<IActionResult> GetCategories(string Prefix = "")
        {
            try
            {
                _logger.LogInformation("Request Recieved at GetCategories");
                List<ServiceCategory> ObjList = await _context.ServiceCategories.ToListAsync();
                _logger.LogInformation("Fetched records from database.");
                // Convert Prefix to lowercase to make the search case-insensitive
                Prefix = Prefix?.ToLower();
                var result = string.IsNullOrEmpty(Prefix)
                    ? ObjList.Select(c => new { c.CategoryName, c.CategoryId })
                    : ObjList.Where(c => c.CategoryName.ToLower().StartsWith(Prefix)).Select(c => new { c.CategoryName, c.CategoryId });

                string json = JsonConvert.SerializeObject(result);
                _logger.LogInformation("Sending Response to Frontend.");
                return Ok(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching categories. Exception:" + ex.ToString());
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("GetDashboardStatistics")]
        public async Task<ActionResult<DashboardStatistics>> GetDashboardStatistics()
        {
            try
            {
                // Single optimized query for all statistics
                var statistics = await _context.Database
                    .SqlQuery<DashboardStatistics>($@"
                        SELECT 
                            (SELECT COUNT(*) FROM Customers) as TotalCustomers,
                            (SELECT COUNT(*) FROM MerchantRatings) + 
                            (SELECT COUNT(*) FROM MerchantUserRatings) as TotalReviews,
                            (SELECT AVG(CAST(RatingValue AS FLOAT)) FROM 
                                (SELECT RatingValue FROM MerchantRatings WHERE RatingValue > 0
                                 UNION ALL 
                                 SELECT RatingValue FROM MerchantUserRatings WHERE RatingValue > 0) as AllRatings) as AverageRating,
                            (SELECT COUNT(*) FROM Merchants WHERE IsActive = 1) as TotalMerchants")
                    .FirstOrDefaultAsync();

                // Set default values if query returns null
                if (statistics == null)
                {
                    statistics = new DashboardStatistics
                    {
                        TotalCustomers = 0,
                        TotalReviews = 0,
                        AverageRating = 0.0,
                        TotalMerchants = 0
                    };
                }

                // Round average rating to 1 decimal place
                statistics.AverageRating = Math.Round(statistics.AverageRating, 1);

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dashboard statistics");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("GetRecentReviews")]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetRecentReviews()
        {
            try
            {
                var recentReviews = await _context.MerchantRatings
                    .Include(r => r.RatedByUser)
                    .Include(r => r.RatedMerchant)
                    .OrderByDescending(r => r.RatedDate)
                    .Take(6)
                    .Select(r => new ReviewDto
                    {
                        Id = r.RatingId,
                        CustomerName = r.RatedByUser.Username,
                        MerchantName = r.RatedMerchant.CompanyName,
                        Rating = r.RatingValue,
                        Comment = r.Comments ?? "Great service!",
                        ServiceType = "General Service",
                        ReviewDate = r.RatedDate
                    })
                    .ToListAsync();

                return Ok(recentReviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching recent reviews");
                return StatusCode(500, "Internal server error");
            }
        }
    }

    public class DashboardStatistics
    {
        public int TotalCustomers { get; set; }
        public int TotalReviews { get; set; }
        public double AverageRating { get; set; }
        public int TotalMerchants { get; set; }
    }

    public class ReviewDto
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string MerchantName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public DateTime ReviewDate { get; set; }
    }
}
