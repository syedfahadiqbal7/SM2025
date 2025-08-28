using AFFZ_Customer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace AFFZ_Customer.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ILogger<CategoriesController> _logger;
        private readonly string _userUrl;
        private readonly string _adminUrl;
        private readonly string _publicCustomerDomain;
        private readonly HttpClient _httpClient;
        public CategoriesController(ILogger<CategoriesController> logger, IOptions<AppSettings> appSettings, IHttpClientFactory httpClientFactory)
        {
            _userUrl = appSettings.Value.UserUrl;
            _adminUrl = appSettings.Value.AdminHttpsPort;
            _publicCustomerDomain = appSettings.Value.CustomerHttpsPort;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("Main");
        }

        [HttpGet]
        [ResponseCache(NoStore = true, Duration = 0)]
        public async Task<ActionResult> CategoriesIndex(string cityName = "", string serviceCategoryName = "", int rangeStart = -1, int rangeEnd = -1)
        {
            _logger.LogInformation("Index method called with parameters CityName: {CityName}, ServiceCategoryName: {ServiceCategoryName}, RangeStart: {RangeStart}, RangeEnd: {RangeEnd}", cityName, serviceCategoryName, rangeStart, rangeEnd);

            if (rangeStart > rangeEnd)
            {
                _logger.LogWarning("Invalid price range: RangeStart ({RangeStart}) is greater than RangeEnd ({RangeEnd})", rangeStart, rangeEnd);
                return BadRequest("Invalid price range");
            }

            try
            {
                var jsonResponse = await _httpClient.GetAsync($"CategoryServices/AllCategories");
                var getCategoriesResponse = await _httpClient.GetAsync($"MainPage/GetCategories");
                if (!jsonResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to retrieve referral code. StatusCode: {StatusCode}", jsonResponse.StatusCode);
                    return NotFound();
                }

                // Deserialize the response
                var responseString = await jsonResponse.Content.ReadAsStringAsync();
                var getCategoriesResponseString = await getCategoriesResponse.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(responseString) || string.IsNullOrEmpty(getCategoriesResponseString))
                {
                    _logger.LogWarning("Received empty response from API");
                    ViewBag.Categories = new List<AllSubCategories>();
                    return View();
                }
                // Deserialize JSON string to List<ServiceCategory>
                // Remove outer quotes to get the actual JSON string
                var getCategoriesList = JsonConvert.DeserializeObject<List<ServiceCat>>(getCategoriesResponseString);
                var categories = JsonConvert.DeserializeObject<List<AllSubCategories>>(responseString);

                var filterString = new List<int>();

                if (!string.IsNullOrEmpty(serviceCategoryName))
                {
                    var serviceCategoryNameList = serviceCategoryName.Split(',').Select(name => name.Trim()).ToList();
                    filterString = getCategoriesList
                        .Where(c => serviceCategoryNameList.Contains(c.CategoryName, StringComparer.OrdinalIgnoreCase))
                        .Select(c => c.CategoryId)
                        .ToList();
                }

                if (!string.IsNullOrEmpty(cityName))
                {
                    categories = categories.Where(c => c.Location.Contains(cityName, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (filterString.Any())
                {
                    categories = categories.Where(c => filterString.Contains(c.CatId))
                                           .GroupBy(c => c.ServiceName)
                                           .Select(g => g.First())
                                           .ToList();
                }
                else
                {
                    categories = categories.GroupBy(c => c.ServiceName)
                                           .Select(g => g.First())
                                           .ToList();
                }

                if (rangeStart >= 0 && rangeEnd > 0)
                {
                    categories = categories.Where(c => c.ServicePrice >= rangeStart && c.ServicePrice <= rangeEnd).ToList();
                }

                ViewBag.SubCategories = categories;
                ViewBag.Categories = getCategoriesList ?? new List<ServiceCat>();
                ViewBag.CustomerUrl = _userUrl;//_config.GetValue<string>("WebService:WCFUSERNAME")
                ViewBag.AdminUrl = _adminUrl;//_config.GetValue<string>("WebService:WCFUSERNAME")
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the request.");
                return StatusCode(500, "Internal server error.");
            }

            return View();
        }
    }
    public class AllSubCategories
    {
        public string? ServiceName { get; set; }
        public int ServicePrice { get; set; }
        public int CatId { get; set; }
        public string? Location { get; set; }
        public string? ServiceImage { get; set; }
    }
}