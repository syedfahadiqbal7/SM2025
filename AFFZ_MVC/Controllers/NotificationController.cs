using AFFZ_Customer.Models;
using AFFZ_Customer.Utils;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AFFZ_Customer.Controllers
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
            string userId = HttpContext.Session.GetEncryptedString("UserId", _protector); // Placeholder for session user ID retrieval
            if (string.IsNullOrEmpty(userId)) return new List<Notification>();
            var notifications = await _httpClient.GetAsync($"Notifications/GetUserNotifications/{userId}");
            notifications.EnsureSuccessStatusCode();
            if (notifications != null)
            {
                var responseString = await notifications.Content.ReadAsStringAsync();
                notificationsList = JsonConvert.DeserializeObject<List<Notification>>(responseString);
            }
            return notificationsList ?? new List<Notification>();
        }
    }
}
