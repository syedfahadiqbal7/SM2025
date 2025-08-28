using AFFZ_Admin.Models;
using AFFZ_Admin.Utils;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace AFFZ_Admin.Controllers
{
    public class Providers : Controller
    {
        private readonly HttpClient _httpClient;
        private string BaseUrl = string.Empty;
        private string PublicDomain = string.Empty;
        private string ApiHttpsPort = string.Empty;
        private string MerchantHttpsPort = string.Empty;
        private ILogger<Providers> _logger;
        public Providers(IHttpClientFactory httpClientFactory, IAppSettingsService service, ILogger<Providers> logger)
        {
            _httpClient = httpClientFactory.CreateClient("Main");
            BaseUrl = service.GetBaseIpAddress();
            PublicDomain = service.GetPublicDomain();
            ApiHttpsPort = service.GetApiHttpsPort();
            MerchantHttpsPort = service.GetMerchantHttpsPort();
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _httpClient.GetAsync("Providers");
                var responseString = await response.Content.ReadAsStringAsync();
                var providers = JsonConvert.DeserializeObject<List<ProviderUser>>(responseString);
                ViewBag.APILink = _httpClient.BaseAddress;
                ViewBag.MerchantLink = $"{Request.Scheme}://{PublicDomain}:{MerchantHttpsPort}/";
                return View("Providers", providers);
            }
            catch (Exception ex)
            {
                return View("Dashboard");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMerchantDocuments(int providerId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Providers/GetMerchantDocs/{providerId}");
                var responseString = await response.Content.ReadAsStringAsync();
                var documents = JsonConvert.DeserializeObject<List<MerchantDocuments>>(responseString);

                return Json(documents);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error fetching documents.");
            }
        }
        [HttpPost]
        public async Task<IActionResult> UpdateDocumentStatus([FromBody] DocumentStatusUpdateModel model)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("Providers/UpdateDocumentStatus", content);

                if (response.IsSuccessStatusCode)
                {
                    return Ok("Document status updated.");
                }

                return StatusCode(500, "Failed to update document status.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error updating document status.");
            }
        }
        [HttpGet]
        public IActionResult DownloadFile(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    return BadRequest("File path is required.");
                }

                // Decode the file path from the URL
                filePath = Uri.UnescapeDataString(filePath);

                // Ensure the file path uses the correct directory separator
                filePath = filePath.Replace('/', Path.DirectorySeparatorChar);

                // Combine the file path with the root directory of uploads
                var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads").Replace("AFFZ_Admin", "AFFZ_Provider");
                var fullPath = Path.Combine(uploadsRoot, Path.GetFileName(filePath));

                // Validate that the file exists within the uploads directory
                if (!fullPath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase) || !System.IO.File.Exists(fullPath))
                {
                    return NotFound("File not found or invalid path.");
                }

                // Get the file content and file name
                var fileContent = System.IO.File.ReadAllBytes(fullPath);
                var fileName = Path.GetFileName(fullPath);

                // Return the file with explicit Content-Disposition header
                return File(fileContent, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                // Log the exception (if needed) and return an error response
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        public async Task<IActionResult> ToggleStatus(int providerId, bool isActive)
        {
            try
            {
                var response = await _httpClient.PostAsync("Providers/ToggleMerchantStatus",
                    new StringContent(JsonConvert.SerializeObject(new { ProviderId = providerId, IsActive = isActive }), Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true });
                }

                return Json(new { success = false, error = await response.Content.ReadAsStringAsync() });
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error toggling merchant status for Provider ID: {ProviderId}", providerId);
                return Json(new { success = false, error = ex.Message });
            }
        }
        public async Task<IActionResult> MerchantServices(int MerchantId, int pageNumber = 1, int pageSize = 10)
        {
            _logger.LogInformation("MerchantServices called with pageNumber: {PageNumber}, pageSize: {PageSize}", pageNumber, pageSize); // Log method entry with parameters

            // Validate input parameters
            if (pageNumber <= 0)
            {
                _logger.LogWarning("Invalid page number: {PageNumber}", pageNumber); // Log a warning if page number is invalid
                ModelState.AddModelError("PageNumber", "Page number must be greater than 0."); // Add model error for invalid page number
                return View(new List<ServiceDTO>()); // Return view with an empty list
            }

            if (pageSize <= 0 || pageSize > 100)
            {
                _logger.LogWarning("Invalid page size: {PageSize}", pageSize); // Log a warning if page size is invalid
                ModelState.AddModelError("PageSize", "Page size must be between 1 and 100."); // Add model error for invalid page size
                return View(new List<ServiceDTO>()); // Return view with an empty list
            }

            // Retrieve Merchant ID from session
            _logger.LogInformation("Retrieving Merchant ID from session.");
            string _merchantId = MerchantId.ToString(); // Get encrypted Merchant ID from session
            if (string.IsNullOrEmpty(_merchantId))
            {
                _logger.LogWarning("Merchant ID not found in session."); // Log a warning if Merchant ID is not found
                return RedirectToAction("Index", "Login");
            }

            // Validate the Merchant ID
            _logger.LogInformation("Validating Merchant ID: {MerchantId}", _merchantId);
            if (!int.TryParse(_merchantId, out int id) || id <= 0)
            {
                _logger.LogWarning("Invalid Merchant ID: {MerchantId}", _merchantId); // Log a warning if Merchant ID is invalid
                return BadRequest("Invalid Merchant ID."); // Return BadRequest response
            }

            try
            {
                _logger.LogInformation("Fetching services from API for Merchant ID: {MerchantId}, PageNumber: {PageNumber}, PageSize: {PageSize}", id, pageNumber, pageSize);
                // Fetch services from the API
                var jsonResponse = await _httpClient.GetAsync($"Service/GetAllServices?pageNumber={pageNumber}&pageSize={pageSize}&merchantId={id}"); // Send GET request to fetch services
                if (!jsonResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("Error fetching services. Status Code: {StatusCode}", jsonResponse.StatusCode); // Log an error if the response is not successful
                    ModelState.AddModelError(string.Empty, "Failed to load services."); // Add model error for failed service load
                    return View(new List<ServiceDTO>()); // Return view with an empty list
                }

                // Read the response content as a string
                _logger.LogInformation("Reading response content from API.");
                var responseString = await jsonResponse.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(responseString))
                {
                    _logger.LogWarning("No response data received."); // Log a warning if the response data is empty
                    ViewBag.ServiceList = new List<ServiceDTO>(); // Set an empty service list in ViewBag
                }
                else
                {
                    _logger.LogInformation("Deserializing response data to list of Service objects.");
                    // Deserialize the response string to a list of Service objects
                    var categories = JsonConvert.DeserializeObject<List<ServiceDTO>>(responseString);
                    if (categories == null)
                    {
                        _logger.LogWarning("Failed to deserialize service data."); // Log a warning if deserialization fails
                        ViewBag.ServiceList = new List<ServiceDTO>(); // Set an empty service list in ViewBag
                    }
                    else
                    {
                        _logger.LogInformation("Filtering services by MerchantID: {MerchantId}", id);
                        // Filter the services by MerchantID and set in ViewBag
                        ViewBag.ServiceList = categories.Where(x => x.MerchantID == id).ToList();
                    }

                    // Try to get the total count of services from the response headers
                    if (jsonResponse.Headers.TryGetValues("X-Total-Count", out var totalCountValues) && int.TryParse(totalCountValues.FirstOrDefault(), out int totalCount))
                    {
                        _logger.LogInformation("Total count of services retrieved: {TotalCount}", totalCount);
                        ViewBag.TotalCount = totalCount; // Set total count in ViewBag
                    }
                    else
                    {
                        _logger.LogWarning("X-Total-Count header missing or invalid."); // Log a warning if the header is missing or invalid
                    }
                    // Fetch amount change requests from the API
                    _logger.LogInformation("Fetching amount change requests for Merchant ID: {MerchantId}", MerchantId);
                    var requestResponse = await _httpClient.GetAsync($"AmountChangeRequest/GetByMerchantId?merchantId={MerchantId}");
                    if (requestResponse.IsSuccessStatusCode)
                    {
                        var requestResponseString = await requestResponse.Content.ReadAsStringAsync();
                        var amountChangeRequests = JsonConvert.DeserializeObject<List<AmountChangeRequestDTO>>(requestResponseString);
                        ViewBag.AmountChangeRequests = amountChangeRequests ?? new List<AmountChangeRequestDTO>();
                    }
                    else
                    {
                        _logger.LogWarning("Failed to fetch amount change requests.");
                        ViewBag.AmountChangeRequests = new List<AmountChangeRequestDTO>();
                    }
                    // Set pagination details in ViewBag
                    _logger.LogInformation("Setting pagination details: PageNumber: {PageNumber}, PageSize: {PageSize}", pageNumber, pageSize);
                    ViewBag.PageNumber = pageNumber;
                    ViewBag.PageSize = pageSize;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error during HTTP request."); // Log an error if an HTTP request exception occurs
                ModelState.AddModelError(string.Empty, "Error fetching data from the server. Please try again later."); // Add model error for HTTP request error
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing JSON response."); // Log an error if JSON deserialization fails
                ModelState.AddModelError(string.Empty, "Error processing server response. Please contact support."); // Add model error for JSON deserialization error
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred."); // Log an error for any unexpected exceptions
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later."); // Add model error for unexpected errors
            }

            // Handle success message from TempData
            if (TempData.ContainsKey("SuccessMessage") && !string.IsNullOrEmpty(TempData["SuccessMessage"].ToString()))
            {
                _logger.LogInformation("Success message found in TempData: {SuccessMessage}", TempData["SuccessMessage"].ToString());
                ViewBag.SaveResponse = TempData["SuccessMessage"].ToString(); // Set success message in ViewBag
            }

            // Return the view with model errors if any
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is invalid."); // Log a warning if model state is invalid
                return View(new List<ServiceDTO>()); // Return view with an empty list
            }

            _logger.LogInformation("Returning view with populated data.");
            return View(); // Return the view with the populated data
        }
        [HttpPost]
        public async Task<IActionResult> MerchantServiceEdit(int serviceId, decimal AmountTopayToAdmin)
        {
            _logger.LogInformation("Updating Admin Commission for Service ID: {ServiceId}", serviceId);

            if (serviceId <= 0)
            {
                _logger.LogWarning("Invalid Service ID received.");
                return BadRequest("Invalid Service ID.");
            }
            try
            {
                // Update the Admin Commission
                var response = await _httpClient.GetAsync($"service/UpdateServiceAmountPaidToAdmin?id={serviceId}&ServiceAmountPaidToAdmin={AmountTopayToAdmin}"); // Send POST request to update the service
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Service update failed for service ID: {ServiceId}. Status code: {StatusCode}", serviceId, response.StatusCode); // Log error if service update fails
                    ModelState.AddModelError(string.Empty, "Failed to update service."); // Add error message to model state
                    return BadRequest(new { message = "Failed to update service." });// Return the view with the service model containing errors
                }

                _logger.LogInformation("Successfully updated Admin Commission for Service ID: {ServiceId}", serviceId);
                return Ok(new { message = "Admin Commission updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Admin Commission for Service ID: {ServiceId}", serviceId);
                return StatusCode(500, "An error occurred while updating the Admin Commission.");
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetMerchantMembershipName(int providerId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"MembershipPayment/CheckMemberShip?merchantId={providerId}");
                var responseString = await response.Content.ReadAsStringAsync();
                var documents = JsonConvert.DeserializeObject<MembershipPaymentHistory>(responseString);

                return Json(documents);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error fetching documents.");
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetRequestDetails(int requestId)
        {
            _logger.LogInformation("Fetching request details for Request ID: {RequestId}", requestId);

            if (requestId <= 0)
            {
                _logger.LogWarning("Invalid Request ID: {RequestId}", requestId);
                return BadRequest("Invalid Request ID.");
            }

            try
            {
                // Fetch request details from the API
                var response = await _httpClient.GetAsync($"AmountChangeRequest/GetByServiceId?requestId={requestId}");
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var requestDetails = JsonConvert.DeserializeObject<AmountChangeRequestDTO>(responseString);

                    // Return partial view with the request details
                    return PartialView("_RequestDetailsPartial", requestDetails);
                }
                else
                {
                    _logger.LogWarning("Failed to fetch request details. Status Code: {StatusCode}", response.StatusCode);
                    return StatusCode((int)response.StatusCode, "Failed to fetch request details.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while fetching request details.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
        [HttpPost]
        public async Task<IActionResult> ApproveRequest(int requestId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"AmountChangeRequest/Approve?requestId={requestId}&isApproved=true", null);
                if (response.IsSuccessStatusCode)
                {

                    return Ok("Request approved successfully.");
                }
                return StatusCode((int)response.StatusCode, "Failed to approve the request.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving the request.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> RejectRequest(int requestId, string reason)
        {
            try
            {
                var payload = new { RequestId = requestId, Reason = reason };
                var content = new StringContent(JsonConvert.SerializeObject(null), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"AmountChangeRequest/Reject?requestId={requestId}&reason={reason}", content);

                if (response.IsSuccessStatusCode)
                {
                    return Ok("Request rejected successfully.");
                }
                return StatusCode((int)response.StatusCode, "Failed to reject the request.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting the request.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

    }

    public class DocumentStatusUpdateModel
    {
        public int Id { get; set; }
        public string Status { get; set; }
    }
    public class AmountChangeRequestDTO
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public int ProviderId { get; set; }
        public decimal RequestedAmount { get; set; }
        public string? Status { get; set; }
        public DateTime? RequestedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime? RejectedAt { get; set; }
    }
}
