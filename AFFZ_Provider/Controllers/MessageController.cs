using AFFZ_Provider.Models;
using AFFZ_Provider.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AFFZ_Provider.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        private readonly ILogger<MessageController> _logger;
        private static string _merchantIdCat = string.Empty;
        private readonly HttpClient _httpClient;
        IDataProtector _protector;
        public MessageController(IHttpClientFactory httpClientFactory, ILogger<MessageController> logger, IDataProtectionProvider provider)
        {
            _httpClient = httpClientFactory.CreateClient("Main");
            _protector = provider.CreateProtector("Example.SessionProtection");
            _logger = logger;
        }
        public async Task<IActionResult> Inbox()
        {
            int merchantId = Convert.ToInt32(HttpContext.Session.GetEncryptedString("ProviderId", _protector));
            _logger.LogInformation("Message.Inbox method called by Merchant: {merchantId}", merchantId);

            List<ChatterMessage> messages = new List<ChatterMessage>();

            try
            {
                var jsonResponse = await _httpClient.GetAsync($"Message/MerchantMessageList/{merchantId}");
                jsonResponse.EnsureSuccessStatusCode();
                if (jsonResponse != null)
                {
                    var responseString = await jsonResponse.Content.ReadAsStringAsync();
                    messages = JsonConvert.DeserializeObject<List<ChatterMessage>>(responseString);
                    ViewBag.SubCategoriesWithMerchant = messages;
                }
                else
                {
                    _logger.LogWarning("Received empty response from API");
                    ViewBag.SubCategoriesWithMerchant = new List<ChatterMessage>();
                }
            }
            catch (JsonSerializationException ex)
            {
                _logger.LogError(ex, "JSON deserialization error.");
                ViewBag.SubCategoriesWithMerchant = new List<ChatterMessage>();
                ModelState.AddModelError(string.Empty, "Failed to load categories.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred.");
                ViewBag.SubCategoriesWithMerchant = new List<ChatterMessage>();
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while loading categories.");
            }

            if (TempData.TryGetValue("SuccessMessage", out var saveResponse))
            {
                ViewBag.SaveResponse = saveResponse.ToString();
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> SendMessage(string SenderId)
        {
            ChatterMessage message = new ChatterMessage();

            //return View(message);


            int userId = Convert.ToInt32(HttpContext.Session.GetEncryptedString("ProviderId", _protector));
            ViewBag.CurrentUser = userId.ToString();
            List<ChatterMessage> messages = new List<ChatterMessage>();
            message.SenderId = Convert.ToInt32(HttpContext.Session.GetEncryptedString("ProviderId", _protector));
            message.ReceiverId = Convert.ToInt32(SenderId);
            var jsonResponse2 = await _httpClient.GetAsync($"Message/messages/{userId}/{(string.IsNullOrEmpty(SenderId) ? "0" : SenderId)}");
            jsonResponse2.EnsureSuccessStatusCode();
            if (jsonResponse2 != null)
            {
                var responseString = await jsonResponse2.Content.ReadAsStringAsync();
                messages = JsonConvert.DeserializeObject<List<ChatterMessage>>(responseString);
                ViewBag.ChatMessageList = messages;
            }
            else
            {
                _logger.LogWarning("Received empty response from API");
                ViewBag.ChatMessageList = new List<ChatterMessage>();
            }
            return View(message);

        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(ChatterMessage message)
        {
            message.SenderId = Convert.ToInt32(HttpContext.Session.GetEncryptedString("ProviderId", _protector));
            message.CreatedBy = Convert.ToInt32(HttpContext.Session.GetEncryptedString("ProviderId", _protector));
            message.CreatedDate = DateTime.Now;
            message.MessageType = "Chat";
            message.MerchantId = message.ReceiverId;
            _logger.LogInformation("Sending message From : {Sender}, To: {ReceiverId}", message.SenderId, message.ReceiverId);

            if (string.IsNullOrEmpty(message.SenderId.ToString()) || string.IsNullOrEmpty(message.ReceiverId.ToString()) || string.IsNullOrEmpty(message.MessageContent))
            {
                _logger.LogWarning("Invalid parameters");
                return BadRequest("Invalid parameters.");
            }

            try
            {

                var responseMessage = await _httpClient.PostAsync("Message/SendMessage", Customs.GetJsonContent(message));
                var responseString = await responseMessage.Content.ReadAsStringAsync();

                TempData["SuccessResponse"] = responseString;
                string SenderId = message.ReceiverId.ToString();
                return RedirectToAction("SendMessage", "Message", new { SenderId });//MessageBox.Show("Error in production file 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending the message.");
                return StatusCode(500, "An internal server error occurred. Please try again later.");
            }
        }
    }
}
