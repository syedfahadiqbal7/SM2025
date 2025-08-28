using AFFZ_Provider.Models;
using Microsoft.AspNetCore.DataProtection;
using Newtonsoft.Json;

namespace AFFZ_Provider.Utils
{
    public class NotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        IDataProtector _protector;
        private string AdminHttpsPort = string.Empty;
        private string CustomerHttpsPort = string.Empty;
        public NotificationService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, IDataProtectionProvider provider, IHttpClientFactory httpClientFactory, IAppSettingsService service)
        {
            _httpClient = httpClientFactory.CreateClient("Main");
            _httpContextAccessor = httpContextAccessor;
            _protector = provider.CreateProtector("Example.SessionProtection");
            CustomerHttpsPort = service.GetCustomerHttpsPort();
        }
        public string GetMerchantId()
        {
            try
            {
                return _httpContextAccessor.HttpContext.Session.GetEncryptedString("ProviderId", _protector);
            }
            catch (Exception ex)
            {
                return "";
            }

        }
        public async Task<List<Notification>> GetNotificationsAsync()
        {
            List<Notification> notificationsList = new List<Notification>();
            string userId = _httpContextAccessor.HttpContext.Session.GetEncryptedString("ProviderId", _protector);
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
        public async Task<string> GetUserName()
        {

            try
            {
                return _httpContextAccessor.HttpContext.Session.GetEncryptedString("ProviderName", _protector);
            }
            catch (Exception ex)
            {
                // Log exception if needed
                Console.WriteLine($"Error fetching notifications: {ex.Message}");
                return ex.Message;
            }


        }
        public async Task<string> GetProviderEmail()
        {

            try
            {
                return _httpContextAccessor.HttpContext.Session.GetEncryptedString("Email", _protector);
            }
            catch (Exception ex)
            {
                // Log exception if needed
                Console.WriteLine($"Error fetching Provider Details: {ex.Message}");
                return ex.Message;
            }


        }
        public async Task<string> GetProviderIsActive()
        {

            try
            {
                string IsActive = ((_httpContextAccessor.HttpContext.Session.GetEncryptedString("IsActive", _protector)) == "True" ? "Active" : "In Active");
                return IsActive;
            }
            catch (Exception ex)
            {
                // Log exception if needed
                Console.WriteLine($"Error fetching Provider Details: {ex.Message}");
                return ex.Message;
            }


        }
        public async Task<string> GetadminLink()
        {
            return AdminHttpsPort;
        }
        public async Task<string> GetCustomerLink()
        {
            return CustomerHttpsPort;
        }
    }
}
