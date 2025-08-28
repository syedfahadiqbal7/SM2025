using AFFZ_Provider.Models;
using AFFZ_Provider.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;

namespace AFFZ_Provider.Controllers
{
    [Authorize]
    public class EligibilityController : Controller
    {
        private readonly ILogger<MerchantResponseToUser> _logger;
        private readonly IWebHostEnvironment _environment;
        private static string _merchantIdCat = string.Empty;
        private readonly HttpClient _httpClient;
        IDataProtector _protector;
        private string BaseUrl = string.Empty;
        private string PublicDomain = string.Empty;
        private string ApiHttpsPort = string.Empty;
        private string CustomerHttpsPort = string.Empty;

        public EligibilityController(ILogger<MerchantResponseToUser> logger, IWebHostEnvironment environment, IHttpClientFactory httpClientFactory, IDataProtectionProvider provider, IAppSettingsService service)
        {
            _httpClient = httpClientFactory.CreateClient("Main");
            _protector = provider.CreateProtector("Example.SessionProtection");
            _logger = logger;
            _environment = environment;
            BaseUrl = service.GetBaseIpAddress();
            PublicDomain = service.GetPublicDomain();
            ApiHttpsPort = service.GetApiHttpsPort();
            CustomerHttpsPort = service.GetCustomerHttpsPort();
        }

        public async Task<IActionResult> ManageRequests()
        {
            int merchantId = Convert.ToInt32(HttpContext.Session.GetEncryptedString("ProviderId", _protector));
            List<EligibilityRequestDTO> eligibilityRequestDTOs = new List<EligibilityRequestDTO>();
            try
            {
                var response = await _httpClient.GetAsync($"Eligibility/GetPendingRequests?merchantId={merchantId}");
                var responseString = await response.Content.ReadAsStringAsync();

                var jsonResponse = JsonConvert.DeserializeObject<ResponseModel>(responseString);

                if (response.StatusCode == HttpStatusCode.OK && jsonResponse?.data != null)
                {
                    // Deserialize the `data` property properly
                    eligibilityRequestDTOs = JsonConvert.DeserializeObject<List<EligibilityRequestDTO>>(jsonResponse.data.ToString());
                    foreach (var item in eligibilityRequestDTOs)
                    {
                        var GetService = await _httpClient.GetAsync($"ServicesList/GetServiceNameById?id=" + item.Service.SID);
                        item.Service.ServiceName = await GetService.Content.ReadAsStringAsync();
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


        [HttpPost("UpdateEligibilityRequest")]
        public async Task<IActionResult> UpdateEligibilityRequest(int requestId, string status, string? reasonForRejection, string customerId, string ServiceId, string ServiceName, string? Questions)
        {
            int merchantId = Convert.ToInt32(HttpContext.Session.GetEncryptedString("ProviderId", _protector));
            try
            {
                var updateDTO = new EligibilityUpdateDTO
                {
                    RequestID = requestId,
                    StatusID = (status == "Rejected") ? 23 : (status == "Accepted") ? 22 : 24,
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
                    //StatusID	StatusName	StatusDescription	Usertype
                    //22  Approved Eligibility request has been approved.	Customer
                    //23  Rejected Eligibility request has been rejected.	Customer
                    string statusid = (status == "Rejected" ? "23" : "22");
                    var notification = new Notification
                    {
                        UserId = customerId,
                        Message = $"Merchant[{merchantId.ToString()}] has responded for the Eligibility for Service[{ServiceId}].",
                        MerchantId = merchantId.ToString(),
                        RedirectToActionUrl = "/Eligibility/ManageRequests",//"/MerchantList/SelectedMerchantList?catName=" + ServiceName,
                        MessageFromId = merchantId,
                        SID = Convert.ToInt32(ServiceId),
                        SenderType = "Merchant"
                    };


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

    public class EligibilityUpdateDTO
    {
        public int RequestID { get; set; }
        public int StatusID { get; set; } // Approved, Rejected
        public string ReasonForRejection { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsRequestRejected { get; set; }
        public bool IsRespondedByMerchant { get; set; }
        public string RequestDetails { get; set; }
    }
    public class EligibilityRequestDTO
    {
        public int RequestID { get; set; } // Primary key
        public int CustomerID { get; set; } // Foreign key to Customers
        public int ServiceID { get; set; } // Foreign key to Service
        public int MerchantID { get; set; } // Foreign key to ProviderUser
        public int StatusID { get; set; } // Foreign key to RequestStatuses
        public string RequestDetails { get; set; }
        public string Nationality { get; set; }
        public string ReasonForRejection { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsRequestRejected { get; set; }
        public bool IsRespondedByMerchant { get; set; }

        // Navigation properties
        public virtual Customers Customer { get; set; }
        public virtual Merchant Merchant { get; set; }
        public virtual Service Service { get; set; }
        public virtual RequestStatus Status { get; set; }
    }
}
