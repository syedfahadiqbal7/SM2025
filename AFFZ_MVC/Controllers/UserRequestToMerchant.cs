using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AFFZ_Customer.Controllers
{
    public class UserRequestToMerchant : Controller
    {
        private ILogger<MerchantList> _logger;
        public UserRequestToMerchant(ILogger<MerchantList> logger, WebApiHelper webApiHelper)
        {
            _logger = logger;
        }
        public async Task<ActionResult> Index()
        {
            string _merchantId = HttpContext.Session.GetString("ProviderId");

            var jsonResponse = await WebApiHelper.GetData("/api/CategoryWithMerchant/AllRequestMerchant?Mid=" + _merchantId);

            if (!string.IsNullOrEmpty(jsonResponse))
            {
                try
                {
                    List<RequestForDiscountViewModel> categories = JsonConvert.DeserializeObject<List<RequestForDiscountViewModel>>(jsonResponse);
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
            if (TempData.ContainsKey("SaveResponse"))
            {
                if (!string.IsNullOrEmpty(TempData["SaveResponse"].ToString()))
                {
                    ViewBag.SaveResponse = TempData["SaveResponse"].ToString();
                }
            }

            return View();
        }
        [HttpPost]
        public async Task<ActionResult> ApplyDiscountedPrice(string RFDTM, string DiscountPrice, string MID, string UID, string SID)
        {
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
                    string responseMessage = await WebApiHelper.PostData("/api/CategoryWithMerchant/SaveMerchantResponseForDiscount", SRBM);

                    // Use TempData to pass the response message to the Index view
                    TempData["SaveResponse"] = responseMessage;

                    // Redirect to the Index action
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    // Log the exception details
                    // Use your preferred logging framework here. For example:
                    _logger.LogError(ex, "An error occurred while processing the discount request.");

                    return StatusCode(500, "An internal server error occurred. Please try again later.");
                }
            }
            else
            {
                return BadRequest();
            }
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
