using AFFZ_Provider.Models;
using AFFZ_Provider.Utils;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AFFZ_Provider.Controllers
{
    public class NotificationController : Controller
    {
        private readonly ILogger<NotificationController> _logger;
        private readonly IWebHostEnvironment _environment;
        private static string _merchantIdCat = string.Empty;
        private readonly HttpClient _httpClient;
        IDataProtector _protector;
        private string BaseUrl = string.Empty;
        private string PublicDomain = string.Empty;
        private string ApiHttpsPort = string.Empty;
        private string CustomerHttpsPort = string.Empty;
        public NotificationController(ILogger<NotificationController> logger, IWebHostEnvironment environment, IHttpClientFactory httpClientFactory, IDataProtectionProvider provider, IAppSettingsService service)
        {
            _httpClient = httpClientFactory.CreateClient("Main");
            _protector = provider.CreateProtector("Example.SessionProtection");
            _logger = logger;
            _environment = environment;
            BaseUrl = service.GetBaseIpAddress();
            PublicDomain = service.GetPublicDomain();
            ApiHttpsPort = service.GetApiHttpsPort();
            CustomerHttpsPort = service.GetCustomerHttpsPort();
        }
        public async Task<IActionResult> MyNotifications()
        {
            List<Notification> list = await GetNotificationsAsync();
            return View(list);
        }
        public async Task<List<Notification>> GetNotificationsAsync()
        {
            List<Notification> notificationsList = new List<Notification>();
            string userId = HttpContext.Session.GetEncryptedString("ProviderId", _protector);
            if (string.IsNullOrEmpty(userId)) return new List<Notification>();
            try
            {
                var response = await _httpClient.GetAsync($"Notifications/GetMerchantNotifications/{userId}");
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                notificationsList = JsonConvert.DeserializeObject<List<Notification>>(responseString);
            }
            catch (Exception ex)
            {
                // Log exception if needed
                Console.WriteLine($"Error fetching notifications: {ex.Message}");
            }

            return notificationsList ?? new List<Notification>();
        }
    }
}
