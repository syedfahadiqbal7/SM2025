using AFFZ_Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
namespace AFFZ_Admin.Controllers
{
    public class CustomerTransferController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CustomerTransferController> _logger;
        public CustomerTransferController(IHttpClientFactory httpClientFactory, ILogger<CustomerTransferController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("Main");
            _logger = logger;
        }

        public async Task<IActionResult> TransferList()
        {
            List<CustomerTransferViewModel> CustomerServicePurchased = new List<CustomerTransferViewModel>();
            try
            {
                var response = await _httpClient.GetAsync($"CustomerTransfer/TransferList");
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    CustomerServicePurchased = JsonConvert.DeserializeObject<List<CustomerTransferViewModel>>(responseString);
                }
                else
                {
                    var responseFailString = await response.Content.ReadAsStringAsync();
                    TempData["FailMessage"] = "Failed to fetch Customer Transfer Data due to :" + responseFailString;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Customer Transfer");
                TempData["FailMessage"] = "An error occurred while fetching Customer Transfer.";
            }

            return View(CustomerServicePurchased);
        }

        public async Task<IActionResult> SalesTransactionList()
        {
            List<SalesTransactionViewModel> transactions = new List<SalesTransactionViewModel>();

            var response = await _httpClient.GetAsync($"CustomerTransfer/GetSalesTransactions");
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                transactions = JsonConvert.DeserializeObject<List<SalesTransactionViewModel>>(responseString);
            }
            else
            {
                var responseFailString = await response.Content.ReadAsStringAsync();
                TempData["FailMessage"] = "Failed to fetch sales transactions  Data due to :" + responseFailString;
            }

            return View(transactions);
        }
    }
    public class SalesTransactionViewModel
    {
        public int ID { get; set; }
        public string PaymentType { get; set; }
        public decimal Amount { get; set; }
        public string ProviderName { get; set; }
        public bool IsPaymentSuccess { get; set; }
        public int ServiceID { get; set; }
        public DateTime PaymentDateTime { get; set; }
        public int Quantity { get; set; }
        public string ServiceImage { get; set; }
        public string CustomerName { get; set; }
        public string ServiceName { get; set; }
        public string PaymentStatus { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
        public decimal ServiceCharge { get; set; }
    }
}