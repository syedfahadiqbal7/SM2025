using AFFZ_Customer.Models;
using AFFZ_Customer.Utils;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using Stripe;
using Stripe.Checkout;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace AFFZ_Customer.Controllers
{
    public class MerchantResponseToUser : Controller
    {
        private readonly ILogger<MerchantResponseToUser> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly HttpClient _httpClient;
        private readonly IDataProtector _protector;
        private string BaseUrlIp = string.Empty;
        private string PublicDomain = string.Empty;
        private string ApiHttpsPort = string.Empty;
        private string CustomerHttpsPort = string.Empty;
        public MerchantResponseToUser(ILogger<MerchantResponseToUser> logger, IWebHostEnvironment environment, IHttpClientFactory httpClientFactory, IDataProtectionProvider provider, IOptions<AppSettings> options, IAppSettingsService service)
        {
            // Initialize HTTP client, data protector, logger, and environment
            _httpClient = httpClientFactory.CreateClient("Main");
            _protector = provider.CreateProtector("Example.SessionProtection");
            _logger = logger;
            _environment = environment;
            BaseUrlIp = options.Value.BaseIpAddress;
            PublicDomain = service.GetPublicDomain();
            ApiHttpsPort = service.GetApiHttpsPort();
            CustomerHttpsPort = service.GetCustomerHttpsPort();
            _logger.LogInformation("MerchantResponseToUser controller initialized"); // Log initialization
        }

        [HttpGet]
        public async Task<ActionResult> MerchantResponseIndex()
        {
            _logger.LogInformation("MerchantResponseIndex called"); // Log method entry

            // Validate user ID from session
            string userId = HttpContext.Session.GetEncryptedString("UserId", _protector);
            _logger.LogDebug("Retrieved userId from session: {UserId}", userId); // Log retrieved user ID
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in session. Redirecting to login."); // Log missing user ID
                return RedirectToAction("Login", "Account"); // Redirect to login if user ID is missing
            }

            try
            {
                // Fetch responses from the API
                _logger.LogInformation("Fetching responses from API for userId: {UserId}", userId); // Log API request
                var jsonResponse = await _httpClient.GetAsync($"CategoryWithMerchant/AllResponsesFromMerchant?Uid={userId}");
                _logger.LogDebug("API response status code: {StatusCode}", jsonResponse.StatusCode); // Log response status code
                if (!jsonResponse.IsSuccessStatusCode)
                {
                    // Log error if the API request fails
                    _logger.LogError("Failed to fetch responses for userId: {UserId}. Status code: {StatusCode}", userId, jsonResponse.StatusCode);
                    ModelState.AddModelError(string.Empty, "Failed to load responses from the server."); // Add model error for API failure
                    ViewBag.ResponseForDisCountFromMerchant = new List<RequestForDisCountToUserViewModel>(); // Return empty list in case of error
                    return View();
                }

                // Parse the response
                var responseString = await jsonResponse.Content.ReadAsStringAsync();
                _logger.LogDebug("API response content: {ResponseContent}", responseString); // Log response content
                if (string.IsNullOrEmpty(responseString))
                {
                    // Log warning if response is empty
                    _logger.LogWarning("Empty response received from API for userId: {UserId}", userId);
                    ViewBag.ResponseForDisCountFromMerchant = new List<RequestForDisCountToUserViewModel>(); // Return empty list if response is empty
                }
                else
                {
                    // Deserialize response into a list of view models
                    List<RequestForDisCountToUserViewModel> categories = JsonConvert.DeserializeObject<List<RequestForDisCountToUserViewModel>>(responseString);
                    _logger.LogInformation("Successfully deserialized API response for userId: {UserId}", userId); // Log successful deserialization
                    ViewBag.ResponseForDisCountFromMerchant = categories; // Assign deserialized list to ViewBag
                    //Get file list of all the completed service....
                    foreach (var item in categories)
                    {
                        if (item.IsRequestCompleted)
                        {
                            int _uid = Convert.ToInt32(userId);
                            var jsonFileResponse = await _httpClient.GetAsync($"FileUpload/GetListOfDocuments/{userId}/{item.RFDFU}");
                            string responseFileString = await jsonFileResponse.Content.ReadAsStringAsync();

                            if (!string.IsNullOrEmpty(responseFileString))
                            {
                                // Deserialize the response into a list of UploadedFile objects
                                List<UploadedFile> documentList = JsonConvert.DeserializeObject<List<UploadedFile>>(responseFileString);
                                var username = $"Documents_{userId}";

                                // Construct the folder path
                                var folderPath = Path.Combine(_environment.WebRootPath, "uploads", username);
                                if (documentList.Count > 0)
                                {
                                    // Check if the folder exists
                                    if (Directory.Exists(folderPath))
                                    {
                                        foreach (var itemDocs in documentList)
                                        {
                                            // Construct the full file path
                                            var filePath = Path.Combine(folderPath, itemDocs.FileName);

                                            // Check if the file exists before attempting to delete it
                                            if (System.IO.File.Exists(filePath))
                                            {
                                                //System.IO.File.Delete(filePath);
                                                _logger.LogInformation($"Deleted file: {filePath}");
                                                //DeleteUploadedFilesByStatus
                                                //_logger.LogDebug("Saving payment history via API.");
                                                //var responseMessage = await _httpClient.PostAsync($"FileUpload/DeleteUploadedFilesByStatus?rfdfu="+itemDocs.RFDFU, Customs.GetJsonContent(null));
                                                //if (!responseMessage.IsSuccessStatusCode)
                                                //{
                                                //    _logger.LogWarning("Failed to save payment history.");
                                                //    return StatusCode(500, "Failed to save payment history.");
                                                //}
                                            }
                                            else
                                            {
                                                _logger.LogInformation($"File not found: {filePath}");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        _logger.LogInformation($"Folder not found: {folderPath}");
                                    }
                                }
                            }
                            else
                            {
                                _logger.LogInformation("No files to process. The response is empty.");
                            }

                        }
                    }
                }
            }
            catch (JsonSerializationException ex)
            {
                // Log JSON deserialization exception and handle model error
                _logger.LogError(ex, "JSON deserialization error for userId: {UserId}", userId);
                ModelState.AddModelError(string.Empty, "Failed to load data due to a server error."); // Add model error for deserialization issue
                ViewBag.ResponseForDisCountFromMerchant = new List<RequestForDisCountToUserViewModel>(); // Return empty list in case of exception
            }
            catch (HttpRequestException ex)
            {
                // Log HTTP request exception and handle model error
                _logger.LogError(ex, "HTTP request error occurred while fetching data for userId: {UserId}", userId);
                ModelState.AddModelError(string.Empty, "Error communicating with the server. Please try again later."); // Add model error for communication issues
                ViewBag.ResponseForDisCountFromMerchant = new List<RequestForDisCountToUserViewModel>(); // Return empty list in case of exception
            }
            catch (Exception ex)
            {
                // Log any other unexpected exceptions and handle model error
                _logger.LogError(ex, "An unexpected error occurred while fetching data for userId: {UserId}", userId);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while loading data."); // Add generic error message to model state
                ViewBag.ResponseForDisCountFromMerchant = new List<RequestForDisCountToUserViewModel>(); // Return empty list in case of exception
            }

            // Display any saved response messages from TempData
            if (TempData.TryGetValue("SaveResponse", out var saveResponse))
            {
                _logger.LogDebug("Retrieved SaveResponse from TempData: {SaveResponse}", saveResponse); // Log retrieved TempData message
                ViewBag.SaveResponse = saveResponse.ToString();
            }
            var verificationLink = $"{ApiHttpsPort}";
            ViewBag.APIUrl = verificationLink;
            return View();
        }
        [HttpGet]
        public IActionResult DownloadFile(string fileName, string folderName)
        {
            // Retrieve userId from session using data protector
            string userId = HttpContext.Session.GetEncryptedString("UserId", _protector);
            _logger.LogDebug("DownloadFile called with userId: {UserId}", userId);
            try
            {
                _logger.LogInformation("DownloadFile called with fileName: {FileName}, folderName: {FolderName}", fileName, folderName);

                // Validate input parameters
                if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(folderName))
                {
                    _logger.LogWarning("Invalid parameters passed to DownloadFile");
                    return BadRequest("Invalid parameters.");
                }

                // Construct the file path based on the environment's web root path
                var filePath = Path.Combine(_environment.WebRootPath.Replace("AFFZ_MVC", "AFFZ_Provider"), "uploads", fileName);
                _logger.LogDebug("Constructed file path: {FilePath}", filePath);

                if (System.IO.File.Exists(filePath))
                {
                    // Read the file bytes and return as a download response
                    _logger.LogDebug("File found at path: {FilePath}", filePath);
                    var fileBytes = System.IO.File.ReadAllBytes(filePath);
                    return File(fileBytes, "application/octet-stream", fileName);
                }
                else
                {
                    _logger.LogWarning("File not found: {FilePath}", filePath);
                    return NotFound("File not found.");
                }
            }
            catch (Exception ex)
            {
                // Log the error and return a 500 status code
                _logger.LogError(ex, "An unexpected error occurred while fetching data for userId: {UserId}", userId);
                return StatusCode(500, "An unexpected error occurred while loading data.");
            }
        }

        [HttpPost]
        public async Task<ActionResult> PaymentDone([FromForm] string amount, [FromForm] string payerId, [FromForm] string merchantId, [FromForm] string serviceId, [FromForm] string rfdfu, [FromForm] string NoOfQuantity, [FromForm] string ServicePrice)
        {
            // Retrieve userId from session using data protector
            string userId = HttpContext.Session.GetEncryptedString("UserId", _protector);
            _logger.LogDebug("PaymentDone called with userId: {UserId}", userId);
            try
            {
                _logger.LogInformation("PaymentDone called with amount: {Amount}, payerId: {PayerId}, merchantId: {MerchantId}, serviceId: {ServiceId}, rfdfu: {RFDFU}", amount, payerId, merchantId, serviceId, rfdfu);

                // Validate input parameters
                if (string.IsNullOrEmpty(amount) || string.IsNullOrEmpty(payerId) || string.IsNullOrEmpty(merchantId) || string.IsNullOrEmpty(serviceId) || string.IsNullOrEmpty(rfdfu))
                {
                    _logger.LogWarning("Invalid parameters passed to PaymentDone");
                    return BadRequest("Invalid parameters.");
                }

                // Safe conversion with validation
                if (!int.TryParse(payerId, out int parsedPayerId))
                {
                    _logger.LogWarning("Invalid payerId format: {PayerId}", payerId);
                    return BadRequest("Invalid payer ID format.");
                }

                if (!int.TryParse(merchantId, out int parsedMerchantId))
                {
                    _logger.LogWarning("Invalid merchantId format: {MerchantId}", merchantId);
                    return BadRequest("Invalid merchant ID format.");
                }

                if (!int.TryParse(serviceId, out int parsedServiceId))
                {
                    _logger.LogWarning("Invalid serviceId format: {ServiceId}", serviceId);
                    return BadRequest("Invalid service ID format.");
                }

                if (!int.TryParse(rfdfu, out int parsedRfdfu))
                {
                    _logger.LogWarning("Invalid rfdfu format: {RFDFU}", rfdfu);
                    return BadRequest("Invalid RFDFU format.");
                }

                if (!int.TryParse(NoOfQuantity, out int parsedNoOfQuantity))
                {
                    _logger.LogWarning("Invalid NoOfQuantity format: {NoOfQuantity}", NoOfQuantity);
                    return BadRequest("Invalid quantity format.");
                }

                // Validate amount format
                if (!decimal.TryParse(amount, out decimal parsedAmount))
                {
                    _logger.LogWarning("Invalid amount format: {Amount}", amount);
                    return BadRequest("Invalid amount format.");
                }

                // Fetch merchant details using merchantId
                _logger.LogDebug("Fetching merchant details for merchantId: {MerchantId}", parsedMerchantId);
                var getMerchantName = await _httpClient.GetAsync("Providers/" + parsedMerchantId);
                if (!getMerchantName.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch merchant details for merchantId: {MerchantId}", parsedMerchantId);
                    return NotFound("Merchant not found.");
                }
                var responseString = await getMerchantName.Content.ReadAsStringAsync();
                _logger.LogDebug("Merchant details response: {ResponseString}", responseString);
                var providerUser = JsonConvert.DeserializeObject<ProviderUser>(responseString);

                // Fetch service details using serviceId
                _logger.LogDebug("Fetching service details for serviceId: {ServiceId}", parsedServiceId);
                var getServiceName = await _httpClient.GetAsync("CategoryServices/getServiceByName?id=" + parsedServiceId);
                if (!getServiceName.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch service details for serviceId: {ServiceId}", parsedServiceId);
                    return NotFound("Service not found.");
                }
                responseString = await getServiceName.Content.ReadAsStringAsync();
                _logger.LogDebug("Service details response: {ResponseString}", responseString);
                var services = JsonConvert.DeserializeObject<Models.Service>(responseString);
                var service = services;
                if (service == null)
                {
                    _logger.LogWarning("Service not found in the response for serviceId: {ServiceId}", parsedServiceId);
                    return NotFound("Service not found.");
                }
                string ServiceName = string.Empty;
                //
                var ServiceNameRequest = await _httpClient.GetAsync("CategoryServices/getServiceNameFromServiceList?id=" + service.SID);
                if (!getServiceName.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch serviceList details for serviceListId: {ServiceListId}", service.SID);
                    return NotFound("serviceList not found.");
                }
                responseString = await ServiceNameRequest.Content.ReadAsStringAsync();
                _logger.LogDebug("Service details response: {ResponseString}", responseString);
                ServiceName = responseString;
                // Store relevant data in session using data protector
                _logger.LogDebug("Storing relevant data in session.");
                HttpContext.Session.SetEncryptedString("amount", amount, _protector);
                HttpContext.Session.SetEncryptedString("paymentType", "Online", _protector);
                HttpContext.Session.SetEncryptedString("payerId", parsedPayerId.ToString(), _protector);
                HttpContext.Session.SetEncryptedString("merchantId", parsedMerchantId.ToString(), _protector);
                HttpContext.Session.SetEncryptedString("serviceId", parsedServiceId.ToString(), _protector);
                HttpContext.Session.SetEncryptedString("rfdfu", parsedRfdfu.ToString(), _protector);
                HttpContext.Session.SetEncryptedString("NoOfQuantity", parsedNoOfQuantity.ToString(), _protector);
                HttpContext.Session.SetEncryptedString("ServicePrice", ServicePrice, _protector);

                // Call payment gateway API to process payment
                _logger.LogDebug("Calling payment gateway with amount: {Amount}, serviceName: {ServiceName}, providerName: {ProviderName}", parsedAmount, ServiceName, providerUser.ProviderName);
                var paymentGatewayResponse = await PaymentGateway(parsedAmount.ToString(), 1, ServiceName, providerUser.ProviderName);
                return paymentGatewayResponse;
            }
            catch (Exception ex)
            {
                // Log the error and return a 500 status code
                _logger.LogError(ex, "An unexpected error occurred while processing payment for userId: {UserId}", userId);
                return StatusCode(500, "An unexpected error occurred while processing payment.");
            }
        }

        [HttpGet]
        public async Task<ActionResult> Cancel()
        {
            _logger.LogDebug("Cancel action called.");
            try
            {
                // Retrieve session values and validate them
                string amount = HttpContext.Session.GetEncryptedString("amount", _protector);
                string paymentType = HttpContext.Session.GetEncryptedString("paymentType", _protector);
                _logger.LogDebug("Session values retrieved: amount: {Amount}, paymentType: {PaymentType}", amount, paymentType);

                if (!int.TryParse(HttpContext.Session.GetEncryptedString("payerId", _protector), out int payerId) ||
                    !int.TryParse(HttpContext.Session.GetEncryptedString("merchantId", _protector), out int merchantId) ||
                    !int.TryParse(HttpContext.Session.GetEncryptedString("serviceId", _protector), out int serviceId) ||
                    !int.TryParse(HttpContext.Session.GetEncryptedString("rfdfu", _protector), out int rfdfu) ||
                    !int.TryParse(HttpContext.Session.GetEncryptedString("NoOfQuantity", _protector), out int noOfQuantity))
                {
                    _logger.LogWarning("Invalid session data for payment cancellation.");
                    return BadRequest("Invalid session data.");
                }

                // Create payment history object to save payment details
                _logger.LogDebug("Creating payment history object.");
                var paymentHistory = new PaymentHistory
                {
                    PAYMENTTYPE = paymentType,
                    AMOUNT = amount,
                    PAYERID = payerId,
                    MERCHANTID = merchantId,
                    ISPAYMENTSUCCESS = 0,
                    SERVICEID = serviceId,
                    PAYMENTDATETIME = DateTime.Now,
                    Quantity = noOfQuantity
                };

                // Save payment history via API call
                _logger.LogDebug("Saving payment history via API.");
                var responseMessage = await _httpClient.PostAsync("Payment/sendRequestToSavePayment", Customs.GetJsonContent(paymentHistory));
                if (!responseMessage.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to save payment history.");
                    return StatusCode(500, "Failed to save payment history.");
                }

                // Parse response and validate payment history ID
                var responseString = await responseMessage.Content.ReadAsStringAsync();
                _logger.LogDebug("Payment history response: {ResponseString}", responseString);
                var updatedPaymentHistory = JsonConvert.DeserializeObject<PaymentHistory>(responseString);
                if (updatedPaymentHistory?.ID <= 0)
                {
                    _logger.LogWarning("Invalid payment history response.");
                    return StatusCode(500, "Failed to update payment history.");
                }

                // Create discount update object to update discount details
                _logger.LogDebug("Creating discount update object.");
                var discountUpdateInfo = new RequestForDisCountToUser
                {
                    RFDFU = rfdfu,
                    SID = serviceId,
                    MID = merchantId,
                    UID = payerId,
                    ResponseDateTime = DateTime.Now,
                    IsMerchantSelected = 1,
                    FINALPRICE = Convert.ToInt32(amount.Split('.')[0]),
                    IsPaymentDone = updatedPaymentHistory.ID
                };

                // Update discount information via API call
                _logger.LogDebug("Updating discount information via API.");
                responseMessage = await _httpClient.PostAsync("Payment/UpdateRequestForDisCountToUserForPaymentDone", Customs.GetJsonContent(discountUpdateInfo));
                if (!responseMessage.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to update discount information.");
                    return StatusCode(500, "Failed to update discount information.");
                }

                // Create tracker update object to track the service status
                _logger.LogDebug("Creating tracker update object.");
                var trackerUpdate = new TrackServiceStatusHistory
                {
                    ChangedByID = payerId,
                    StatusID = 10,
                    RFDFU = rfdfu,
                    ChangedByUserType = "User",
                    ChangedOn = DateTime.Now,
                    Comments = $"User[{payerId}] failed to make the payment for the Service[{serviceId}]."
                };

                // Send tracker update request via API
                _logger.LogDebug("Sending tracker update request via API.");
                var trackerUpdateResponse = await _httpClient.PostAsJsonAsync("TrackServiceStatusHistory/CreateStatus", trackerUpdate);
                if (!trackerUpdateResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to update tracker process.");
                    TempData["FailMessage"] = "Service updated but failed to update the tracker process.";
                }
                else
                {
                    _logger.LogInformation("Service and tracker process status updated successfully.");
                    TempData["SuccessMessage"] = "Service and tracker process status updated successfully.";
                }

                // Create notification object to notify merchant
                _logger.LogDebug("Creating notification object.");
                var notification = new Notification
                {
                    UserId = payerId.ToString(),
                    Message = $"User[{payerId}] failed to make the payment for the Service[{serviceId}].",
                    MerchantId = merchantId.ToString(),
                    RedirectToActionUrl = "",
                    MessageFromId = payerId,
                    SenderType = "Customer",
                    SID = serviceId
                };

                // Send notification request via API
                _logger.LogDebug("Sending notification request via API.");
                var res = await _httpClient.PostAsync("Notifications/CreateNotification?StatusId=11", Customs.GetJsonContent(notification));
                string resString = await res.Content.ReadAsStringAsync();
                _logger.LogInformation("Notification Response: {Response}", resString);

                // Set success response in TempData and return view
                _logger.LogDebug("Setting success response and returning view.");
                TempData["SuccessResponse"] = responseString;
                return View();
            }
            catch (Exception ex)
            {
                // Log the error and return a 500 status code
                _logger.LogError(ex, "An error occurred while processing the payment.");
                return StatusCode(500, "An internal server error occurred. Please try again later.");
            }
        }
        [HttpGet]
        public async Task<ActionResult> success()
        {
            try
            {
                string amount = HttpContext.Session.GetEncryptedString("amount", _protector);
                string ServicePrice = HttpContext.Session.GetEncryptedString("ServicePrice", _protector);
                string paymentType = HttpContext.Session.GetEncryptedString("paymentType", _protector);
                int payerId = Convert.ToInt32(HttpContext.Session.GetEncryptedString("payerId", _protector));
                int merchantId = Convert.ToInt32(HttpContext.Session.GetEncryptedString("merchantId", _protector));
                int serviceId = Convert.ToInt32(HttpContext.Session.GetEncryptedString("serviceId", _protector));
                string rfdfu = HttpContext.Session.GetEncryptedString("rfdfu", _protector);
                int NoOfQuantity = Convert.ToInt32(HttpContext.Session.GetEncryptedString("NoOfQuantity", _protector));

                var paymentHistory = new PaymentHistory
                {
                    PAYMENTTYPE = paymentType,
                    AMOUNT = amount,
                    PAYERID = payerId,
                    MERCHANTID = merchantId,
                    ISPAYMENTSUCCESS = 1,
                    SERVICEID = serviceId,
                    PAYMENTDATETIME = DateTime.Now,
                    Quantity = NoOfQuantity,
                    RFDFU = Convert.ToInt32(rfdfu)
                };

                var responseMessage = await _httpClient.PostAsync("Payment/sendRequestToSavePayment", Customs.GetJsonContent(paymentHistory));
                var responseString = await responseMessage.Content.ReadAsStringAsync();
                PaymentHistory UpdatedPaymentHistory = JsonConvert.DeserializeObject<PaymentHistory>(responseString);
                if (UpdatedPaymentHistory.ID > 0)
                {

                    string ReferralCode = HttpContext.Session.GetEncryptedString("ReferralCode", _protector);
                    if (string.IsNullOrEmpty(ReferralCode) || ReferralCode == "No Code.")
                    {
                        string userId = HttpContext.Session.GetEncryptedString("UserId", _protector);
                        string refcode = await GetReferralCode(Convert.ToInt32(userId));
                        // Store additional information in session
                        HttpContext.Session.SetEncryptedString("ReferralCode", refcode, _protector);
                    }


                    var discountUpdateInfo = new RequestForDisCountToUser
                    {
                        RFDFU = Convert.ToInt32(rfdfu),
                        SID = Convert.ToInt32(serviceId),
                        MID = Convert.ToInt32(merchantId),
                        UID = Convert.ToInt32(payerId),
                        ResponseDateTime = DateTime.Now,
                        IsMerchantSelected = 1,
                        //FINALPRICE = Convert.ToInt32(amount.Split('.')[0]),
                        IsPaymentDone = UpdatedPaymentHistory.ID
                    };

                    responseMessage = await _httpClient.PostAsync("Payment/UpdateRequestForDisCountToUserForPaymentDone", Customs.GetJsonContent(discountUpdateInfo));
                    responseString = await responseMessage.Content.ReadAsStringAsync();
                    //Tracker
                    var TrackerUpdate = new TrackServiceStatusHistory
                    {
                        ChangedByID = payerId,
                        StatusID = 11,
                        RFDFU = Convert.ToInt32(rfdfu),
                        ChangedByUserType = "User",
                        ChangedOn = DateTime.Now,
                        Comments = $"User[{payerId.ToString()}] has Made the payment Successfully. Merchant[{merchantId}] need to wait till files are uploaded]."
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
                        ChangedByID = payerId,
                        StatusID = 12,
                        RFDFU = Convert.ToInt32(rfdfu),
                        ChangedByUserType = "User",
                        ChangedOn = DateTime.Now,
                        Comments = $"User[{payerId.ToString()}] Need to start uploading the necessary Documents."
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
                    //GetDiscount amount
                    var DiscountAmount = await _httpClient.GetAsync("RequestForDisCountToUser/GetDiscountedAmount?rfdfu=" + rfdfu);
                    if (!DiscountAmount.IsSuccessStatusCode)
                    {
                        _logger.LogWarning($"Failed to fetch GetDiscount amount details for Request For DisCount To User Id: {rfdfu}");
                        return NotFound($"details for Request For DisCount To User Id: {rfdfu} not found.");
                    }
                    int DiscountPrice = Convert.ToInt32(await DiscountAmount.Content.ReadAsStringAsync());
                    // Trigger notification
                    var notification = new Notification
                    {
                        UserId = payerId.ToString(),
                        Message = $"User[{payerId.ToString()}] has paid the amount and it has been received. Wait for the alert for the document from customer for Service[{Convert.ToInt32(serviceId)}] requested. Paid Amount={Convert.ToDecimal(ServicePrice) - DiscountPrice} AED only.",
                        MerchantId = merchantId.ToString(),
                        RedirectToActionUrl = "",
                        MessageFromId = Convert.ToInt32(payerId),
                        SenderType = "Customer",
                        SID = Convert.ToInt32(serviceId)
                    };

                    var res = await _httpClient.PostAsync("Notifications/CreateNotification?StatusId=11", Customs.GetJsonContent(notification));
                    string resString = await res.Content.ReadAsStringAsync();
                    _logger.LogInformation("Notification Response : " + resString);
                }

                TempData["SuccessResponse"] = responseString;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the payment.");
                return StatusCode(500, "An internal server error occurred. Please try again later.");
            }
        }
        // Helper method to get the referral code
        private async Task<string> GetReferralCode(int? referrerCustomerId)
        {
            _logger.LogDebug("GetReferralCode called with referrerCustomerId: {ReferrerCustomerId}", referrerCustomerId);
            if (!referrerCustomerId.HasValue)
            {
                _logger.LogWarning("Referrer customer ID is null.");
                return "No Code.";
            }

            try
            {
                // Send request to retrieve referral code
                _logger.LogDebug("Sending request to retrieve referral code for CustomerId: {CustomerId}", referrerCustomerId);
                var response = await _httpClient.GetAsync($"Referral/GetUserReferral?CustomerId={referrerCustomerId}");
                _logger.LogDebug("Received response with status code: {StatusCode}", response.StatusCode);

                // Check if the response was successful
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to retrieve referral code. StatusCode: {StatusCode}", response.StatusCode);
                    return "No Code.";
                }

                // Deserialize the response
                var responseString = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Received response content: {ResponseString}", responseString);
                SResponse sResponse = JsonConvert.DeserializeObject<SResponse>(responseString);

                // Return the referral code if available
                if (sResponse.StatusCode == HttpStatusCode.OK && sResponse.Data != null)
                {
                    _logger.LogDebug("Referral code retrieved successfully.");
                    return sResponse.Data.ToString();
                }
                else
                {
                    _logger.LogWarning("Failed to retrieve referral code. StatusCode: {StatusCode}, Data: {Data}", sResponse.StatusCode, sResponse.Data);
                    return "No Code.";
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request failed while retrieving referral code.");
                return "Error retrieving code.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while retrieving referral code.");
                return "Error retrieving code.";
            }
        }
        [HttpGet]
        public async Task<ActionResult> Payment(string rfdfu, string uid, string merchantId, string ServicePrice)
        {
            string userId = HttpContext.Session.GetEncryptedString("UserId", _protector); // Placeholder for session user ID retrieval

            try
            {
                // Input validation
                if (string.IsNullOrWhiteSpace(rfdfu))
                {
                    _logger.LogWarning("RFDFU parameter is null or empty");
                    return BadRequest("RFDFU parameter is required.");
                }

                if (string.IsNullOrWhiteSpace(merchantId))
                {
                    _logger.LogWarning("Merchant ID parameter is null or empty");
                    return BadRequest("Merchant ID parameter is required.");
                }

                if (string.IsNullOrWhiteSpace(ServicePrice))
                {
                    _logger.LogWarning("Service price parameter is null or empty");
                    return BadRequest("Service price parameter is required.");
                }

                // Validate RFDFU format
                if (!int.TryParse(rfdfu, out int selectedRfdfuId))
                {
                    _logger.LogWarning("Invalid RFDFU format: {RFDFU}", rfdfu);
                    return BadRequest("Invalid RFDFU format.");
                }

                // Validate merchant ID format
                if (!int.TryParse(merchantId, out int parsedMerchantId))
                {
                    _logger.LogWarning("Invalid merchant ID format: {MerchantId}", merchantId);
                    return BadRequest("Invalid merchant ID format.");
                }

                // Validate service price format
                if (!decimal.TryParse(ServicePrice, out decimal parsedServicePrice))
                {
                    _logger.LogWarning("Invalid service price format: {ServicePrice}", ServicePrice);
                    return BadRequest("Invalid service price format.");
                }

                var jsonResponse = await _httpClient.GetAsync($"CategoryWithMerchant/AllResponsesFromMerchant?Uid={userId}");
                string responseString = await jsonResponse.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(responseString))
                {
                    List<RequestForDisCountToUserViewModel> categories = JsonConvert.DeserializeObject<List<RequestForDisCountToUserViewModel>>(responseString);
                    foreach (var item in categories)
                    {
                        item.FINALPRICE = item.ServicePrice - item.FINALPRICE;
                    }
                    ViewBag.ResponseForDisCountFromMerchant = categories.FirstOrDefault(x => x.RFDFU == selectedRfdfuId);
                    ViewBag.ServicePrice = parsedServicePrice.ToString("F2"); // Format to 2 decimal places
                }
                else
                {
                    _logger.LogWarning("Empty response received from API for userId: {UserId}", userId);
                    ViewBag.ResponseForDisCountFromMerchant = new List<RequestForDisCountToUserViewModel>();
                }
                ViewBag.Configuration = await SettingIndex();
            }
            catch (JsonSerializationException ex)
            {
                _logger.LogError(ex, "JSON deserialization error for userId: {UserId}", userId);
                ViewBag.ResponseForDisCountFromMerchant = new List<RequestForDisCountToUserViewModel>();
                ModelState.AddModelError(string.Empty, "Failed to load data.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while fetching data for userId: {UserId}", userId);
                ViewBag.ResponseForDisCountFromMerchant = new List<RequestForDisCountToUserViewModel>();
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while loading data.");
            }

            if (TempData.TryGetValue("SaveResponse", out var saveResponse))
            {
                ViewBag.SaveResponse = saveResponse.ToString();
            }

            return View();
        }
        public async Task<ConfigurationSettings> SettingIndex()
        {
            ConfigurationSettings setting = null;
            try
            {
                var response = await _httpClient.GetAsync("ConfigurationSettings");
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    setting = JsonConvert.DeserializeObject<ConfigurationSettings>(responseString);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching configuration settings.");
                TempData["FailMessage"] = "An error occurred while loading settings.";
            }
            return setting;
        }
        [HttpGet]
        public async Task<ActionResult> PaymentCart(string rfdfu, string uid, string merchantId, int Quantity, string Payment)
        {
            string userId = HttpContext.Session.GetEncryptedString("UserId", _protector); // Placeholder for session user ID retrieval

            try
            {
                // Input validation
                if (string.IsNullOrWhiteSpace(rfdfu))
                {
                    _logger.LogWarning("RFDFU parameter is null or empty");
                    return BadRequest("RFDFU parameter is required.");
                }

                if (string.IsNullOrWhiteSpace(merchantId))
                {
                    _logger.LogWarning("Merchant ID parameter is null or empty");
                    return BadRequest("Merchant ID parameter is required.");
                }

                // Validate merchant ID format
                if (!int.TryParse(merchantId, out int parsedMerchantId))
                {
                    _logger.LogWarning("Invalid merchant ID format: {MerchantId}", merchantId);
                    return BadRequest("Invalid merchant ID format.");
                }
                var jsonResponse = await _httpClient.GetAsync($"CategoryWithMerchant/AllResponsesFromMerchant?Uid={userId}");
                string responseString = await jsonResponse.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(responseString))
                {
                    int selectedServiceId = Convert.ToInt32(rfdfu);
                    List<RequestForDisCountToUserViewModel> categories = JsonConvert.DeserializeObject<List<RequestForDisCountToUserViewModel>>(responseString);
                    ViewBag.ResponseForDisCountFromMerchant = categories.FirstOrDefault(x => x.RFDFU == selectedServiceId);
                    ViewBag.Quantity = Quantity;
                }
                else
                {
                    _logger.LogWarning("Empty response received from API for userId: {UserId}", userId);
                    ViewBag.ResponseForDisCountFromMerchant = new List<RequestForDisCountToUserViewModel>();
                }
                ViewBag.Configuration = await SettingIndex();
            }
            catch (JsonSerializationException ex)
            {
                _logger.LogError(ex, "JSON deserialization error for userId: {UserId}", userId);
                ViewBag.ResponseForDisCountFromMerchant = new List<RequestForDisCountToUserViewModel>();
                ModelState.AddModelError(string.Empty, "Failed to load data.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while fetching data for userId: {UserId}", userId);
                ViewBag.ResponseForDisCountFromMerchant = new List<RequestForDisCountToUserViewModel>();
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while loading data.");
            }

            if (TempData.TryGetValue("SaveResponse", out var saveResponse))
            {
                ViewBag.SaveResponse = saveResponse.ToString();
            }
            ViewBag.ServicePrice = Convert.ToDecimal(Payment).ToString("F2"); // Format to 2 decimal places
            return View("Payment", new { rfdfu, uid, merchantId, Payment });
        }
        private async Task<ActionResult> PaymentGatewaytelr(string _price, long _quantity, string servicetype, string merchantname)
        {

            var domain = CustomerHttpsPort + "/MerchantResponseToUser/";
            string SuccessUrl = domain + "success";
            string CancelUrl = domain + "cancel";

            var optionstelr = new RestClientOptions("https://secure.telr.com/gateway/order.json");
            var client = new RestClient(optionstelr);
            var request = new RestRequest("");
            request.AddHeader("accept", "application/json");
            request.AddJsonBody("{\"method\":\"create\",\"store\":1234,\"authkey\":\"mykey1234\",\"framed\":0,\"order\":{\"cartid\":\"1234\",\"test\":\"1\",\"amount\":\"" + _price + "\",\"currency\":\"AED\",\"description\":\"1 month Visa\"},\"return\":{\"authorised\":\"" + SuccessUrl + "\",\"declined\":\"" + CancelUrl + "\",\"cancelled\":\"" + CancelUrl + "\"}}", false);
            var response = await client.PostAsync(request);

            return Ok(response.Content);
        }
        private async Task<ActionResult> PaymentGateway(string _price, long _quantity, string servicetype, string merchantname)
        {
            // Get Stripe API key from configuration
            var stripeSecretKey = HttpContext.RequestServices.GetService<IOptions<AppSettings>>()?.Value?.StripeSecretKey;
            if (string.IsNullOrEmpty(stripeSecretKey))
            {
                _logger.LogError("Stripe secret key not configured");
                return BadRequest("Payment gateway configuration error");
            }

            StripeConfiguration.ApiKey = stripeSecretKey;
            var domain = CustomerHttpsPort + "/MerchantResponseToUser/";
            var optionsProduct = new ProductCreateOptions
            {
                Name = servicetype, // Use dynamic service name instead of hardcoded
                Description = $"{servicetype} Purchase from {merchantname}",
            };
            var serviceProduct = new ProductService();
            Product product = serviceProduct.Create(optionsProduct);
            // Console.Write("Success! Here is your starter subscription product id: {0}\n", product.Id);
            double priceValue = Convert.ToDouble(_price);

            // Multiply by 100 and then convert to long
            long finalPrice = Convert.ToInt64(priceValue * 100);
            var optionsPrice = new PriceCreateOptions
            {
                UnitAmount = finalPrice,
                Currency = "aed",
                //Recurring = new PriceRecurringOptions
                //{
                //    Interval = "one-time",
                //},
                Product = product.Id
            };
            var servicePrice = new PriceService();
            Price price = servicePrice.Create(optionsPrice);
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>
                {
                  new SessionLineItemOptions
                  {
                    // Provide the exact Price ID (for example, pr_1234) of the product you want to sell
                    Price = price.Id,
                    Quantity = 1,
                  },
                },
                Mode = "payment",
                SuccessUrl = domain + "success",
                CancelUrl = domain + "cancel",


            };
            var service = new SessionService();
            Session session = service.Create(options);

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }
        public async Task<ActionResult> SelectFinalMerchant(RequestForDisCountToUserViewModel requestForDisCount)
        {
            if (requestForDisCount == null || requestForDisCount.RFDFU == 0)
            {
                _logger.LogWarning("Invalid request data passed to SelectFinalMerchant");
                return BadRequest("Invalid request data.");
            }

            _logger.LogInformation("SelectFinalMerchant called with request data: {RequestData}", requestForDisCount);

            try
            {
                var srbm = new RequestForDisCountToUser
                {
                    RFDFU = requestForDisCount.RFDFU,
                    UID = requestForDisCount.UID,
                    MID = requestForDisCount.MerchantID,
                    IsMerchantSelected = 1,
                    IsPaymentDone = 0
                };

                var responseMessage = await _httpClient.PostAsync("CategoryWithMerchant/SelectFinalMerchant", Customs.GetJsonContent(srbm));
                string responseString = await responseMessage.Content.ReadAsStringAsync();
                TempData["SaveResponse"] = responseString;
                //Tracker

                var TrackerUpdate = new TrackServiceStatusHistory
                {
                    ChangedByID = requestForDisCount.UID,
                    StatusID = 5,
                    RFDFU = requestForDisCount.RFDFU,
                    ChangedByUserType = "User",
                    ChangedOn = DateTime.Now,
                    Comments = $"User[{requestForDisCount.UID.ToString()}] has selected Merchant[{requestForDisCount.MerchantID}] as a final merchant For Service [{requestForDisCount.ServiceName}]."
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
                    ChangedByID = requestForDisCount.UID,
                    StatusID = 6,
                    RFDFU = requestForDisCount.RFDFU,
                    ChangedByUserType = "User",
                    ChangedOn = DateTime.Now,
                    Comments = $"User Started Service."
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

                TrackerUpdate = new TrackServiceStatusHistory
                {
                    ChangedByID = requestForDisCount.UID,
                    StatusID = 9,
                    RFDFU = requestForDisCount.RFDFU,
                    ChangedByUserType = "User",
                    ChangedOn = DateTime.Now,
                    Comments = $"Merchant Selected. Now Waiting For Payment To be Paid From Customer."
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
                    UserId = requestForDisCount.UID.ToString(),
                    Message = $"Congratulations. User[{requestForDisCount.UID.ToString()}] has selected you as a final merchant among 500 others for Service[{requestForDisCount.SID}] ",
                    MerchantId = requestForDisCount.MerchantID.ToString(),
                    RedirectToActionUrl = "#",
                    MessageFromId = Convert.ToInt32(requestForDisCount.UID),
                    SenderType = "Customer",
                    SID = requestForDisCount.SID
                };

                var res = await _httpClient.PostAsync("Notifications/CreateNotification?StatusId=5", Customs.GetJsonContent(notification));
                string resString = await res.Content.ReadAsStringAsync();
                _logger.LogInformation("Notification Response : " + resString);
                return RedirectToAction("MerchantResponseIndex");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while selecting the final merchant.");
                return StatusCode(500, "An internal server error occurred. Please try again later.");
            }
        }
        public async Task<ActionResult> DeSelectFinalMerchant(RequestForDisCountToUserViewModel requestForDisCount)
        {
            if (requestForDisCount == null || requestForDisCount.RFDFU == 0)
            {
                _logger.LogWarning("Invalid request data passed to DeSelectFinalMerchant");
                return BadRequest("Invalid request data.");
            }

            _logger.LogInformation("DeSelectFinalMerchant called with request data: {RequestData}", requestForDisCount);

            try
            {
                var srbm = new RequestForDisCountToUser
                {
                    RFDFU = requestForDisCount.RFDFU,
                    UID = requestForDisCount.UID,
                    MID = requestForDisCount.MerchantID,
                    IsMerchantSelected = -1
                };

                var responseMessage = await _httpClient.PostAsync("CategoryWithMerchant/DeSelectFinalMerchant", Customs.GetJsonContent(srbm));
                string responseString = await responseMessage.Content.ReadAsStringAsync();
                TempData["SaveResponse"] = responseString;
                // Trigger notification
                var notification = new Notification
                {
                    UserId = requestForDisCount.UID.ToString(),
                    Message = $"User[{requestForDisCount.UID.ToString()}] has decided not to move with you.",
                    MerchantId = requestForDisCount.MerchantID.ToString(),
                    RedirectToActionUrl = "#",
                    MessageFromId = Convert.ToInt32(requestForDisCount.UID),
                    SenderType = "Customer"
                };

                var res = await _httpClient.PostAsync("Notifications/CreateNotification?StatusId=7", Customs.GetJsonContent(notification));
                string resString = await res.Content.ReadAsStringAsync();
                _logger.LogInformation("Notification Response : " + resString);
                return RedirectToAction("MerchantResponseIndex");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deselecting the final merchant.");
                return StatusCode(500, "An internal server error occurred. Please try again later.");
            }
        }
        //CheckDocumentStatus
        [HttpGet]
        public async Task<ActionResult> CheckDocumentStatus(string RFDFU, string UID, string MerchantID)
        {
            try
            {
                int UserId = Convert.ToInt32(HttpContext.Session.GetEncryptedString("UserId", _protector));//Convert.ToInt32(HttpContext.Session.GetString("UserId"));
                int _rfdfu = Convert.ToInt32(RFDFU);
                int _MerchantID = Convert.ToInt32(MerchantID);
                var jsonResponse = await _httpClient.GetAsync("FileUpload/GetFilesList");
                string responseString = await jsonResponse.Content.ReadAsStringAsync();
                FileUploadViewModel model = new FileUploadViewModel();

                if (!string.IsNullOrEmpty(responseString))
                {
                    List<UploadedFile> documentList = JsonConvert.DeserializeObject<List<UploadedFile>>(responseString);
                    model.UploadedFiles = documentList.Where(x => x.UserId == UserId && x.RFDFU == _rfdfu && x.MerchantId == _MerchantID).ToList();
                }

                ViewBag.SaveResponse = model;
                return View(model);
            }
            catch (JsonSerializationException ex)
            {
                _logger.LogError(ex, "JSON deserialization error while fetching file list.");
                ModelState.AddModelError(string.Empty, "Failed to load data.");
                return View(new FileUploadViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while fetching file list.");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while loading data.");
                return View(new FileUploadViewModel());
            }
        }
        [HttpGet]
        public async Task<ActionResult> UploadDocuments(int rfdfu, string MerchantID)
        {
            try
            {
                int UserId = Convert.ToInt32(HttpContext.Session.GetEncryptedString("UserId", _protector));

                // Fetching the document list from the API
                var jsonResponse = await _httpClient.GetAsync($"FileUpload/GetServiceFileListByRFDFUId?rfdfuId={rfdfu}&UploadedBy=Customer");
                string responseString = await jsonResponse.Content.ReadAsStringAsync();

                List<DocumentInfo> documentList = new List<DocumentInfo>();

                if (!string.IsNullOrEmpty(responseString))
                {
                    documentList = JsonConvert.DeserializeObject<List<DocumentInfo>>(responseString);
                }
                //get Quantity of Service
                ViewBag.MerchantID = MerchantID;
                ViewBag.RFDFU = rfdfu;

                ViewBag.DocumentList = documentList;
                ViewBag.ServiceName = await GetServiceName(documentList[0].SID);
                ViewBag.Quantity = Convert.ToInt32(await GetQuantityOfService(documentList[0].RFDFU));
                return View(new FileUploadViewModel { UserId = UserId, RFDFU = rfdfu });
            }
            catch (JsonSerializationException ex)
            {
                _logger.LogError(ex, "JSON deserialization error while fetching file list.");
                ModelState.AddModelError(string.Empty, "Failed to load data.");
                return View(new FileUploadViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while fetching file list.");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while loading data.");
                return View(new FileUploadViewModel());
            }
        }
        [HttpPost]
        public async Task<IActionResult> UploadDocuments(int RFDFU, FileUploadViewModel model)
        {
            string FileNameTypes = string.Empty;
            if (model == null || model.Files == null || model.Files.Count == 0)
            {
                _logger.LogWarning("Invalid model passed to UploadDocuments");
                TempData["FailMessage"] = "Invalid passed to UploadDocuments";
                return BadRequest("Invalid model.");
            }

            _logger.LogInformation("UploadDocuments called with model: {Model}", model);

            try
            {
                if (ModelState.IsValid)
                {
                    int loginUserId = Convert.ToInt32(HttpContext.Session.GetEncryptedString("UserId", _protector));
                    var username = $"Documents_{loginUserId}";

                    var folderPath = Path.Combine(_environment.WebRootPath, "uploads", username);
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    var fileUploadModelAPI = new FileUploadModelAPI
                    {
                        UserId = loginUserId,
                        MID = model.Merchant,
                        RFDFU = RFDFU,
                        UploadedBy = "Customer",
                        UploadedFiles = new List<UploadedFile>()
                    };



                    //Get List Of documents:
                    var jsonResponse = await _httpClient.GetAsync($"FileUpload/GetServiceFileListByRFDFUId?rfdfuId={RFDFU}&UploadedBy=Customer");
                    string responseStringDocList = await jsonResponse.Content.ReadAsStringAsync();

                    List<DocumentInfo> documentList = new List<DocumentInfo>();
                    if (!string.IsNullOrEmpty(responseStringDocList))
                    {
                        documentList = JsonConvert.DeserializeObject<List<DocumentInfo>>(responseStringDocList);
                    }
                    int documentsPerPerson = documentList.Count; // Number of documents each person uploads
                    int personCount = model.PersonNames.Count; // Total number of persons


                    // Separate files for each person
                    for (int i = 0; i < personCount; i++)
                    {
                        var filesForPerson = model.Files.Skip(i * documentsPerPerson).Take(documentsPerPerson).ToList();
                        var personName = model.PersonNames.ElementAtOrDefault(i) ?? $"Person_{i + 1}";
                        int j = 0;
                        foreach (var file in filesForPerson)
                        {
                            if (file.Length > 0)
                            {
                                //var fileName = Path.GetFileName(file.FileName);
                                //var sanitizedFileName = $"{personName}_{fileName}";
                                //var filePath = Path.Combine(folderPath, sanitizedFileName);


                                var fileExtension = Path.GetExtension(file.FileName);
                                var serviceDocumentName = documentList[j].ServiceDocumentName.Replace(" ", ""); // Remove spaces
                                var fileName = $"{personName}_{serviceDocumentName}_{documentList[j].ServiceDocumenListtId}_{j}_{model.UserId}_{model.Merchant}_{documentList[j].ServiceDocumenListtId}_{RFDFU}{fileExtension}";
                                var filePath = Path.Combine(folderPath, fileName);

                                FileNameTypes += serviceDocumentName + ",";


                                using (var stream = new FileStream(filePath, FileMode.Create))
                                {
                                    await file.CopyToAsync(stream);
                                }

                                var uploadedFile = new UploadedFile
                                {
                                    FileName = fileName,
                                    ContentType = file.ContentType,
                                    FileSize = file.Length,
                                    FolderName = username,
                                    Status = "Pending",
                                    UserId = loginUserId,
                                    MerchantId = model.Merchant,
                                    DocumentAddedDate = DateTime.Now,
                                    DocumentModifiedDate = DateTime.Now,
                                    RFDFU = RFDFU,
                                    UploadedBy = "Customer",
                                };

                                fileUploadModelAPI.UploadedFiles.Add(uploadedFile);
                            }
                            j++;
                        }
                    }

                    var responseMessage = await _httpClient.PostAsync("FileUpload/UploadFiles", Customs.GetJsonContent(fileUploadModelAPI));
                    string responseString = await responseMessage.Content.ReadAsStringAsync();
                    TempData["SuccessResponse"] = responseString;
                    //Tracker
                    var TrackerUpdate = new TrackServiceStatusHistory
                    {
                        ChangedByID = loginUserId,
                        StatusID = 13,
                        RFDFU = RFDFU,
                        ChangedByUserType = "User",
                        ChangedOn = DateTime.Now,
                        Comments = $"User[{loginUserId.ToString()}] has Uploaded Some Files. Merchant[{model.Merchant}] need to Check and verify the files now."
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
                    // Trigger notification
                    var notification = new Notification
                    {
                        UserId = loginUserId.ToString(),
                        Message = $"User[{loginUserId}] has uploaded ({FileNameTypes.TrimEnd(',')}) documents for the Service[{documentList[0].SID}]",
                        MerchantId = model.Merchant.ToString(),
                        RedirectToActionUrl = "/MerchantResponseToUser/GetUsersWithDocuments",
                        MessageFromId = loginUserId,
                        SenderType = "Customer",
                        SID = documentList[0].SID
                    };

                    var notificationResponse = await _httpClient.PostAsync("Notifications/CreateNotification?StatusId=13", Customs.GetJsonContent(notification));
                    string notificationResponseString = await notificationResponse.Content.ReadAsStringAsync();
                    _logger.LogInformation("Notification Response : " + notificationResponseString);

                    return RedirectToAction("MerchantResponseIndex");
                }

                return RedirectToAction("MerchantResponseIndex");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while uploading documents.");
                return StatusCode(500, "An error occurred. Please try again later.");
            }
        }
        /***********Existing Working Code End*************/
        [HttpGet]
        public async Task<ActionResult> ReviewDocument(int userId)
        {
            try
            {
                var jsonResponse = await _httpClient.GetAsync($"FileUpload/ReviewDocuments/{userId}");
                string responseString = await jsonResponse.Content.ReadAsStringAsync();
                FileUploadViewModel model = new FileUploadViewModel();

                if (!string.IsNullOrEmpty(responseString))
                {
                    List<UploadedFile> documentList = JsonConvert.DeserializeObject<List<UploadedFile>>(responseString);
                    model.UploadedFiles = documentList.Where(x => x.UserId == 1).ToList();
                }
                ViewBag.UserId = userId;
                ViewBag.SaveResponse = model;
                return View(model);
            }
            catch (JsonSerializationException ex)
            {
                _logger.LogError(ex, "JSON deserialization error while reviewing document for userId: {UserId}", userId);
                ModelState.AddModelError(string.Empty, "Failed to load data.");
                return View(new FileUploadViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while reviewing document for userId: {UserId}", userId);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while loading data.");
                return View(new FileUploadViewModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> ReUploadDocument(IFormFile file, string FileName, int UFID, int RFDFU, int MID)
        {
            string existingFileName = FileName;
            if (file == null || file.Length == 0 || existingFileName == null)
            {
                _logger.LogWarning("No file uploaded for re-upload.");
                return BadRequest("Please select a file to upload.");
            }

            try
            {
                // Extract the person's name from the existing file name before the first underscore
                var personName = existingFileName.Split('_')[0];
                var loginUserId = Convert.ToInt32(HttpContext.Session.GetEncryptedString("UserId", _protector));
                var folderPath = Path.Combine(_environment.WebRootPath, "uploads", $"Documents_{loginUserId}");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var existingFilePath = Path.Combine(folderPath, existingFileName);

                // Delete the existing file if it exists
                if (System.IO.File.Exists(existingFilePath))
                {
                    System.IO.File.Delete(existingFilePath);
                    _logger.LogInformation($"Existing file '{existingFileName}' deleted successfully.");
                }

                // Create a new filename with the original structure
                var fileExtension = Path.GetExtension(file.FileName);
                var newFileName = existingFileName; // Retain the original file name
                var newFilePath = Path.Combine(folderPath, newFileName);

                // Save the new file
                using (var stream = new FileStream(newFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation($"New file '{newFileName}' uploaded successfully.");

                // Optional: Update the file metadata in the API if necessary
                var updatedFile = new UploadedFile
                {
                    FileName = newFileName,
                    ContentType = file.ContentType,
                    FileSize = file.Length,
                    FolderName = $"Documents_{loginUserId}",
                    Status = "Pending",
                    UserId = loginUserId,
                    DocumentModifiedDate = DateTime.Now,
                    UFID = UFID
                };

                // Send the updated file info to the API
                var response = await _httpClient.PostAsync($"FileUpload/UpdateDocumenttoPendingStatus/{UFID}", Customs.GetJsonContent(UFID));
                string apiResponse = await response.Content.ReadAsStringAsync();

                TempData["SuccessResponse"] = apiResponse;
                _logger.LogInformation($"Metadata for '{newFileName}' updated successfully in the API.");
                //Get Service Id for Tracking and notification Purpose:
                //
                var jsonServiceIdResponse = await _httpClient.GetAsync("RequestForDisCountToUser/GetServiceIdFromRfdfu");
                string ServiceIdString = await jsonServiceIdResponse.Content.ReadAsStringAsync();
                //Tracker
                var TrackerUpdate = new TrackServiceStatusHistory
                {
                    ChangedByID = loginUserId,
                    StatusID = 20,
                    RFDFU = RFDFU,
                    ChangedByUserType = "User",
                    ChangedOn = DateTime.Now,
                    Comments = $"User[{loginUserId.ToString()}] has Uploaded Some Files. Merchant need to Check and verify the files now  for the Service[{ServiceIdString}]."
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
                // Trigger notification
                var notification = new Notification
                {
                    UserId = loginUserId.ToString(),
                    Message = $"User[{loginUserId}] has uploaded some documents for the Service[{ServiceIdString}].",
                    MerchantId = MID.ToString(),
                    RedirectToActionUrl = "/MerchantResponseToUser/GetUsersWithDocuments",
                    MessageFromId = loginUserId,
                    SenderType = "Customer"
                };

                var notificationResponse = await _httpClient.PostAsync("Notifications/CreateNotification?StatusId=13", Customs.GetJsonContent(notification));
                string notificationResponseString = await notificationResponse.Content.ReadAsStringAsync();
                _logger.LogInformation("Notification Response : " + notificationResponseString);


                return RedirectToAction("CheckDocumentStatus", new { RFDFU, UID = loginUserId, MerchantID = MID });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while re-uploading the document '{existingFileName}'.");
                return StatusCode(500, "An error occurred. Please try again later.");
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetUsersWithDocuments()
        {
            int merchantId = 1; // Placeholder for session merchant ID retrieval

            try
            {
                var jsonResponse = await _httpClient.GetAsync("FileUpload/UsersWithDocuments");
                string responseString = await jsonResponse.Content.ReadAsStringAsync();
                UserDocumentsViewModel model = new UserDocumentsViewModel();

                if (!string.IsNullOrEmpty(responseString))
                {
                    List<UserDocumentsViewModel> documentList = JsonConvert.DeserializeObject<List<UserDocumentsViewModel>>(responseString);
                    ViewBag.UsersWithDocuments = documentList.Where(x => x.MID == merchantId).ToList();
                }

                return View();
            }
            catch (JsonSerializationException ex)
            {
                _logger.LogError(ex, "JSON deserialization error while fetching users with documents.");
                ModelState.AddModelError(string.Empty, "Failed to load data.");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while fetching users with documents.");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while loading data.");
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> VerifyDocument(string documentId, string userId)
        {
            _logger.LogInformation("VerifyDocument method called for DocumentId: {DocumentId}", documentId);
            try
            {
                var responseMessage = await _httpClient.PostAsync($"FileUpload/VerifyDocument/{documentId}", Customs.GetJsonContent(documentId));
                string responseString = await responseMessage.Content.ReadAsStringAsync();

                _logger.LogInformation($"Document verification response for DocumentId: {documentId}. Response : {responseString}");
                TempData["SuccessMessage"] = responseString;
                ViewBag.SaveResponse = responseString;

                return RedirectToAction("ReviewDocument", new { userId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while verifying document for DocumentId: {DocumentId}", documentId);
                return StatusCode(500, "Internal server error.");
            }
        }
        [HttpPost]
        public async Task<IActionResult> Review([FromBody] ReviewCreate review)
        {
            _logger.LogInformation("Review method called");
            try
            {
                int UserId = Convert.ToInt32(HttpContext.Session.GetEncryptedString("UserId", _protector));
                var responseMessage = await _httpClient.PostAsync($"ReviewsApi/CreateReview", Customs.GetJsonContent(review));
                string responseString = await responseMessage.Content.ReadAsStringAsync();

                _logger.LogInformation($"ReviewsApi response for ServiceId: {review.ServiceId}. Response : {responseString}");
                //TempData["SuccessMessage"] = responseString;
                ViewBag.SaveResponse = responseString;
                // Create tracker update object to track the service status
                _logger.LogDebug("Creating tracker update object.");
                //var trackerUpdate = new TrackServiceStatusHistory
                //{
                //    ChangedByID = payerId,
                //    StatusID = 10,
                //    RFDFU = rfdfu,
                //    ChangedByUserType = "User",
                //    ChangedOn = DateTime.Now,
                //    Comments = $"User[{payerId}] failed to make the payment for the Service[{serviceId}]."
                //};
                //// Send tracker update request via API
                //_logger.LogDebug("Sending tracker update request via API.");
                //var trackerUpdateResponse = await _httpClient.PostAsJsonAsync("TrackServiceStatusHistory/CreateStatus", trackerUpdate);
                //if (!trackerUpdateResponse.IsSuccessStatusCode)
                //{
                //    _logger.LogWarning("Failed to update tracker process.");
                //    TempData["FailMessage"] = "Service updated but failed to update the tracker process.";
                //}
                //else
                //{
                //    _logger.LogInformation("Service and tracker process status updated successfully.");
                //    TempData["SuccessMessage"] = "Service and tracker process status updated successfully.";
                //}

                // Create notification object to notify merchant
                _logger.LogDebug("Creating notification object.");
                var notification = new Notification
                {
                    UserId = UserId.ToString(),
                    Message = $"User[{UserId}] has sent a review for you for the Service[{review.ServiceId}].",
                    MerchantId = review.merchantId.ToString(),
                    RedirectToActionUrl = "/ReviewsClient/ProviderReviews",
                    MessageFromId = UserId,
                    SenderType = "Customer",
                    SID = review.ServiceId
                };

                // Send notification request via API
                _logger.LogDebug("Sending notification request via API.");
                var res = await _httpClient.PostAsync("Notifications/CreateNotification?StatusId=11", Customs.GetJsonContent(notification));
                string resString = await res.Content.ReadAsStringAsync();
                _logger.LogInformation("Notification Response: {Response}", resString);
                return RedirectToAction("ReviewDocument", new { UserId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while Review save: {ex}", ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }
        [HttpGet]
        public async Task<IActionResult> DeleteDocument(string documentId, int RFDFU, int MID)
        {
            _logger.LogInformation("DeleteDocument method called for DocumentId: {DocumentId}", documentId);
            try
            {
                var loginUserId = Convert.ToInt32(HttpContext.Session.GetEncryptedString("UserId", _protector));
                var responseMessage = await _httpClient.PostAsync($"FileUpload/DeleteDocument/{documentId}", Customs.GetJsonContent(documentId));
                string responseString = await responseMessage.Content.ReadAsStringAsync();
                _logger.LogInformation($"Document Deletion response for DocumentId: {documentId}. Response : {responseString}");
                //TempData["SaveResponse"] = responseMessage;
                ViewBag.SaveResponse = responseString;
                //return RedirectToAction("CheckDocumentStatus");//RedirectToAction("CheckDocumentStatus", new { RFDFU, loginUserId, MID });
                return RedirectToAction("CheckDocumentStatus", new { RFDFU, UID = loginUserId, MerchantID = MID });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while verifying document for DocumentId: {DocumentId}", documentId);
                return StatusCode(500, "Internal server error.");
            }
        }
        private async Task<string> GetServiceName(int id)
        {
            try
            {
                var jsonResponse = await _httpClient.GetAsync($"Service/ServiceNameById?id=" + id);
                var ServiceName = await jsonResponse.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(ServiceName))
                {
                    try
                    {
                        return ServiceName;
                    }
                    catch (JsonSerializationException ex)
                    {
                        // Log the exception details
                        _logger.LogError(ex, "JSON deserialization error.");
                        // Handle the error response accordingly
                        ModelState.AddModelError(string.Empty, "Failed to load Data.");
                        return "Failed to load Data";
                    }
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while Retrieving Service Name For Id: {id}", id);
                return "Internal server error.";
            }
        }
        private async Task<string> GetQuantityOfService(int id)
        {
            try
            {
                var jsonResponse = await _httpClient.GetAsync($"FileUpload/GetQuantityOfService?rfdfuId=" + id);
                var ServiceName = await jsonResponse.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(ServiceName))
                {
                    try
                    {
                        return ServiceName;
                    }
                    catch (JsonSerializationException ex)
                    {
                        // Log the exception details
                        _logger.LogError(ex, "JSON deserialization error.");
                        // Handle the error response accordingly
                        ModelState.AddModelError(string.Empty, "Failed to load Data.");
                        return "Failed to load Data";
                    }
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while Retrieving Quantity For Id: {id}", id);
                return "Internal server error.";
            }
        }
    }
    public class RequestForDisCountToUserViewModel
    {
        public int SID { get; set; }
        public int ServicePrice { get; set; }
        public string ServiceName { get; set; }
        public int MerchantID { get; set; }
        public decimal FINALPRICE { get; set; }
        public int UID { get; set; }
        public int RFDFU { get; set; }
        public int IsMerchantSelected { get; set; }
        public int IsPaymentDone { get; set; }
        public DateTime ResponseDateTime { get; set; }
        public string? CurrentStatus { get; set; }
        public bool IsRequestCompleted { get; set; }
    }
    public class FileUploadViewModel
    {
        [Required]
        public IFormFileCollection Files { get; set; }
        //public List<List<IFormFile>> UserDocuments { get; set; }// = new List<List<IFormFile>>(); // List of file lists for multiple persons
        public int UserId { get; set; }
        public int Merchant { get; set; }
        public int RFDFU { get; set; }
        public List<UploadedFile>? UploadedFiles { get; set; }
        public List<string> PersonNames { get; set; } = new List<string>(); // Names for each person
    }
    public class UserDocumentsViewModel
    {
        public int UserId { get; set; }
        public int MID { get; set; }
        public int DocumentCount { get; set; }
    }
    public class FileUploadModelAPI
    {

        public int UserId { get; set; }
        public int MID { get; set; }
        public int RFDFU { get; set; }
        public string UploadedBy { get; set; }
        public List<UploadedFile>? UploadedFiles { get; set; }
    }
}
