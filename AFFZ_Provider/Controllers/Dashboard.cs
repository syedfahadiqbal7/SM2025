using AFFZ_Provider.Models;
using AFFZ_Provider.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AFFZ_Provider.Controllers
{
    [Authorize]
    public class Dashboard : Controller
    {
        private readonly ILogger<Dashboard> _logger;
        private readonly IWebHostEnvironment _environment;
        private static string _merchantIdCat = string.Empty;
        private readonly HttpClient _httpClient;
        IDataProtector _protector;
        public Dashboard(ILogger<Dashboard> logger, IWebHostEnvironment environment, IHttpClientFactory httpClientFactory, IDataProtectionProvider provider)
        {
            _httpClient = httpClientFactory.CreateClient("Main");
            _protector = provider.CreateProtector("Example.SessionProtection");
            _logger = logger;
            _environment = environment;
        }
        public async Task<IActionResult> Index()
        {
            try
            {
                int merchantId = Convert.ToInt32(HttpContext.Session.GetEncryptedString("ProviderId", _protector));
                var GetTotalRevenue = await _httpClient.GetAsync($"Dashboard/GetTotalRevenueAsync?mId=" + merchantId);
                var GetTotalThisWeekRevenue = await _httpClient.GetAsync($"Dashboard/GetTotalRevenueLastWeekAsync?mId=" + merchantId);
                var GetTopRevenueService = await _httpClient.GetAsync($"Dashboard/GetTopRevenueServiceAsync?mid=" + merchantId);
                var GetRecentTransactions = await _httpClient.GetAsync($"Dashboard/GetRecentTransactionsAsync?mId=" + merchantId + "&count=5");
                var GetMiniDashboardStats = await _httpClient.GetAsync($"Dashboard/GetMiniDashboardStats?merchantId=" + merchantId);
                var MiniDashboard = await _httpClient.GetAsync($"Dashboard/statistics?merchantId=" + merchantId);
                if (GetTotalRevenue.IsSuccessStatusCode)
                {
                    ViewBag.GetTotalRevenue = await GetTotalRevenue.Content.ReadAsStringAsync();
                    ViewBag.GetTotalThisWeekRevenue = await GetTotalThisWeekRevenue.Content.ReadAsStringAsync();
                }
                else
                {
                    ViewBag.GetTotalRevenue = "0";
                    ViewBag.GetTotalThisWeekRevenue = "0";
                }

                if (GetTopRevenueService.IsSuccessStatusCode)
                {
                    var TRS = await GetTopRevenueService.Content.ReadAsStringAsync();
                    var reviewsTRS = JsonConvert.DeserializeObject<TopServiceRevenueDto>(TRS);
                    ViewBag.GetTopRevenueService = reviewsTRS?.ServiceName ?? "N/A";
                }
                else
                {
                    ViewBag.GetTopRevenueService = "";
                }

                if (GetRecentTransactions.IsSuccessStatusCode)
                {
                    var RecentTrans = await GetRecentTransactions.Content.ReadAsStringAsync();
                    ViewBag.GetRecentTransactions = JsonConvert.DeserializeObject<List<GetRecentTransaction>>(RecentTrans);
                }
                else
                {
                    ViewBag.GetRecentTransactions = new List<GetRecentTransaction>();
                }

                if (GetMiniDashboardStats.IsSuccessStatusCode)
                {
                    var miniStats = await GetMiniDashboardStats.Content.ReadAsStringAsync();
                    ViewBag.MiniDashboardStats = JsonConvert.DeserializeObject<MiniDashboardStatsViewModel>(miniStats);
                }
                else
                {
                    ViewBag.MiniDashboardStats = new MiniDashboardStatsViewModel();
                }
                if (MiniDashboard.IsSuccessStatusCode)
                {
                    var _miniDashboardData = await MiniDashboard.Content.ReadAsStringAsync();
                    ViewBag.MiniDashBoardData = JsonConvert.DeserializeObject<MiniDashBoardData>(_miniDashboardData);
                }
                else
                {
                    ViewBag.MiniDashBoardData = "";
                }
                try
                {
                    var TopServices = await _httpClient.GetAsync($"ReviewsApi/GetAllReviewsWithAverageRating?merchantId={merchantId}");
                    if (TopServices.IsSuccessStatusCode)
                    {
                        var TopServicesData = await TopServices.Content.ReadAsStringAsync();
                        ViewBag.TopServices = JsonConvert.DeserializeObject<List<ServiceReviewViewModel>>(TopServicesData);
                    }
                    else
                    {
                        _logger.LogError($"Failed to fetch reviews: {TopServices.StatusCode}");
                        ViewBag.TopServices = new List<ServiceReviewViewModel>();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in Fetching TopServices: {ex.Message}");
                    return View("Dashboard", new List<ServiceReviewViewModel>());
                }

                try
                {
                    var RecentBookings = await _httpClient.GetAsync($"CategoryWithMerchant/AllResponsesFromMerchant?Uid=Merchant");
                    string responseString = await RecentBookings.Content.ReadAsStringAsync();
                    List<RequestForDisCountToUserViewModel> documentList = new List<RequestForDisCountToUserViewModel>();
                    if (!string.IsNullOrEmpty(responseString) && !responseString.Contains("No users found with uploaded documents."))
                    {
                        documentList = JsonConvert.DeserializeObject<List<RequestForDisCountToUserViewModel>>(responseString);
                        documentList = documentList.Where(x => x.MerchantID == merchantId).ToList();
                        ViewBag.UsersWithDocuments = documentList;
                    }
                    else
                    {
                        ViewBag.UsersWithDocuments = new List<UserDocumentsViewModel>();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in Fetching Bookings: {ex.Message}");
                    return View("Dashboard", new List<ServiceReviewViewModel>());
                }

                try
                {
                    var ServiceCounts = await _httpClient.GetAsync("Service/TotalServicesForMerchant?id=" + merchantId);
                    string counts = await ServiceCounts.Content.ReadAsStringAsync();
                    ViewBag.TotalService = !string.IsNullOrEmpty(counts) ? counts : "0";
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in Fetching Services Count: {ex.Message}");
                    return View("Dashboard", new List<ServiceReviewViewModel>());
                }

                try
                {
                    var response = await _httpClient.GetAsync($"ReviewsApi/GetAllReviews?merchantId=" + merchantId);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var reviews = JsonConvert.DeserializeObject<List<ReviewViewModel>>(content);
                        if (reviews.Count > 0)
                        {
                            ViewBag.DashboardReview = reviews[0];
                        }
                    }
                    else
                    {
                        var res = await response.Content.ReadAsStringAsync();
                        return View("Dashboard", new List<ReviewViewModel>());
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in Fetching Reviews: {ex.Message}");
                    return View("Dashboard", new List<ServiceReviewViewModel>());
                }

                return View("Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Dashboard.Index: {ex.Message}");
                return View("Dashboard", new List<ServiceReviewViewModel>());
            }
        }

    }
    public class MiniDashBoardData
    {
        public int TotalUsersServed { get; set; }
        public int TotalSuccessRequests { get; set; }
        public int TotalFailedRequests { get; set; }
        public int TotalInProgressRequests { get; set; }
    }

    public class MiniDashboardStatsViewModel
    {
        public int RequestsReceived { get; set; }
        public int EligibilityRequests { get; set; }
        public int DocumentVerification { get; set; }
        public int DocsVerifiedWaiting { get; set; }
        public int CustomersServiceStarted { get; set; }
    }

    public class PaymentHistoryWithCustomer
    {
        public int ID { get; set; }
        public string PAYMENTTYPE { get; set; }
        public string AMOUNT { get; set; }
        public int PAYERID { get; set; }
        public int MERCHANTID { get; set; }
        public int ISPAYMENTSUCCESS { get; set; }
        public int Quantity { get; set; }
        public int SERVICEID { get; set; }
        public DateTime PAYMENTDATETIME { get; set; }
        public string CustomerName { get; set; }
    }
    public class GetRecentTransaction
    {
        public DateTime Date { get; set; }
        public string ServiceName { get; set; }
        public string ServiceImage { get; set; }
        public string CustomerName { get; set; }
        public decimal AmountToPay { get; set; }
        public bool Status { get; set; }
    }
}
