using AFFZ_Provider.Models;
using AFFZ_Provider.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
namespace AFFZ_Provider.Controllers
{
    [Authorize]
    public class UserRequestToMerchant : Controller
    {
        private ILogger<UserRequestToMerchant> _logger;
        private readonly HttpClient _httpClient;
        IDataProtector _protector;
        public UserRequestToMerchant(ILogger<UserRequestToMerchant> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration, IDataProtectionProvider provider)
        {
            _httpClient = httpClientFactory.CreateClient("Main");
            _protector = provider.CreateProtector("Example.SessionProtection");
            _logger = logger;
        }
        public async Task<ActionResult> CheckReqest()
        {
            string _merchantId = HttpContext.Session.GetEncryptedString("ProviderId", _protector);
            if (string.IsNullOrEmpty(_merchantId))
            {
                return RedirectToAction("Index", "Login");
            }
            var jsonResponse = await _httpClient.GetAsync("CategoryWithMerchant/AllRequestMerchant?Mid=" + _merchantId);
            jsonResponse.EnsureSuccessStatusCode();


            var responseString = await jsonResponse.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(responseString))
            {
                try
                {
                    List<RequestForDiscountViewModel> categories = JsonConvert.DeserializeObject<List<RequestForDiscountViewModel>>(responseString);
                    ViewBag.RequestForDisCountToMerchant = categories;
                }
                catch (JsonSerializationException ex)
                {
                    // Log the exception details
                    _logger.LogError(ex, "JSON deserialization error.");

                    // Handle the error response accordingly
                    ViewBag.RequestForDisCountToMerchant = new List<RequestForDiscountViewModel>();
                    ModelState.AddModelError(string.Empty, "Failed to load Data.");
                }
            }
            else
            {
                ViewBag.Categories = new List<RequestForDiscountViewModel>();
            }

            // Check TempData for the response message
            if (TempData.ContainsKey("SuccessMessage"))
            {
                if (!string.IsNullOrEmpty(TempData["SuccessMessage"].ToString()))
                {
                    ViewBag.SaveResponse = TempData["SuccessMessage"].ToString();
                }
            }

            return View();
        }
        [HttpPost]
        public async Task<ActionResult> ApplyDiscountedPrice(string RFDTM, string DiscountPrice, string MID, string UID, string SID)
        {
            string _merchantId = HttpContext.Session.GetEncryptedString("ProviderId", _protector);

            if (!string.IsNullOrEmpty(RFDTM) && !string.IsNullOrEmpty(DiscountPrice))
            {
                SubmitResponseByMerchant SRBM = new SubmitResponseByMerchant();
                SRBM.RFDTM = RFDTM;

                SRBM.DiscountPrice = DiscountPrice;
                SRBM.UID = UID;
                SRBM.SID = SID;
                SRBM.MID = MID;

                try
                {
                    var responseMessage = await _httpClient.PostAsync("CategoryWithMerchant/SaveMerchantResponseForDiscount", Customs.GetJsonContent(SRBM));
                    responseMessage.EnsureSuccessStatusCode();
                    var responseString = await responseMessage.Content.ReadAsStringAsync();
                    // Use TempData to pass the response message to the Index view
                    TempData["SuccessMessage"] = responseString;
                    //Tracker

                    var TrackerUpdate = new TrackServiceStatusHistory
                    {
                        ChangedByID = Convert.ToInt32(MID),
                        StatusID = 3,
                        RFDFU = Convert.ToInt32(responseString.Split('-')[1]),
                        ChangedByUserType = "Merchant",
                        ChangedOn = DateTime.Now,
                        Comments = $"Merchant [{UID.ToString()}] has sent a discount response for the visa type [{SID}] you requested."
                    };

                    // Send the request to the AFFZ_API
                    var TrackerUpdateResponse = await _httpClient.PostAsJsonAsync("TrackServiceStatusHistory/CreateStatus", TrackerUpdate);

                    if (TrackerUpdateResponse.IsSuccessStatusCode)
                    {

                        //"Your service process has started. You will be notified once updated by the merchant.";//Notifiaction
                        TempData["SuccessMessage"] = "Service and Tracker process Status Updated Successfully.";
                    }
                    else
                    {
                        TempData["FailMessage"] = "Service updated but Failed to update the Tracker process.";
                    }

                    TrackerUpdate = new TrackServiceStatusHistory
                    {
                        ChangedByID = Convert.ToInt32(MID),
                        StatusID = 4,
                        RFDFU = Convert.ToInt32(responseString.Split('-')[1]),
                        ChangedByUserType = "Merchant",
                        ChangedOn = DateTime.Now,
                        Comments = $"Merchant [{UID.ToString()}] has sent a discount response for the visa type [{SID}] requested and waiting for selection."
                    };

                    // Send the request to the AFFZ_API
                    TrackerUpdateResponse = await _httpClient.PostAsJsonAsync("TrackServiceStatusHistory/CreateStatus", TrackerUpdate);

                    if (TrackerUpdateResponse.IsSuccessStatusCode)
                    {

                        //"Your service process has started. You will be notified once updated by the merchant.";//Notifiaction
                        TempData["SuccessMessage"] = "Service and Tracker process Status Updated Successfully.";
                    }
                    else
                    {
                        TempData["FailMessage"] = "Service updated but Failed to update the Tracker process.";
                    }




                    // Trigger notification
                    var notification = new Notification
                    {
                        UserId = UID.ToString(),
                        Message = $"Merchant[{_merchantId}] has sent a discount for the Service[{SID}] you requested.",
                        MerchantId = _merchantId,
                        RedirectToActionUrl = "/MerchantResponseToUser/MerchantResponseIndex",
                        MessageFromId = Convert.ToInt32(_merchantId),
                        SenderType = "Merchant",
                        SID = Convert.ToInt32(SID)
                    };

                    var res = await _httpClient.PostAsync("Notifications/CreateNotification?StatusId=3", Customs.GetJsonContent(notification));
                    string resString = await res.Content.ReadAsStringAsync();
                    _logger.LogInformation("Notification Response : " + resString);
                }
                catch (Exception ex)
                {
                    // Log the exception details
                    // Use your preferred logging framework here. For example:
                    _logger.LogError(ex, "An error occurred while processing the discount request.");
                    TempData["FailMessage"] = ex.Message;
                }
            }
            return RedirectToAction("CheckReqest");
        }
    }
    public class RequestForDiscountViewModel
    {
        public int RFDTM { get; set; }
        public int SID { get; set; }
        public int UID { get; set; }
        public int MID { get; set; }
        public string ServiceName { get; set; }
        public int? ServicePrice { get; set; }
        public DateTime RequestDatetime { get; set; }
    }
    public class SubmitResponseByMerchant
    {
        public string RFDTM { get; set; }
        public string DiscountPrice { get; set; }
        public string MID { get; set; }
        public string UID { get; set; }
        public string SID { get; set; }
    }
}
