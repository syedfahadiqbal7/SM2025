using AFFZ_Customer.Models;
using AFFZ_Customer.Models.Partial;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AFFZ_Customer.Utils
{
    public class NotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        IDataProtector _protector;
        private string AdminHttpsPort = string.Empty;
        public NotificationService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, IDataProtectionProvider provider, IHttpClientFactory httpClientFactory, IAppSettingsService service)
        {
            _httpClient = httpClientFactory.CreateClient("Main");
            _httpContextAccessor = httpContextAccessor;
            _protector = provider.CreateProtector("Example.SessionProtection");
            AdminHttpsPort = service.GetAdminHttpsPort();
        }
        public async Task<string> GetUserId()
        {
            return _httpContextAccessor.HttpContext.Session.GetEncryptedString("UserId", _protector);
        }
        public async Task<string> GetadminLink()
        {
            return AdminHttpsPort;
        }
        public async Task<List<Notification>> GetNotificationsAsync()
        {
            List<Notification> notificationsList = new List<Notification>();
            string userId = _httpContextAccessor.HttpContext.Session.GetEncryptedString("UserId", _protector); // Placeholder for session user ID retrieval
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
        public async Task<string> GetUserName()
        {

            try
            {
                return _httpContextAccessor.HttpContext.Session.GetEncryptedString("CustomerName", _protector);
            }
            catch (Exception ex)
            {
                // Log exception if needed
                Console.WriteLine($"Error fetching notifications: {ex.Message}");
                return ex.Message;
            }
        }
        [HttpGet]
        public async Task<string> GetMemberSince()
        {
            try
            {
                return _httpContextAccessor.HttpContext.Session.GetEncryptedString("MemberSince", _protector);
            }
            catch (Exception ex)
            {
                // Log exception if needed
                Console.WriteLine($"Error fetching notifications: {ex.Message}");
                return ex.Message;
            }
        }
        [HttpGet]
        public async Task<string> GetReferralCode()
        {
            try
            {
                return _httpContextAccessor.HttpContext.Session.GetEncryptedString("ReferralCode", _protector);
            }
            catch (Exception ex)
            {
                // Log exception if needed
                Console.WriteLine($"Error fetching ReferralCode : {ex.Message}");
                return ex.Message;
            }
        }
        [HttpGet]
        public async Task<string> GetWalletPoints()
        {
            try
            {
                return _httpContextAccessor.HttpContext.Session.GetEncryptedString("walletPoints", _protector);
            }
            catch (Exception ex)
            {
                // Log exception if needed
                Console.WriteLine($"Error fetching ReferralCode : {ex.Message}");
                return ex.Message;
            }
        }
        [HttpGet]
        public async Task<int> GetCartItemCount()
        {
            string userId = _httpContextAccessor.HttpContext.Session.GetEncryptedString("UserId", _protector);
            if (string.IsNullOrEmpty(userId)) return 0;

            var response = await _httpClient.GetAsync($"Cart/GetCartItems/{userId}");
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var cartItems = JsonConvert.DeserializeObject<SResponse>(responseString);

            if (cartItems.Data is JArray cartItemsArray)
            {
                return cartItemsArray.Count;
            }

            return 0;
        }

    }
}
