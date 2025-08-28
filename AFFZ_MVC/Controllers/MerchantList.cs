using AFFZ_Customer.Models;
using AFFZ_Customer.Utils;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AFFZ_Customer.Controllers
{
    public class MerchantList : Controller
    {
        private static string _merchantIdCat = string.Empty;
        private readonly ILogger<MerchantList> _logger;
        private readonly HttpClient _httpClient;
        private readonly IDataProtector _protector;
        private string _adminUrl = string.Empty;

        public MerchantList(IHttpClientFactory httpClientFactory, ILogger<MerchantList> logger, IDataProtectionProvider provider, IAppSettingsService _service)
        {
            _httpClient = httpClientFactory.CreateClient("Main");
            _protector = provider.CreateProtector("Example.SessionProtection");
            _logger = logger;
            _adminUrl = $"{_service.GetAdminHttpsPort()}";
        }
        [HttpGet]
        public async Task<ActionResult> MerchantIndex(string catName)
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
        [HttpGet]
        public async Task<IActionResult> SelectedMerchantList(string catName)
        {
            _logger.LogInformation("SelectedMerchantList called with Category Name: {CategoryName}", catName);
            int UserId = 0;
            if (string.IsNullOrEmpty(HttpContext.Session.GetEncryptedString("UserId", _protector)))
            {
                _logger.LogWarning("UserId is not set in session. Redirecting to login page.");
                string returnUrl = Url.Action("SelectedMerchantList", "MerchantList", new { catName }, Request.Scheme);
                return RedirectToAction("Index", "Login", new { returnUrl });
            }
            UserId = Convert.ToInt32(HttpContext.Session.GetEncryptedString("UserId", _protector));
            var categories = new List<CatWithMerchant>();
            try
            {
                // Fetch categories with merchants
                var response = await _httpClient.GetAsync($"CategoryWithMerchant/GetServiceListByMerchant?CatName={catName}&uid={UserId}");
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                categories = JsonConvert.DeserializeObject<List<CatWithMerchant>>(responseString) ?? new List<CatWithMerchant>();

                // Fetch cart items for the logged-in user
                string userId = HttpContext.Session.GetEncryptedString("UserId", _protector);
                if (!string.IsNullOrEmpty(userId))
                {
                    var cartResponse = await _httpClient.GetAsync($"Cart/GetCartItems/{userId}");
                    cartResponse.EnsureSuccessStatusCode();

                    var cartResponseString = await cartResponse.Content.ReadAsStringAsync();
                    var cartItems = JsonConvert.DeserializeObject<SResponse>(cartResponseString);

                    // Handle the Data property, which is a JArray
                    if (cartItems.Data is JArray cartServiceJArray)
                    {
                        // Extract the `serviceID` from each object in the array
                        var cartServiceList = cartServiceJArray
                            .Select(item => (int?)item["serviceID"]) // Use nullable int in case of missing values
                            .Where(id => id.HasValue) // Filter out null values
                            .Select(id => id.Value) // Convert to non-nullable int
                            .ToList();

                        // Mark services as already in cart if they exist in the cartServiceList
                        foreach (var service in categories)
                        {
                            if (cartServiceList.Contains(Convert.ToInt32(service.SID)))
                            {
                                service.IsAddedToCart = true;
                            }
                        }
                    }
                }

                ViewBag.SubCategoriesWithMerchant = categories;
                ViewBag.AdminUrl = _adminUrl;
                _logger.LogInformation("Successfully retrieved categories for Category Name: {CategoryName}", catName);
            }
            catch (JsonSerializationException ex)
            {
                _logger.LogError(ex, "Error deserializing JSON response for SelectedMerchantList.");
                ViewBag.SubCategoriesWithMerchant = categories;
                ModelState.AddModelError(string.Empty, "Failed to load categories.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in SelectedMerchantList.");
                ViewBag.SubCategoriesWithMerchant = categories;
                ModelState.AddModelError(string.Empty, "An error occurred while loading categories.");
            }

            if (TempData.TryGetValue("SuccessMessage", out var saveResponse))
            {
                ViewBag.SaveResponse = saveResponse.ToString();
            }

            return View();
        }
        public async Task<IActionResult> RequestForDiscount(string id, string catName)
        {
            _logger.LogInformation("RequestForDiscount initiated with id: {Id} For Category {CatName}", id, catName);
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("RequestForDiscount called with empty id");
                return NotFound();
            }

            try
            {
                var reqIds = id.Split('~');
                var discountRequest = new DiscountRequestClass
                {
                    MerchantId = Convert.ToInt32(reqIds[0]),
                    ServiceId = Convert.ToInt32(reqIds[1]),
                    UserId = Convert.ToInt32(HttpContext.Session.GetEncryptedString("UserId", _protector))
                };

                var discountResponse = await _httpClient.PostAsync("CategoryWithMerchant/sendRequestForDiscount", Customs.GetJsonContent(discountRequest));
                discountResponse.EnsureSuccessStatusCode();
                TempData["SuccessMessage"] = "Request for discount submitted successfully.";

                await UpdateTrackerStatus(reqIds, "User has asked for a discount.");
                await TriggerNotification(reqIds[0], reqIds[1], discountRequest.UserId.ToString(), Convert.ToInt32(reqIds[1]));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing RequestForDiscount.");
                TempData["FailMessage"] = "An error occurred while sending the discount request.";
            }

            return RedirectToAction("SelectedMerchantList", new { catName });
        }
        public async Task<IActionResult> RequestForDiscountFromCart(string id, string CartId)
        {
            _logger.LogInformation("RequestForDiscountFromCart initiated with id: {Id}", id);
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("RequestForDiscountFromCart called with empty id");
                return NotFound();
            }

            try
            {
                var reqIds = id.Split('~');
                var discountRequest = new DiscountRequestClass
                {
                    MerchantId = Convert.ToInt32(reqIds[0]),
                    ServiceId = Convert.ToInt32(reqIds[1]),
                    UserId = Convert.ToInt32(HttpContext.Session.GetEncryptedString("UserId", _protector))
                };

                var discountResponse = await _httpClient.PostAsync("CategoryWithMerchant/sendRequestForDiscount", Customs.GetJsonContent(discountRequest));
                discountResponse.EnsureSuccessStatusCode();
                TempData["SuccessMessage"] = "Request for discount submitted successfully.";

                var responseCartId = await _httpClient.DeleteAsync($"Cart/RemoveFromCart/{CartId}");
                responseCartId.EnsureSuccessStatusCode();

                await UpdateTrackerStatus(reqIds, "User has asked for a discount.");
                await TriggerNotification(reqIds[0], reqIds[1], discountRequest.UserId.ToString(), Convert.ToInt32(reqIds[1]));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing RequestForDiscountFromCart.");
                TempData["FailMessage"] = "An error occurred while sending the discount request.";
            }

            return RedirectToAction("SelectedMerchantList");
        }
        public async Task<ActionResult> ProceedDirecttoPaymentByCart(string id, int quantity, string payment, string CartId)
        {
            _logger.LogInformation("ProceedDirecttoPayment method called with id: {Id}", id);
            string userId = HttpContext.Session.GetEncryptedString("UserId", _protector);

            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("ProceedDirecttoPayment called with empty id");
                return NotFound();
            }

            RequestForDisCountToUserViewModel requestForDisCount = new RequestForDisCountToUserViewModel();
            var reqIds = id.Split('~');
            int merchantId = Convert.ToInt32(reqIds[0]);
            int serviceId = Convert.ToInt32(reqIds[1]);
            int userIdInt = Convert.ToInt32(userId);

            try
            {
                // Create discount request By Merchant and save in RequestForDiscountToMerchant
                var discountRequestClass = new DiscountRequestClass { MerchantId = merchantId, ServiceId = serviceId, UserId = userIdInt };
                var request = await _httpClient.PostAsync("CategoryWithMerchant/sendRequestForDiscount", Customs.GetJsonContent(discountRequestClass));
                request.EnsureSuccessStatusCode();
                await NotifyUser(reqIds, userIdInt, "#", $"User[{userIdInt}] has completed the payment and selected you as his final merchant. Wait for the files to get uploaded by the user  for the Service[{serviceId}].", "11", serviceId);
                // Retrieve discount details sent to merchant
                var jsonResponse = await _httpClient.GetAsync($"CategoryWithMerchant/AllRequestMerchant?Mid={reqIds[0]}");
                jsonResponse.EnsureSuccessStatusCode();

                //Add TrackingStatus 1,2
                await UpdateServiceStatusAsync(userIdInt, 1, 0);
                //Add changestatus
                var statusUpdate = new CurrentServiceStatusViewModel
                {
                    UId = userIdInt,
                    MId = merchantId,
                    RFDFU = 0,
                    CurrentStatus = "1"
                };

                // Send the request to the AFFZ_API
                var _changeStatusAdd = await _httpClient.PostAsJsonAsync("CategoryWithMerchant/UpdateServiceStatus", statusUpdate);
                await UpdateServiceStatusAsync(userIdInt, 2, 0);
                statusUpdate = new CurrentServiceStatusViewModel
                {
                    UId = userIdInt,
                    MId = merchantId,
                    RFDFU = 0,
                    CurrentStatus = "2"
                };

                // Send the request to the AFFZ_API
                _changeStatusAdd = await _httpClient.PostAsJsonAsync("CategoryWithMerchant/UpdateServiceStatus", statusUpdate);
                string responseString = await jsonResponse.Content.ReadAsStringAsync();
                List<RequestForDiscountViewModel> categories = JsonConvert.DeserializeObject<List<RequestForDiscountViewModel>>(responseString);

                var mdl = categories?.FirstOrDefault(x => x.MID == merchantId && x.SID == serviceId && x.UID == userIdInt);
                if (mdl == null)
                {
                    TempData["FailMessage"] = "Unable to retrieve discount data.";
                    return RedirectToAction("ErrorPage");
                }

                // Save Merchant Response In Request For Discount To User and update RequestForDiscountToMerchant IsResponseSent
                var submitResponse = new SubmitResponseByMerchant
                {
                    RFDTM = mdl.RFDTM.ToString(),
                    DiscountPrice = mdl.ServicePrice.ToString(),
                    UID = userIdInt.ToString(),
                    SID = serviceId.ToString(),
                    MID = merchantId.ToString()
                };

                var responseMessage = await _httpClient.PostAsync("CategoryWithMerchant/SaveMerchantResponseForDiscount", Customs.GetJsonContent(submitResponse));
                responseMessage.EnsureSuccessStatusCode();

                int crfdfu = Convert.ToInt32((await responseMessage.Content.ReadAsStringAsync()).Split('-')[1]);

                // Update Tracking Information
                await UpdateServiceStatusAsync(userIdInt, 3, crfdfu);
                await UpdateServiceStatusAsync(userIdInt, 4, crfdfu);

                // Retrieve merchant responses for validation
                var merchantResponses = await _httpClient.GetAsync($"CategoryWithMerchant/AllResponsesFromMerchant?Uid={userId}");
                merchantResponses.EnsureSuccessStatusCode();

                string responseString3 = await merchantResponses.Content.ReadAsStringAsync();
                List<RequestForDisCountToUserViewModel> merchantDiscountResponses = JsonConvert.DeserializeObject<List<RequestForDisCountToUserViewModel>>(responseString3);

                requestForDisCount = merchantDiscountResponses?
                    .FirstOrDefault(x => x.MerchantID == merchantId && x.UID == userIdInt && x.SID == serviceId && x.IsMerchantSelected == 0 && x.IsPaymentDone == 0);

                if (requestForDisCount == null)
                {
                    _logger.LogWarning("No pending discount responses found for userId: {UserId}", userId);
                    TempData["FailMessage"] = "Discount selection was unsuccessful. Please try again.";
                    return RedirectToAction("ErrorPage");
                }

                // Select final merchant
                var selectFinalMerchant = new RequestForDisCountToUser
                {
                    RFDFU = requestForDisCount.RFDFU,
                    UID = userIdInt,
                    MID = requestForDisCount.MerchantID,
                    IsMerchantSelected = 1,
                    IsPaymentDone = 0
                };

                var finalMerchantResponse = await _httpClient.PostAsync("CategoryWithMerchant/SelectFinalMerchant", Customs.GetJsonContent(selectFinalMerchant));
                finalMerchantResponse.EnsureSuccessStatusCode();

                var responseCartId = await _httpClient.DeleteAsync($"Cart/RemoveFromCart/{CartId}");
                responseCartId.EnsureSuccessStatusCode();


                await UpdateServiceStatusAsync(userIdInt, 5, requestForDisCount.RFDFU);
                await UpdateServiceStatusAsync(userIdInt, 6, requestForDisCount.RFDFU);
                await UpdateServiceStatusAsync(userIdInt, 9, requestForDisCount.RFDFU);

                await NotifyUser(reqIds, userIdInt, "#", $"User[{userIdInt}] has completed the payment and selected you as his final merchant. Wait for the files to get uploaded by the user for the Service[{serviceId}].", "11", serviceId);

                TempData["SuccessMessage"] = "Discount processed successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending discount request.");
                TempData["FailMessage"] = "An error occurred while sending the discount request.";
            }

            return RedirectToAction("PaymentCart", "MerchantResponseToUser", new { rfdfu = requestForDisCount.RFDFU, uid = userIdInt, merchantId = merchantId, Quantity = quantity, Payment = payment });
        }
        public async Task<ActionResult> ProceedDirecttoPayment(string id)
        {
            _logger.LogInformation("ProceedDirecttoPayment method called with id: {Id}", id);
            string userId = HttpContext.Session.GetEncryptedString("UserId", _protector);

            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("ProceedDirecttoPayment called with empty id");
                return NotFound();
            }

            RequestForDisCountToUserViewModel requestForDisCount = new RequestForDisCountToUserViewModel();
            var reqIds = id.Split('~');
            int merchantId = Convert.ToInt32(reqIds[0]);
            int serviceId = Convert.ToInt32(reqIds[1]);
            int userIdInt = Convert.ToInt32(userId);
            int? ServPrice = 0;
            try
            {
                // Create discount request By Merchant and save in RequestForDiscountToMerchant
                var discountRequestClass = new DiscountRequestClass { MerchantId = merchantId, ServiceId = serviceId, UserId = userIdInt };
                var request = await _httpClient.PostAsync("CategoryWithMerchant/sendRequestForDiscount", Customs.GetJsonContent(discountRequestClass));
                request.EnsureSuccessStatusCode();
                await NotifyUser(reqIds, userIdInt, "#", $"User[{userIdInt}] has completed the payment and selected you as his final merchant. Wait for the files to get uploaded by the user for the Service[{serviceId}].", "11", serviceId);
                // Retrieve discount details sent to merchant
                var jsonResponse = await _httpClient.GetAsync($"CategoryWithMerchant/AllRequestMerchant?Mid={reqIds[0]}");
                jsonResponse.EnsureSuccessStatusCode();

                //Add TrackingStatus 1,2
                await UpdateServiceStatusAsync(userIdInt, 1, 0);
                //Add changestatus
                var statusUpdate = new CurrentServiceStatusViewModel
                {
                    UId = userIdInt,
                    MId = merchantId,
                    RFDFU = 0,
                    CurrentStatus = "1"
                };

                // Send the request to the AFFZ_API
                var _changeStatusAdd = await _httpClient.PostAsJsonAsync("CategoryWithMerchant/UpdateServiceStatus", statusUpdate);
                await UpdateServiceStatusAsync(userIdInt, 2, 0);
                statusUpdate = new CurrentServiceStatusViewModel
                {
                    UId = userIdInt,
                    MId = merchantId,
                    RFDFU = 0,
                    CurrentStatus = "2"
                };

                // Send the request to the AFFZ_API
                _changeStatusAdd = await _httpClient.PostAsJsonAsync("CategoryWithMerchant/UpdateServiceStatus", statusUpdate);
                string responseString = await jsonResponse.Content.ReadAsStringAsync();
                List<RequestForDiscountViewModel> categories = JsonConvert.DeserializeObject<List<RequestForDiscountViewModel>>(responseString);

                var mdl = categories?.FirstOrDefault(x => x.MID == merchantId && x.SID == serviceId && x.UID == userIdInt);
                if (mdl == null)
                {
                    TempData["FailMessage"] = "Unable to retrieve discount data.";
                    return RedirectToAction("ErrorPage");
                }

                // Save Merchant Response In Request For Discount To User and update RequestForDiscountToMerchant IsResponseSent
                var submitResponse = new SubmitResponseByMerchant
                {
                    RFDTM = mdl.RFDTM.ToString(),
                    DiscountPrice = "0",
                    UID = userIdInt.ToString(),
                    SID = serviceId.ToString(),
                    MID = merchantId.ToString()
                };
                ServPrice = mdl.ServicePrice;
                var responseMessage = await _httpClient.PostAsync("CategoryWithMerchant/SaveMerchantResponseForDiscount", Customs.GetJsonContent(submitResponse));
                responseMessage.EnsureSuccessStatusCode();

                int crfdfu = Convert.ToInt32((await responseMessage.Content.ReadAsStringAsync()).Split('-')[1]);

                // Update Tracking Information
                await UpdateServiceStatusAsync(userIdInt, 3, crfdfu);
                await UpdateServiceStatusAsync(userIdInt, 4, crfdfu);

                // Retrieve merchant responses for validation
                var merchantResponses = await _httpClient.GetAsync($"CategoryWithMerchant/AllResponsesFromMerchant?Uid={userId}");
                merchantResponses.EnsureSuccessStatusCode();

                string responseString3 = await merchantResponses.Content.ReadAsStringAsync();
                List<RequestForDisCountToUserViewModel> merchantDiscountResponses = JsonConvert.DeserializeObject<List<RequestForDisCountToUserViewModel>>(responseString3);

                requestForDisCount = merchantDiscountResponses?
                    .FirstOrDefault(x => x.MerchantID == merchantId && x.UID == userIdInt && x.SID == serviceId && x.IsMerchantSelected == 0 && x.IsPaymentDone == 0);

                if (requestForDisCount == null)
                {
                    _logger.LogWarning("No pending discount responses found for userId: {UserId}", userId);
                    TempData["FailMessage"] = "Discount selection was unsuccessful. Please try again.";
                    return RedirectToAction("ErrorPage");
                }

                // Select final merchant
                var selectFinalMerchant = new RequestForDisCountToUser
                {
                    RFDFU = requestForDisCount.RFDFU,
                    UID = userIdInt,
                    MID = requestForDisCount.MerchantID,
                    IsMerchantSelected = 1,
                    IsPaymentDone = 0
                };

                var finalMerchantResponse = await _httpClient.PostAsync("CategoryWithMerchant/SelectFinalMerchant", Customs.GetJsonContent(selectFinalMerchant));
                finalMerchantResponse.EnsureSuccessStatusCode();

                await UpdateServiceStatusAsync(userIdInt, 5, requestForDisCount.RFDFU);
                await UpdateServiceStatusAsync(userIdInt, 6, requestForDisCount.RFDFU);
                await UpdateServiceStatusAsync(userIdInt, 9, requestForDisCount.RFDFU);

                await NotifyUser(reqIds, userIdInt, "#", $"User[{userIdInt}] has Selected to Proceed wtih direct payment and selected you as his final merchant for the Service[{serviceId}].", "11", serviceId);

                TempData["SuccessMessage"] = "Discount processed successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending discount request.");
                TempData["FailMessage"] = "An error occurred while sending the discount request.";
            }

            return RedirectToAction("Payment", "MerchantResponseToUser", new { rfdfu = requestForDisCount.RFDFU, uid = userIdInt, merchantId = merchantId, ServicePrice = ServPrice.ToString() });
        }
        // Helper Method to Update Service Status
        private async Task UpdateServiceStatusAsync(int userId, int statusId, int rfdfu)
        {
            var trackerUpdate = new TrackServiceStatusHistory
            {
                ChangedByID = userId,
                StatusID = statusId,
                RFDFU = rfdfu,
                ChangedByUserType = "User",
                ChangedOn = DateTime.Now,
                Comments = $"User[{userId}] Direct Paying mode."
            };
            await TrackingDirectPayment(trackerUpdate);
        }
        // Helper Method to Send Notification
        private async Task NotifyUser(string[] reqIds, int userId, string url, string message, string StatusId, int ServiceId)
        {
            var notification = new Notification
            {
                UserId = HttpContext.Session.GetEncryptedString("UserId", _protector),
                Message = message,
                MerchantId = reqIds[0],
                RedirectToActionUrl = url,
                MessageFromId = userId,
                SID = ServiceId
            };

            var response = await _httpClient.PostAsync("Notifications/CreateNotification?StatusId=" + StatusId, Customs.GetJsonContent(notification));
            _logger.LogInformation("Notification response: {Response}", await response.Content.ReadAsStringAsync());
        }
        private async Task UpdateTrackerStatus(string[] reqIds, string comment)
        {
            int userId = Convert.ToInt32(HttpContext.Session.GetEncryptedString("UserId", _protector));
            var trackerUpdate = new TrackServiceStatusHistory
            {
                ChangedByID = userId,
                StatusID = 1,
                RFDFU = 0,
                ChangedByUserType = "User",
                ChangedOn = DateTime.Now,
                Comments = comment
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("TrackServiceStatusHistory/CreateStatus", trackerUpdate);
                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Tracker status updated successfully for request: {Request}", reqIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tracker status.");
                throw;
            }
        }
        private async Task TriggerNotification(string merchantId, string serviceId, string userId, int ServiceId)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = $"User[{userId}] has asked for a discount For Service[{serviceId}]",
                MerchantId = merchantId,
                RedirectToActionUrl = "/UserRequestToMerchant/CheckReqest",
                MessageFromId = Convert.ToInt32(userId),
                SID = ServiceId
            };

            try
            {
                var response = await _httpClient.PostAsync("Notifications/CreateNotification?StatusId=1", Customs.GetJsonContent(notification));
                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Notification triggered successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering notification.");
                throw;
            }
        }
        private async Task TrackingDirectPayment(TrackServiceStatusHistory TrackerUpdate)
        {
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
        }
        [HttpPost]
        public async Task<IActionResult> AddToCart(int merchantId, int serviceId)
        {
            _logger.LogInformation("AddToCart initiated for MerchantId: {MerchantId}, ServiceId: {ServiceId}", merchantId, serviceId);
            try
            {
                string userId = HttpContext.Session.GetEncryptedString("UserId", _protector);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User is not logged in. Redirecting to login page.");
                    return Json(new { success = false, message = "User is not logged in" });
                }

                var cartRequest = new
                {
                    CustomerID = Convert.ToInt32(userId),
                    ServiceID = serviceId,
                    Quantity = 1 // Default quantity is 1 for each service
                };

                var response = await _httpClient.PostAsync("Cart/AddToCart", Customs.GetJsonContent(cartRequest));
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Service successfully added to cart for MerchantId: {MerchantId}, ServiceId: {ServiceId}", merchantId, serviceId);
                return Json(new { success = true, message = "Service added to cart successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding service to cart.");
                return Json(new { success = false, message = "An error occurred while adding the service to the cart." });
            }
        }
    }

    public class CartRequest
    {
        public int CustomerID { get; set; }
        public int ServiceID { get; set; }
        public int Quantity { get; set; }
    }
    public class CatWithMerchant
    {
        public string? MID { get; set; }
        public string? SID { get; set; }
        public string? MERCHANTNAME { get; set; }
        public string? SERVICENAME { get; set; }
        public string? ServiceImage { get; set; }
        public string? PRICE { get; set; }
        public string? MERCHANTLOCATION { get; set; }
        public bool IsRequestedAlready { get; set; }
        public bool IsAddedToCart { get; set; } // New property added
        public int SERVICERATING { get; set; }
        public int RequiresEligibility { get; set; }
        public bool IsRequestedEligibility { get; set; }
        public bool IsEligibilityApproved { get; set; }
        public bool IsWaitingEligibilityResponseFromUser { get; set; }
    }

    public class DiscountRequestClass
    {
        public int MerchantId { get; set; }
        public int ServiceId { get; set; }
        public int UserId { get; set; }
    }
    //public class SubmitResponseByMerchant
    //{
    //    public string RFDTM { get; set; }
    //    public string DiscountPrice { get; set; }
    //    public string MID { get; set; }
    //    public string UID { get; set; }
    //    public string SID { get; set; }
    //}
}
