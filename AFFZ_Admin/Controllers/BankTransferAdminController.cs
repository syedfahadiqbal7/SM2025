using AFFZ_Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AFFZ_Admin.Controllers
{
    public class BankTransferAdminController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BankTransferAdminController> _logger;

        public BankTransferAdminController(IHttpClientFactory httpClientFactory, ILogger<BankTransferAdminController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("Main");
            _logger = logger;
        }

        // GET: List Bank Transfer Accounts
        public async Task<IActionResult> BankTransferIndex()
        {
            List<BankTransferAccount> accounts = new List<BankTransferAccount>();
            List<Merchant> merchant = new List<Merchant>();

            var response = await _httpClient.GetAsync("BankTransferAccount");
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                accounts = JsonConvert.DeserializeObject<List<BankTransferAccount>>(responseString);

                if (accounts.Count == 0)
                {
                    responseString = await response.Content.ReadAsStringAsync();
                    TempData["FailMessage"] = "No bank transfer accounts found." + responseString;
                }
            }
            else
            {
                TempData["FailMessage"] = "Failed to fetch bank transfer accounts.";
            }
            var merchantResponse = await _httpClient.GetAsync("BankTransferAccount/AllMerchants");
            if (merchantResponse.IsSuccessStatusCode)
            {
                var merchantResponseString = await merchantResponse.Content.ReadAsStringAsync();
                merchant = JsonConvert.DeserializeObject<List<Merchant>>(merchantResponseString);
                if (merchant.Count == 0)
                {
                    TempData["FailMessage"] = "No Provider accounts found.";
                }
            }
            else
            {
                var errorMsg = await merchantResponse.Content.ReadAsStringAsync();
                TempData["FailMessage"] = "Failed to fetch Provider accounts." + errorMsg;
            }
            ViewBag.Merchants = merchant;
            return View(accounts);
        }

        // POST: Add Bank Account
        [HttpPost]
        public async Task<IActionResult> AddBankAccount([FromBody] BankTransferAccount account)
        {

            var response = await _httpClient.PostAsJsonAsync("BankTransferAccount", account);
            var responseString = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true, message = "Bank Account added successfully." });
            }
            return Json(new { success = false, message = "Failed to add bank account. " + responseString });
        }

        // GET: Fetch Bank Account by Id
        [HttpGet]
        public async Task<IActionResult> GetBankAccount(int id)
        {
            var response = await _httpClient.GetAsync($"BankTransferAccount/{id}");
            var responseString = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var account = JsonConvert.DeserializeObject<BankTransferAccount>(await response.Content.ReadAsStringAsync());
                return Json(account);
            }
            return Json(null);
        }

        // POST: Edit Bank Account
        [HttpPost]
        public async Task<IActionResult> EditBankAccount([FromBody] BankTransferAccount account)
        {
            var response = await _httpClient.PostAsJsonAsync($"BankTransferAccount/{account.Id}", account);
            var responseString = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true, message = "Bank Account updated successfully." });
            }
            return Json(new { success = false, message = "Failed to update bank account. " + responseString });
        }

        // POST: Delete Bank Account
        [HttpPost]
        public async Task<IActionResult> DeleteBankAccount(int id)
        {
            var response = await _httpClient.GetAsync($"BankTransferAccount/DeleteAccount?id={id}");
            var responseString = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true, message = "Bank Account deleted successfully." });
            }
            return Json(new { success = false, message = "Failed to delete bank account. " + responseString });
        }
    }
}
