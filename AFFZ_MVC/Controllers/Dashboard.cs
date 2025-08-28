using AFFZ_Customer.Models.Partial;
using AFFZ_Customer.Utils;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AFFZ_Customer.Controllers
{
    [Route("UserDashboard")]
    public class Dashboard : Controller
    {
        private readonly IDataProtector _protector;
        private readonly HttpClient _httpClient;
        private readonly ILogger<Dashboard> _logger;

        public Dashboard(IHttpClientFactory httpClientFactory, IDataProtectionProvider provider, ILogger<Dashboard> logger)
        {
            _protector = provider.CreateProtector("Example.SessionProtection");
            _httpClient = httpClientFactory.CreateClient("Main");
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            string userIdStr = HttpContext.Session.GetEncryptedString("UserId", _protector);
            if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId))
            {
                ViewBag.CustomerId = userIdStr;
                ViewBag.FirstName = HttpContext.Session.GetEncryptedString("FirstName", _protector);
                ViewBag.MemberSince = HttpContext.Session.GetEncryptedString("MemberSince", _protector);


                // Fetch Total Orders
                var totalOrdersResponse = await _httpClient.GetAsync($"DashboardApi/GetTotalOrders/{userId}");
                int totalOrders = 0;
                if (totalOrdersResponse.IsSuccessStatusCode)
                {
                    totalOrders = JsonConvert.DeserializeObject<int>(await totalOrdersResponse.Content.ReadAsStringAsync());
                }
                ViewBag.TotalOrders = totalOrders;

                // Fetch Total Spend
                var totalSpendResponse = await _httpClient.GetAsync($"DashboardApi/GetTotalSpend/{userId}");
                decimal totalSpend = 0;
                if (totalSpendResponse.IsSuccessStatusCode)
                {
                    totalSpend = JsonConvert.DeserializeObject<decimal>(await totalSpendResponse.Content.ReadAsStringAsync());
                }
                ViewBag.TotalSpend = totalSpend;

                // Fetch Wallet Points
                var walletResponse = await _httpClient.GetAsync($"DashboardApi/GetWalletPoints/{userId}");
                decimal walletPoints = 0;
                if (walletResponse.IsSuccessStatusCode)
                {
                    walletPoints = JsonConvert.DeserializeObject<decimal>(await walletResponse.Content.ReadAsStringAsync());
                }
                ViewBag.WalletPoints = walletPoints;

                // Fetch Recent Bookings
                var bookingsResponse = await _httpClient.GetAsync($"DashboardApi/GetRecentBookings/{userId}");
                List<RecentBookingDto> recentBookings = new();
                if (bookingsResponse.IsSuccessStatusCode)
                {
                    recentBookings = JsonConvert.DeserializeObject<List<RecentBookingDto>>(await bookingsResponse.Content.ReadAsStringAsync());
                }
                ViewBag.RecentBookings = recentBookings;

                _logger.LogInformation("CustomerId exists. Redirecting to Dashboard.");
                return View("Dashboard");
            }
            else
            {
                return View("Login", new LoginModel());
            }
        }
    }
    public class RecentBookingDto
    {
        public string ServiceImage { get; set; }
        public string ServiceName { get; set; }
        public DateTime ResponseDateTime { get; set; }
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public string ProfilePicture { get; set; }
    }
}
