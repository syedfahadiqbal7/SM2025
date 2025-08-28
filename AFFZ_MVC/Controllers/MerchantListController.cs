using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AFFZ_Customer.Controllers
{
    public class MerchantListController : Controller
    {
        private readonly ILogger<MerchantListController> _logger;
        private static string _merchantIdCat = string.Empty;
        public MerchantListController(ILogger<MerchantListController> logger, WebApiHelper webApiHelper)
        {
            _logger = logger;
        }
        [HttpGet]
        public async Task<ActionResult> Index(string catName)
        {
            _logger.LogInformation("Index method called with CatName: {CatName}", catName);

            if (!string.IsNullOrEmpty(catName))
            {
                _merchantIdCat = catName;
            }

            List<CatWithMerchant> categories = new List<CatWithMerchant>();

            try
            {
                var jsonResponse = await WebApiHelper.GetData($"/api/CategoryWithMerchant/GetServiceListByMerchant?CatName={_merchantIdCat}");

                if (!string.IsNullOrEmpty(jsonResponse))
                {
                    categories = JsonConvert.DeserializeObject<List<CatWithMerchant>>(jsonResponse);
                    ViewBag.SubCategoriesWithMerchant = categories;
                }
                else
                {
                    _logger.LogWarning("Received empty response from API");
                    ViewBag.SubCategoriesWithMerchant = new List<CatWithMerchant>();
                }
            }
            catch (JsonSerializationException ex)
            {
                _logger.LogError(ex, "JSON deserialization error.");
                ViewBag.SubCategoriesWithMerchant = new List<CatWithMerchant>();
                ModelState.AddModelError(string.Empty, "Failed to load categories.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred.");
                ViewBag.SubCategoriesWithMerchant = new List<CatWithMerchant>();
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while loading categories.");
            }

            if (TempData.TryGetValue("SaveResponse", out var saveResponse))
            {
                ViewBag.SaveResponse = saveResponse.ToString();
            }

            return View();
        }
        public async Task<ActionResult> RequestForDiscount(string id)
        {
            _logger.LogInformation("RequestForDiscount method called with id: {Id}", id);
            string userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("RequestForDiscount called with empty id");
                return NotFound();
            }

            try
            {
                var reqIds = id.Split('~');
                var discountRequestClass = new DiscountRequestClass
                {
                    MerchantId = Convert.ToInt32(reqIds[0]),
                    ServiceId = Convert.ToInt32(reqIds[1]),
                    UserId = Convert.ToInt32(HttpContext.Session.GetString("UserId")) //HttpContext.Session.GetString("UserId")
                };

                var responseMessage = await WebApiHelper.PostData("/api/CategoryWithMerchant/sendRequestForDiscount", discountRequestClass);
                TempData["SaveResponse"] = responseMessage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending discount request.");
                TempData["SaveResponse"] = "An error occurred while sending the discount request.";
            }

            return RedirectToAction("Index");
        }
    }
    //public class CatWithMerchant
    //{
    //    public string? MID { get; set; }
    //    public string? SID { get; set; }
    //    public string? MERCHANTNAME { get; set; }
    //    public string? SERVICENAME { get; set; }
    //    public string? PRICE { get; set; }
    //    public string? MERCHANTLOCATION { get; set; }
    //}

    //public class DiscountRequestClass
    //{
    //    public int MerchantId { get; set; }
    //    public int ServiceId { get; set; }
    //    public int UserId { get; set; }
    //}
}
