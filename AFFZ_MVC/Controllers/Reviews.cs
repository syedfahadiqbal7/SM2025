using AFFZ_Customer.Models;
using AFFZ_Customer.Utils;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AFFZ_Customer.Controllers
{
    [Route("UserReviews")]
    public class Reviews : Controller
    {
        private readonly IDataProtector _protector;
        private readonly ILogger<Reviews> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly HttpClient _httpClient;

        public Reviews(IHttpClientFactory httpClientFactory, IDataProtectionProvider provider, ILogger<Reviews> logger, IWebHostEnvironment environment)
        {
            _protector = provider.CreateProtector("Example.SessionProtection");
            _httpClient = httpClientFactory.CreateClient("Main");
            _logger = logger;
            _environment = environment;
        }
        public async Task<ActionResult> Index(string sort = "Newest", int pageSize = 10, int page = 1)
        {
            ViewBag.CustomerId = HttpContext.Session.GetEncryptedString("UserId", _protector);

            string userId = HttpContext.Session.GetEncryptedString("UserId", _protector);
            try
            {
                // Call the API with the appropriate query parameters
                var jsonResponse = await _httpClient.GetAsync($"ReViewsApi/GetUserReviewList?userId={userId}&sort={sort}&pageSize={pageSize}&page={page}");
                jsonResponse.EnsureSuccessStatusCode();

                if (jsonResponse != null)
                {
                    var responseString = await jsonResponse.Content.ReadAsStringAsync();

                    // Deserialize the response into the PaginatedReviewsDto
                    var paginatedReviews = JsonConvert.DeserializeObject<PaginatedReviewsDto>(responseString);

                    // Set ViewBag properties for pagination
                    ViewBag.TotalPages = (int)Math.Ceiling((double)paginatedReviews.TotalCount / pageSize);
                    ViewBag.CurrentPage = page;
                    ViewBag.PageSize = pageSize;
                    //get service Image

                    // Pass the reviews to the view
                    ViewBag.MyReviews = paginatedReviews.Reviews;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching reviews for userId: {UserId}", userId);
                ViewBag.MyReviews = new List<ReViewDto>();
            }

            return View("Reviews");
        }

        //public async Task<ActionResult> Index()
        //{
        //    ViewBag.CustomerId = HttpContext.Session.GetEncryptedString("UserId", _protector);
        //    ViewBag.FirstName = HttpContext.Session.GetEncryptedString("FirstName", _protector);
        //    ViewBag.MemberSince = HttpContext.Session.GetEncryptedString("MemberSince", _protector);

        //    string userId = HttpContext.Session.GetEncryptedString("UserId", _protector); // Placeholder for session user ID retrieval
        //    try
        //    {
        //        var jsonResponse = await _httpClient.GetAsync($"ReViewsApi/GetUserReviewList?userId={userId}");
        //        jsonResponse.EnsureSuccessStatusCode();
        //        if (jsonResponse != null)
        //        {
        //            var responseString = await jsonResponse.Content.ReadAsStringAsync();
        //            List<ReViewDto> MyReviews = JsonConvert.DeserializeObject<List<ReViewDto>>(responseString);
        //            foreach (var item in MyReviews)
        //            {
        //                item.Service.serviceName = await GetServiceName(item.Service.SID);
        //                item.ReviewText = item.ReviewText.PadRight(64, '.');
        //                item.ServiceImageUrl = await GetServiceImage(item.Service.SID);
        //            }
        //            ViewBag.MyReviews = MyReviews;
        //        }
        //        else
        //        {
        //            _logger.LogWarning("Empty response received from API for userId: {UserId}", userId);
        //            ViewBag.ResponseForDisCountFromMerchant = new List<ReViewDto>();
        //        }
        //    }
        //    catch (JsonSerializationException ex)
        //    {
        //        _logger.LogError(ex, "JSON deserialization error for userId: {UserId}", userId);
        //        ViewBag.ResponseForDisCountFromMerchant = new List<ReViewDto>();
        //        ModelState.AddModelError(string.Empty, "Failed to load data.");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "An unexpected error occurred while fetching data for userId: {UserId}", userId);
        //        ViewBag.ResponseForDisCountFromMerchant = new List<ReViewDto>();
        //        ModelState.AddModelError(string.Empty, "An unexpected error occurred while loading data.");
        //    }

        //    if (TempData.TryGetValue("SaveResponse", out var saveResponse))
        //    {
        //        ViewBag.SaveResponse = saveResponse.ToString();
        //    }




        //    return View("Reviews");
        //}
        private async Task<string> GetServiceName(int ServiceId)
        {
            string ServiceName = string.Empty;
            try
            {
                var jsonResponse = await _httpClient.GetAsync($"ServicesList/GetServiceNameById?id={ServiceId}");
                jsonResponse.EnsureSuccessStatusCode();
                if (jsonResponse != null)
                {
                    ServiceName = await jsonResponse.Content.ReadAsStringAsync();
                }
                else
                {
                    _logger.LogWarning("Empty response received from API.");
                }
            }
            catch (JsonSerializationException ex)
            {
                _logger.LogError(ex, "JSON deserialization error ");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while fetching data.");
            }
            return ServiceName;
        }
        private async Task<string> GetServiceImage(int sID)
        {
            string ServiceImage = string.Empty;
            try
            {
                var jsonResponse = await _httpClient.GetAsync($"ServicesList/GetServiceImageId?id={sID}");
                jsonResponse.EnsureSuccessStatusCode();
                if (jsonResponse != null)
                {
                    ServiceImage = await jsonResponse.Content.ReadAsStringAsync();
                }
                else
                {
                    _logger.LogWarning("Empty response received from API.");
                }
            }
            catch (JsonSerializationException ex)
            {
                _logger.LogError(ex, "JSON deserialization error ");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while fetching data.");
            }
            return ServiceImage;
        }
    }
}
