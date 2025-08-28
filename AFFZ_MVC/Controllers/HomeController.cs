using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AFFZ_Customer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("Main");
        }

        // GET: HomeController
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Index method called");

            try
            {


                // Execute all API calls in parallel for better performance
                var tasks = new[]
                {
                    _httpClient.GetAsync("MainPage/GetCategories"),
                    _httpClient.GetAsync("MainPage/GetDashboardStatistics"),
                    _httpClient.GetAsync("MainPage/GetRecentReviews")
                };

                await Task.WhenAll(tasks);

                // Process categories response
                if (tasks[0].Result.IsSuccessStatusCode)
                {
                    var categoriesString = await tasks[0].Result.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(categoriesString))
                    {
                        List<ServiceCat> categories = JsonConvert.DeserializeObject<List<ServiceCat>>(categoriesString);
                        ViewBag.Categories = categories;
                    }
                    else
                    {
                        ViewBag.Categories = new List<ServiceCat>();
                    }
                }
                else
                {
                    ViewBag.Categories = new List<ServiceCat>();
                }

                // Process dashboard statistics response
                if (tasks[1].Result.IsSuccessStatusCode)
                {
                    var statsString = await tasks[1].Result.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(statsString))
                    {
                        var statistics = JsonConvert.DeserializeObject<DashboardStatistics>(statsString);
                        ViewBag.Statistics = statistics;
                    }
                    else
                    {
                        ViewBag.Statistics = GetDefaultStatistics();
                    }
                }
                else
                {
                    ViewBag.Statistics = GetDefaultStatistics();
                }

                // Process recent reviews response
                if (tasks[2].Result.IsSuccessStatusCode)
                {
                    var reviewsString = await tasks[2].Result.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(reviewsString))
                    {
                        var reviews = JsonConvert.DeserializeObject<List<ReviewDto>>(reviewsString);
                        ViewBag.RecentReviews = reviews;
                    }
                    else
                    {
                        ViewBag.RecentReviews = new List<ReviewDto>();
                    }
                }
                else
                {
                    ViewBag.RecentReviews = new List<ReviewDto>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching data. Exception:" + ex.ToString());
                ViewBag.Categories = new List<ServiceCat>();
                ViewBag.Statistics = GetDefaultStatistics();
                ViewBag.RecentReviews = new List<ReviewDto>();
            }

            _logger.LogInformation("Render view.");
            return View();
        }
        // Helper method to get service icon path
        public static string GetServiceIcon(string categoryName)
        {
            var iconMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Visa Application and Renewals", "/wwwrootFront/assets/img/services/visa-application.svg" },
            { "Emirates Id Registration And Renewal", "/wwwrootFront/assets/img/services/emirates-id.svg" },
            { "Documents and Clearance", "/wwwrootFront/assets/img/services/documents-clearance.svg" },
            { "Pro Services", "/wwwrootFront/assets/img/services/pro-services.svg" },
            { "Corporate Typing Services", "/wwwrootFront/assets/img/services/corporate-typing.svg" },
            { "Real Estate Services", "/wwwrootFront/assets/img/services/real-estate.svg" }
        };

            return iconMapping.TryGetValue(categoryName, out var iconPath)
                ? iconPath
                : "/wwwrootFront/assets/img/services/default-service.svg";
        }

        // Helper method to get default statistics
        private DashboardStatistics GetDefaultStatistics()
        {
            return new DashboardStatistics
            {
                TotalCustomers = 0,
                TotalReviews = 0,
                AverageRating = 0.0,
                TotalMerchants = 0
            };
        }
    }

    public class ServiceCat
    {
        public string? CategoryName { get; set; }
        public int CategoryId { get; set; }
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
