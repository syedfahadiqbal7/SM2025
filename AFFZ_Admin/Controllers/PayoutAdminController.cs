using AFFZ_Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace AFFZ_Admin.Controllers
{
    public class PayoutAdminController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PayoutAdminController> _logger;

        public PayoutAdminController(IHttpClientFactory httpClientFactory, ILogger<PayoutAdminController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("Main");
            _logger = logger;
        }

        // GET: Payout Settings
        public async Task<IActionResult> PayoutSettings()
        {
            var response = await _httpClient.GetAsync("Payout/GetPayoutSettings");
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var settings = JsonConvert.DeserializeObject<PayoutSettings>(responseString);
                return View(settings);
            }

            TempData["FailMessage"] = "Failed to load Payout Settings.";
            return View(new PayoutSettings());
        }

        // POST: Update Payout Settings
        [HttpPost]
        public async Task<IActionResult> UpdatePayoutSettings(PayoutSettings settings)
        {
            var content = new StringContent(JsonConvert.SerializeObject(settings), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("Payout/UpdatePayoutSettings", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Payout settings updated successfully.";
            }
            else
            {
                TempData["FailMessage"] = "Failed to update payout settings.";
            }

            return RedirectToAction("PayoutSettings");
        }

        // GET: List of Payout Transactions
        public async Task<IActionResult> PayoutTransactions()
        {
            var response = await _httpClient.GetAsync("Payout/GetPayoutTransactions");
            if (response.IsSuccessStatusCode)
            {
                var transactions = JsonConvert.DeserializeObject<List<PayoutTransactionViewModel>>(await response.Content.ReadAsStringAsync());
                return View(transactions);
            }

            TempData["FailMessage"] = "Failed to fetch payout transactions.";
            return View(new List<PayoutTransactionViewModel>());
        }

        // POST: Create Payout Transaction
        [HttpPost]
        public async Task<IActionResult> CreatePayoutTransaction(PayoutTransaction transaction)
        {
            var content = new StringContent(JsonConvert.SerializeObject(transaction), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("Payout/CreatePayoutTransaction", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Payout transaction created successfully.";
            }
            else
            {
                TempData["FailMessage"] = "Failed to create payout transaction.";
            }

            return RedirectToAction("PayoutTransactions");
        }

        // POST: Update Payout Transaction Status
        [HttpPost]
        public async Task<IActionResult> UpdatePayoutStatus(int id, string status)
        {
            var response = await _httpClient.PostAsync($"Payout/UpdatePayoutStatus?id={id}&status={status}", null);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Payout status updated successfully.";
            }
            else
            {
                TempData["FailMessage"] = "Failed to update payout status.";
            }

            return RedirectToAction("PayoutTransactions");
        }
    }
}
