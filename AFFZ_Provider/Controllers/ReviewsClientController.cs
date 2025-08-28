using AFFZ_Provider.Models;
using AFFZ_Provider.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AFFZ_Provider.Controllers
{
    [Authorize]
    public class ReviewsClientController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private static string _merchantIdCat = string.Empty;
        private readonly HttpClient _httpClient;
        private readonly IDataProtector _protector;
        private ILogger<ReviewsClientController> _logger;
        public ReviewsClientController(ILogger<ReviewsClientController> logger, IWebHostEnvironment environment, IHttpClientFactory httpClientFactory, IDataProtectionProvider provider)
        {
            _httpClient = httpClientFactory.CreateClient("Main");
            _protector = provider.CreateProtector("Example.SessionProtection");
            _logger = logger;
            _environment = environment;
        }
        //[HttpGet("ProviderReviews")]
        public async Task<IActionResult> ProviderReviews()
        {
            try
            {
                int merchantId = Convert.ToInt32(HttpContext.Session.GetEncryptedString("ProviderId", _protector)); // Placeholder for session merchant ID retrieval
                var response = await _httpClient.GetAsync($"ReviewsApi/GetAllReviews?merchantId=" + merchantId);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var reviews = JsonConvert.DeserializeObject<List<ReviewViewModel>>(content);
                    foreach (var item in reviews)
                    {
                        item.ServiceName = await GetServiceName(item.SID);
                        item.ServiceImageUrl = await GetServiceImage(item.SID);
                        item.ReviewText = item.ReviewText.PadRight(64, '.');
                    }
                    //ViewBag.MyReviews = MyReviews;
                    return View(reviews);
                }
                else
                {
                    return View(new List<ReviewViewModel>());
                }
            }
            catch (Exception)
            {

                throw;
            }
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
    }
}
