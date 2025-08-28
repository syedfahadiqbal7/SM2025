using AFFZ_Customer.Models;
using AFFZ_Customer.Utils;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Net;

namespace AFFZ_Customer.Controllers
{
    public class EligibilityController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<EligibilityController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        IDataProtector _protector;

        public EligibilityController(IHttpClientFactory httpClientFactory, ILogger<EligibilityController> logger, IHttpContextAccessor httpContextAccessor, IDataProtectionProvider provider)
        {
            _httpClient = httpClientFactory.CreateClient("Main");
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _protector = provider.CreateProtector("Example.SessionProtection");
        }
        public async Task<IActionResult> ManageRequests()
        {
            int userId = Convert.ToInt32(HttpContext.Session.GetEncryptedString("UserId", _protector));
            List<EligibilityRequestDTO> eligibilityRequestDTOs = new List<EligibilityRequestDTO>();
            try
            {
                var response = await _httpClient.GetAsync($"Eligibility/GetUserPendingRequests?userId={userId}");
                var responseString = await response.Content.ReadAsStringAsync();

                var jsonResponse = JsonConvert.DeserializeObject<ResponseModel>(responseString);

                if (response.StatusCode == HttpStatusCode.OK && jsonResponse?.data != null)
                {
                    // Deserialize the `data` property properly
                    eligibilityRequestDTOs = JsonConvert.DeserializeObject<List<EligibilityRequestDTO>>(jsonResponse.data.ToString());
                    foreach (var item in eligibilityRequestDTOs)
                    {
                        var GetService = await _httpClient.GetAsync($"ServicesList/GetServiceNameById?id=" + item.Service.SID);
                        item.Service.serviceName = await GetService.Content.ReadAsStringAsync();
                    }
                }
                else
                {
                    TempData["FailMessage"] = "Failed to fetch requests.";
                }


                return View("ManageRequests", eligibilityRequestDTOs);
            }
            catch (Exception ex)
            {
                TempData["FailMessage"] = ex.Message;
                return View("ManageRequests", eligibilityRequestDTOs);
            }
        }
        [HttpGet]
        public IActionResult SubmitRequest(int merchantId, int serviceId, string srvicename, string ServiceImage)
        {
            var model = new EligibilityRequestDTO
            {
                ServiceID = serviceId,
                MerchantId = merchantId
                // Add other prefilled fields if needed
            };
            // Example: Predefined list of countries

            ViewBag.CountryList = new List<string>
            {
                "Afghanistan", "Albania", "Algeria", "Andorra", "Angola", "Antigua and Barbuda", "Argentina",
    "Armenia", "Australia", "Austria", "Azerbaijan", "Bahamas", "Bahrain", "Bangladesh",
    "Barbados", "Belarus", "Belgium", "Belize", "Benin", "Bhutan", "Bolivia", "Bosnia and Herzegovina",
    "Botswana", "Brazil", "Brunei", "Bulgaria", "Burkina Faso", "Burundi", "Cabo Verde",
    "Cambodia", "Cameroon", "Canada", "Central African Republic", "Chad", "Chile", "China",
    "Colombia", "Comoros", "Congo", "Costa Rica", "Croatia", "Cuba", "Cyprus", "Czech Republic",
    "Denmark", "Djibouti", "Dominica", "Dominican Republic", "Ecuador", "Egypt", "El Salvador",
    "Equatorial Guinea", "Eritrea", "Estonia", "Eswatini", "Ethiopia", "Fiji", "Finland",
    "France", "Gabon", "Gambia", "Georgia", "Germany", "Ghana", "Greece", "Grenada", "Guatemala",
    "Guinea", "Guinea-Bissau", "Guyana", "Haiti", "Honduras", "Hungary", "Iceland", "India", "Indonesia",
    "Iran", "Iraq", "Ireland", "Israel", "Italy", "Jamaica", "Japan", "Jordan", "Kazakhstan",
    "Kenya", "Kiribati", "Kuwait", "Kyrgyzstan", "Laos", "Latvia", "Lebanon", "Lesotho",
    "Liberia", "Libya", "Liechtenstein", "Lithuania", "Luxembourg", "Madagascar", "Malawi", "Malaysia",
    "Maldives", "Mali", "Malta", "Marshall Islands", "Mauritania", "Mauritius", "Mexico",
    "Micronesia", "Moldova", "Monaco", "Mongolia", "Montenegro", "Morocco", "Mozambique",
    "Myanmar", "Namibia", "Nauru", "Nepal", "Netherlands", "New Zealand", "Nicaragua",
    "Niger", "Nigeria", "North Korea", "North Macedonia", "Norway", "Oman", "Pakistan", "Palau",
    "Palestine", "Panama", "Papua New Guinea", "Paraguay", "Peru", "Philippines", "Poland",
    "Portugal", "Qatar", "Romania", "Russia", "Rwanda", "Saint Kitts and Nevis", "Saint Lucia",
    "Saint Vincent and the Grenadines", "Samoa", "San Marino", "Sao Tome and Principe",
    "Saudi Arabia", "Senegal", "Serbia", "Seychelles", "Sierra Leone", "Singapore", "Slovakia",
    "Slovenia", "Solomon Islands", "Somalia", "South Africa", "South Korea", "South Sudan", "Spain",
    "Sri Lanka", "Sudan", "Suriname", "Sweden", "Switzerland", "Syria", "Tajikistan",
    "Tanzania", "Thailand", "Timor-Leste", "Togo", "Tonga", "Trinidad and Tobago", "Tunisia",
    "Turkey", "Turkmenistan", "Tuvalu", "Uganda", "Ukraine", "United Arab Emirates", "United Kingdom",
    "United States", "Uruguay", "Uzbekistan", "Vanuatu", "Vatican City", "Venezuela", "Vietnam",
    "Yemen", "Zambia", "Zimbabwe"
            };
            ViewBag.ServiceName = srvicename;
            ViewBag.ServiceImage = ServiceImage;
            return PartialView("SubmitRequest", model);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitRequest(EligibilityRequestDTO request)
        {
            try
            {
                string userId = HttpContext.Session.GetEncryptedString("UserId", _protector);
                request.CustomerID = Convert.ToInt32(userId);
                request.IsRequestRejected = false;
                request.ReasonForRejection = "";
                request.IsRespondedByMerchant = false;
                if (!ModelState.IsValid)
                {
                    return PartialView("_SubmitRequest", request);
                }
                var response = await _httpClient.PostAsync($"Eligibility/SubmitRequest", Customs.GetJsonContent(request));
                response.EnsureSuccessStatusCode();
                var responseString = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonConvert.DeserializeObject<SResponse>(responseString);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    TempData["SuccessMessage"] = "Eligibility request submitted successfully.";
                    // Create notification object to notify merchant
                    _logger.LogDebug("Creating notification object.");
                    var notification = new Notification
                    {
                        UserId = userId,
                        Message = $"User[{userId}] has requested for eligibility check for the Service[{request.ServiceID}].",
                        MerchantId = request.MerchantId.ToString(),
                        RedirectToActionUrl = "/Eligibility/ManageRequests",
                        MessageFromId = Convert.ToInt32(userId),
                        SenderType = "Customer",
                        SID = request.ServiceID
                    };

                    // Send notification request via API
                    _logger.LogDebug("Sending notification request via API.");
                    var res = await _httpClient.PostAsync("Notifications/CreateNotification?StatusId=11", Customs.GetJsonContent(notification));
                    string resString = await res.Content.ReadAsStringAsync();
                    _logger.LogInformation("Notification Response: {Response}", resString);
                    return Ok("Request Sent. Please Navigate to Track Request Menu to check your request status.");
                    //return RedirectToAction("TrackRequest");
                }
                else
                {
                    TempData["FailMessage"] = jsonResponse?.Message ?? "An error occurred.";
                    return View("SubmitRequest");
                }


            }
            catch (Exception ex)
            {
                TempData["FailMessage"] = ex.Message;
                return View("SubmitRequest");
            }
        }

