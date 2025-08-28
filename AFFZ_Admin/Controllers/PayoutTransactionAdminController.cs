using AFFZ_Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AFFZ_Admin.Controllers
{
    public class PayoutTransactionAdminController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PayoutTransactionAdminController> _logger;

        public PayoutTransactionAdminController(IHttpClientFactory httpClientFactory, ILogger<PayoutTransactionAdminController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("Main");
            _logger = logger;
        }

        // GET: Fetch Payout Transactions
        public async Task<IActionResult> Index()
        {
            List<PayoutTransactionViewModel> transactions = new List<PayoutTransactionViewModel>();

            try
            {
                var response = await _httpClient.GetAsync("PayoutTransaction/GetAllTransactions");
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    transactions = JsonConvert.DeserializeObject<List<PayoutTransactionViewModel>>(responseString);
                }
                else
                {
                    TempData["FailMessage"] = "Failed to fetch transactions.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching transactions.");
                TempData["FailMessage"] = "Error fetching transactions.";
            }

            return View(transactions);
        }

        // POST: Process Payout
        [HttpPost]
        public async Task<IActionResult> ProcessPayout(int merchantId, decimal amount, string payoutMethod)
        {
            var payoutRequest = new { MerchantId = merchantId, Amount = amount, PayoutMethod = payoutMethod };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("PayoutTransaction/ProcessPayout", payoutRequest);
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Payout processed successfully.";
                }
                else
                {
                    TempData["FailMessage"] = "Failed to process payout.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payout.");
                TempData["FailMessage"] = "Error processing payout.";
            }

            return RedirectToAction("Index");
        }
    }
}