        [HttpGet("TrackRequest/{requestId}")]
        public async Task<IActionResult> TrackRequest(int requestId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Eligibility/RequestStatus/{requestId}");
                var responseString = await response.Content.ReadAsStringAsync();

                var jsonResponse = JsonConvert.DeserializeObject<ResponseModel>(responseString);

                if (response.StatusCode == HttpStatusCode.OK && jsonResponse?.data != null)
                {
                    return View("TrackRequest", jsonResponse.data);
                }
                else
                {
                    TempData["FailMessage"] = "Failed to fetch request status.";
                    return View("TrackRequest");
                }
            }
            catch (Exception ex)
            {
                TempData["FailMessage"] = ex.Message;
                return View("TrackRequest");
            }
        }
        // Helper method to get a list of services for dropdown
        public async Task<List<SelectListItem>> ServiceListItems(int sid)
        {
            _logger.LogInformation("ServiceListItems called with service name: {ServiceListId}", sid); // Log method entry with service name

            try
            {
                var jsonResponse = await _httpClient.GetAsync("ServicesList/GetServicesList"); // Send GET request to get list of services
                if (!jsonResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to fetch services list. Status code: {StatusCode}", jsonResponse.StatusCode); // Log error if fetching services list fails
                    return new List<SelectListItem>(); // Return empty list in case of failure
                }

                var responseString = await jsonResponse.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(responseString))
                {
                    List<ServicesList> serviceList = JsonConvert.DeserializeObject<List<ServicesList>>(responseString); // Deserialize response to list of ServicesList
                    return serviceList.Select(i => new SelectListItem
                    {
                        Text = i.ServiceName,
                        Value = i.ServiceListID.ToString(),
                        Selected = sid == i.ServiceListID // Set selected item based on input parameter
                    }).ToList();
                }
            }
            catch (JsonSerializationException ex)
            {
                _logger.LogError(ex, "JSON deserialization error occurred while fetching services list."); // Log JSON deserialization exception
                ModelState.AddModelError(string.Empty, "Failed to load Data."); // Add model error for deserialization issues
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error occurred while fetching services list."); // Log HTTP request exception
                ModelState.AddModelError(string.Empty, "Error communicating with the server. Please try again later."); // Add model error for communication issues
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while fetching services list."); // Log any other exceptions
                ModelState.AddModelError(string.Empty, "Failed to load Data."); // Add generic error message to model state
            }

            return new List<SelectListItem>(); // Return empty list in case of exception
        }

        [HttpPost("UpdateEligibilityRequest")]
        public async Task<IActionResult> UpdateEligibilityRequest(int requestId, string status, string? reasonForRejection, string customerId, string ServiceId, string ServiceName, string? Questions)
        {
            int merchantId = Convert.ToInt32(HttpContext.Session.GetEncryptedString("ProviderId", _protector));
            try
            {
                var updateDTO = new EligibilityRequestDTO
                {
                    RequestID = requestId,
                    StatusID = 21,
                    ReasonForRejection = status == "Rejected" ? reasonForRejection : status,
                    RequestDetails = status == "RequestMore" ? Questions : status,
                    UpdatedDate = DateTime.UtcNow.ToLocalTime(),
                    IsRequestRejected = (status == "Rejected") ? true : false,
                    IsRespondedByMerchant = true
                };

                // Logging the update request
                _logger.LogInformation("Updating eligibility request ID {RequestID} with Status: {Status}", updateDTO.RequestID, updateDTO.StatusID);

                var response = await _httpClient.PostAsync("Eligibility/UpdateRequest", Customs.GetJsonContent(updateDTO));
                var responseString = await response.Content.ReadAsStringAsync();

                var jsonResponse = JsonConvert.DeserializeObject<ResponseModel>(responseString);

                if (response.StatusCode == HttpStatusCode.OK && jsonResponse?.statusCode == 200)
                {
                    TempData["SuccessMessage"] = "Request updated successfully.";
                    _logger.LogInformation("Eligibility request ID {RequestID} updated successfully.", updateDTO.RequestID);
                    // Trigger notification
                    var notification = new Notification
                    {
                        UserId = customerId,
                        Message = $"Merchant[{merchantId.ToString()}] has responded for the Eligibility for Service[{ServiceId}].",
                        MerchantId = merchantId.ToString(),
                        RedirectToActionUrl = "/MerchantList/SelectedMerchantList?catName=" + ServiceName,
                        MessageFromId = merchantId,
                        SID = Convert.ToInt32(ServiceId),
                        SenderType = "Merchant"
                    };
                    //StatusID	StatusName	StatusDescription	Usertype
                    //22  Approved Eligibility request has been approved.	Customer
                    //23  Rejected Eligibility request has been rejected.	Customer
                    string statusid = (status == "Rejected" ? "23" : "22");
                    var res = await _httpClient.PostAsync("Notifications/CreateNotification?StatusId=" + statusid, Customs.GetJsonContent(notification));
                    string resString = await res.Content.ReadAsStringAsync();
                    _logger.LogInformation("Notification Response : " + resString);
                }
                else
                {
                    TempData["FailMessage"] = jsonResponse?.message ?? "An error occurred.";
                    _logger.LogWarning("Failed to update eligibility request ID {RequestID}. Error: {Message}", updateDTO.RequestID, jsonResponse?.message);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating eligibility request ID {RequestID}.", requestId);
                TempData["FailMessage"] = "An unexpected error occurred. Please try again.";
            }

            return Ok("Success");
        }
    }

    public class EligibilityRequestDTO
    {
        public int CustomerID { get; set; }
        public int MerchantId { get; set; }
        public int ServiceID { get; set; }
        public bool IsRequestRejected { get; set; }
        public bool IsRespondedByMerchant { get; set; }
        public string? RequestDetails { get; set; } // JSON
        public string? requestorNationality { get; set; } // JSON
        public string? ReasonForRejection { get; set; } // JSON

        public int RequestID { get; set; } // Primary key

        public int StatusID { get; set; } // Foreign key to RequestStatuses
        public string Nationality { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        // Navigation properties
        public virtual Customers? Customer { get; set; }
        //public virtual Merchant Merchant { get; set; }
        public virtual Service? Service { get; set; }
        public virtual RequestStatuses? Status { get; set; }
    }
    public class ResponseModel
    {
        public int statusCode { get; set; }
        public string? message { get; set; }
        public object? data { get; set; }
    }
}
