using AFFZ_Provider.Models;
using AFFZ_Provider.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;


namespace AFFZ_Provider.Controllers
{
    [Authorize]
    public class MerchantServiceController : Controller
    {
        private readonly ILogger<MerchantServiceController> _logger;
        private readonly IWebHostEnvironment _environment;
        private static string _merchantIdCat = string.Empty;
        private readonly HttpClient _httpClient;
        private readonly IDataProtector _protector;

        // Constructor to initialize dependencies
        public MerchantServiceController(ILogger<MerchantServiceController> logger, IWebHostEnvironment environment, IHttpClientFactory httpClientFactory, IDataProtectionProvider provider)
        {
            _httpClient = httpClientFactory.CreateClient("Main"); // Create an HttpClient instance using a named client "Main"
            _protector = provider.CreateProtector("Example.SessionProtection"); // Create a data protector for session protection
            _logger = logger;
            _environment = environment;
        }
        public async Task<IActionResult> MerchantServiceIndex(int pageNumber = 1, int pageSize = 10)
        {
            _logger.LogInformation("MerchantServiceIndex called with pageNumber: {PageNumber}, pageSize: {PageSize}", pageNumber, pageSize); // Log method entry with parameters

            // Validate input parameters
            if (pageNumber <= 0)
            {
                _logger.LogWarning("Invalid page number: {PageNumber}", pageNumber); // Log a warning if page number is invalid
                ModelState.AddModelError("PageNumber", "Page number must be greater than 0."); // Add model error for invalid page number
                return View(new List<Service>()); // Return view with an empty list
            }

            if (pageSize <= 0 || pageSize > 100)
            {
                _logger.LogWarning("Invalid page size: {PageSize}", pageSize); // Log a warning if page size is invalid
                ModelState.AddModelError("PageSize", "Page size must be between 1 and 100."); // Add model error for invalid page size
                return View(new List<Service>()); // Return view with an empty list
            }

            // Retrieve Merchant ID from session
            _logger.LogInformation("Retrieving Merchant ID from session.");
            string _merchantId = HttpContext.Session.GetEncryptedString("ProviderId", _protector); // Get encrypted Merchant ID from session
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
                    return View(new List<Service>()); // Return view with an empty list
                }

                // Read the response content as a string
                _logger.LogInformation("Reading response content from API.");
                var responseString = await jsonResponse.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(responseString))
                {
                    _logger.LogWarning("No response data received."); // Log a warning if the response data is empty
                    ViewBag.ServiceList = new List<Service>(); // Set an empty service list in ViewBag
                }
                else
                {
                    _logger.LogInformation("Deserializing response data to list of Service objects.");
                    // Deserialize the response string to a list of Service objects
                    var categories = JsonConvert.DeserializeObject<List<Service>>(responseString);
                    if (categories == null)
                    {
                        _logger.LogWarning("Failed to deserialize service data."); // Log a warning if deserialization fails
                        ViewBag.ServiceList = new List<Service>(); // Set an empty service list in ViewBag
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
                return View(new List<Service>()); // Return view with an empty list
            }
            //Check If merchant has any Membership


            _logger.LogInformation("Returning view with populated data.");
            return View(); // Return the view with the populated data
        }

        // Method to view details of a specific service by ID
        [HttpGet]
        public async Task<IActionResult> ViewService(int id)
        {
            _logger.LogInformation("ViewService called with ID: {Id}", id); // Log method entry with ID parameter

            // Validate input parameter
            if (id <= 0)
            {
                _logger.LogWarning("Invalid service ID: {Id}", id); // Log a warning if service ID is invalid
                return BadRequest("Invalid service ID."); // Return BadRequest response
            }

            try
            {
                _logger.LogInformation("Fetching service details from API for ID: {Id}", id); // Log fetching details from API
                var response = await _httpClient.GetAsync($"Service/GetServiceById?id={id}"); // Send GET request to fetch service by ID
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to fetch service. Status code: {StatusCode}", response.StatusCode); // Log an error if the response is not successful
                    ModelState.AddModelError(string.Empty, "Failed to load service details."); // Add model error for failed service load
                    return PartialView("_ServiceDetails", new Service()); // Return partial view with an empty Service object
                }

                _logger.LogInformation("Reading response content from API for service ID: {Id}", id); // Log reading response content
                var responseString = await response.Content.ReadAsStringAsync(); // Read the response content as a string
                if (string.IsNullOrEmpty(responseString))
                {
                    _logger.LogWarning("No response data received for service ID: {Id}", id); // Log a warning if the response data is empty
                    ModelState.AddModelError(string.Empty, "No data available for the requested service."); // Add model error for empty response
                    return PartialView("_ServiceDetails", new Service()); // Return partial view with an empty Service object
                }

                _logger.LogInformation("Deserializing response data for service ID: {Id}", id); // Log deserializing response data
                var service = JsonConvert.DeserializeObject<Service>(responseString); // Deserialize response to Service object
                if (service == null)
                {
                    _logger.LogWarning("Failed to deserialize service data for ID: {Id}", id); // Log a warning if deserialization fails
                    ModelState.AddModelError(string.Empty, "Error processing service details."); // Add model error for deserialization error
                    return PartialView("_ServiceDetails", new Service()); // Return partial view with an empty Service object
                }
                //ServiceNameById

                response = await _httpClient.GetAsync($"Service/ServiceNameById?id={id}"); // Send GET request to fetch service by ID
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to fetch service. Status code: {StatusCode}", response.StatusCode); // Log an error if the response is not successful
                    ModelState.AddModelError(string.Empty, "Failed to load service details."); // Add model error for failed service load
                    return PartialView("_ServiceDetails", new Service()); // Return partial view with an empty Service object
                }

                responseString = await response.Content.ReadAsStringAsync();
                service.ServiceName = responseString;
                _logger.LogInformation("Successfully fetched service details for ID: {Id}", id); // Log success
                return PartialView("_ServiceDetails", service); // Return partial view with the Service object
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error during HTTP request for service ID: {Id}", id); // Log an error if an HTTP request exception occurs
                ModelState.AddModelError(string.Empty, "Error fetching data from the server. Please try again later."); // Add model error for HTTP request error
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing JSON response for service ID: {Id}", id); // Log an error if JSON deserialization fails
                ModelState.AddModelError(string.Empty, "Error processing server response. Please contact support."); // Add model error for JSON deserialization error
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while fetching service details for ID: {Id}", id); // Log an error for any unexpected exceptions
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later."); // Add model error for unexpected errors
            }

            _logger.LogWarning("Returning partial view with empty Service object due to errors for service ID: {Id}", id); // Log returning view with empty Service object
            return PartialView("_ServiceDetails", new Service()); // Return partial view with an empty Service object
        }


        // Method to create a new merchant service
        [HttpGet]
        public async Task<IActionResult> MerchantServiceCreate(string SID = "0", int CategoryID = 0)
        {
            _logger.LogInformation("MerchantServiceCreate called with SID: {SID}, CategoryID: {CategoryID}", SID, CategoryID); // Log method entry with parameters

            try
            {

                // Fetch dropdown data for the view
                _logger.LogInformation("Fetching ServiceListComboData.");
                ViewBag.ServiceListComboData = await ServiceListItems(Convert.ToInt32(SID)); // Fetch service list items for dropdown

                _logger.LogInformation("Fetching ServiceCategoryComboData.");
                ViewBag.ServiceCategoryComboData = await ServiceCategoryItems(CategoryID); // Fetch service category items for dropdown

                List<int> defaultDocsList = new List<int>();
                if (!string.IsNullOrEmpty(SID))
                {
                    _logger.LogInformation("Fetching Service ID for SID: {SID}", SID); // Log fetching Service ID
                    int sid = await GetServiceId(Convert.ToInt32(SID)); // Get service ID based on SID

                    _logger.LogInformation("Fetching Default Service Document List Data for Service ID: {ServiceID}", sid); // Log fetching default document list
                    defaultDocsList = await GetDefaultServiceDocumentListData(sid); // Get default service document list
                }

                _logger.LogInformation("Fetching DocumentListComboData.");
                ViewBag.DocumentListComboData = await DocumentListItems(defaultDocsList); // Fetch document list items for dropdown

                _logger.LogInformation("Returning view for MerchantServiceCreate.");
                var pSlab = await GetProviderSlab();
                ViewBag.ProviderSlabSelectedModel = pSlab.SelectedModel;
                return View();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error during HTTP request while initializing service creation page."); // Log an error if an HTTP request exception occurs
                ModelState.AddModelError(string.Empty, "Error fetching data from the server. Please try again later."); // Add model error for HTTP request error
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while initializing service creation page."); // Log an error for any unexpected exceptions
                ModelState.AddModelError(string.Empty, "Failed to load data."); // Add model error for unexpected errors
            }

            _logger.LogWarning("Returning view with errors for MerchantServiceCreate.");
            return View(); // Return the view with errors if any
        }
        // Method to get document list items for dropdown
        public async Task<List<SelectListItem>> DocumentListItems(List<int> selectedDocumentIds)
        {
            _logger.LogInformation("DocumentListItems called with selectedDocumentIds: {SelectedDocumentIds}", string.Join(",", selectedDocumentIds)); // Log method entry with parameters

            // Validate input parameter
            if (selectedDocumentIds == null)
            {
                _logger.LogWarning("SelectedDocumentIds is null."); // Log a warning if selectedDocumentIds is null
                selectedDocumentIds = new List<int>(); // Initialize selectedDocumentIds to an empty list
            }

            var documentList = new List<SelectListItem>(); // Initialize an empty list for the documents

            try
            {
                _logger.LogInformation("Fetching all documents from API."); // Log fetching documents from API
                var jsonResponse = await _httpClient.GetAsync("ServiceDocumenList/GetAllDocuments"); // Send GET request to fetch all documents
                if (!jsonResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to fetch documents. Status code: {StatusCode}", jsonResponse.StatusCode); // Log an error if the response is not successful
                    return documentList; // Return an empty document list
                }

                var responseString = await jsonResponse.Content.ReadAsStringAsync(); // Read the response content as a string
                if (string.IsNullOrEmpty(responseString))
                {
                    _logger.LogWarning("No response data received for documents."); // Log a warning if the response data is empty
                    return documentList; // Return an empty document list
                }

                _logger.LogInformation("Deserializing response data for documents."); // Log deserializing response data
                var documents = JsonConvert.DeserializeObject<List<ServiceDocumenList>>(responseString); // Deserialize response to list of ServiceDocumenList
                if (documents == null)
                {
                    _logger.LogWarning("Failed to deserialize document data."); // Log a warning if deserialization fails
                    return documentList; // Return an empty document list
                }

                // Map each document to a SelectListItem
                _logger.LogInformation("Mapping documents to SelectListItem.");
                documentList = documents.Select(doc => new SelectListItem
                {
                    Text = doc.ServiceDocumentName,
                    Value = doc.ServiceDocumenListtId.ToString(),
                    Selected = selectedDocumentIds.Contains(doc.ServiceDocumenListtId) // Set selected based on IDs
                }).ToList();

                _logger.LogInformation("Successfully mapped {Count} documents.", documentList.Count); // Log success
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error during HTTP request while fetching document list."); // Log an error if an HTTP request exception occurs
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing JSON response for documents."); // Log an error if JSON deserialization fails
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while fetching document list."); // Log an error for any unexpected exceptions
            }

            return documentList; // Return the list of SelectListItem objects
        }
        // POST: Service/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MerchantServiceCreate([Bind("CategoryID,SID,Description,ServicePrice,ServiceAmountPaidToAdmin, SelectedDocumentIds,DeductionType,DeductionValue,Eligibility")] Service service, List<int> selectedDocumentIds)
        {
            try
            {
                //service.SID = Convert.ToInt32(service.ServiceName);
                _logger.LogInformation("MerchantServiceCreate POST called.");

                // Validate session Merchant ID
                var merchantId = HttpContext.Session.GetEncryptedString("ProviderId", _protector);
                if (string.IsNullOrEmpty(merchantId))
                {
                    _logger.LogWarning("Merchant ID not found in session. Redirecting to login.");
                    return RedirectToAction("Login", "Account"); // Redirect to login if Merchant ID is not found in session
                }
                if (service.ServicePrice < service.ServiceAmountPaidToAdmin)
                {

                }
                // Assign Merchant ID to service
                service.MerchantID = Convert.ToInt32(merchantId); // Convert Merchant ID to integer and assign to the service object
                service.SelectedDocumentIds = string.Join(",", selectedDocumentIds.Select(i => i.ToString()).ToArray()); // Convert selected document IDs to a comma-separated string
                service.RequiresEligibility = service.Eligibility ? 1 : 0;
                // Perform deduction calculation on the server as well for consistency
                decimal deductionAmount = service.DeductionType == "Percentage" ? Convert.ToDecimal(service.ServicePrice * service.DeductionValue) / 100 : service.DeductionValue;

                service.ServiceAmountPaidToAdmin = Convert.ToInt32(service.ServicePrice - deductionAmount);
                // Validate model state
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid service model."); // Log warning if model state is invalid
                    ViewBag.ServiceListComboData = await ServiceListItems(0); // Populate dropdowns for the view in case of invalid model state
                    ViewBag.ServiceCategoryComboData = await ServiceCategoryItems(0);
                    ViewBag.DocumentListComboData = await DocumentListItems(new List<int>());
                    return View(service); // Return the view with the service model containing errors
                }

                // Create the service via API
                _logger.LogInformation("Sending service creation request to API.");
                var jsonResponse = await _httpClient.PostAsync("Service/CreateService", Customs.GetJsonContent(service)); // Send POST request to create the service
                if (!jsonResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("Service creation failed. Status code: {StatusCode}", jsonResponse.StatusCode); // Log error if service creation fails
                    ModelState.AddModelError(string.Empty, await jsonResponse.Content.ReadAsStringAsync()); // Add error message to model state
                    ViewBag.ServiceListComboData = await ServiceListItems(0); // Populate dropdowns for the view
                    ViewBag.ServiceCategoryComboData = await ServiceCategoryItems(0);
                    ViewBag.DocumentListComboData = await DocumentListItems(new List<int>());
                    return View(service); // Return the view with the service model containing errors
                }

                // Parse response and set ServiceId
                var responseString = await jsonResponse.Content.ReadAsStringAsync();
                var createdService = JsonConvert.DeserializeObject<dynamic>(responseString); // Deserialize the response to get the created service ID
                service.ServiceId = (int)createdService["serviceId"]; // Set the ServiceId from the response

                // Save document mappings if selected
                if (selectedDocumentIds != null && selectedDocumentIds.Count > 0)
                {
                    _logger.LogInformation("Creating document mappings for service ID: {ServiceId}", service.ServiceId); // Log creation of document mappings
                    List<ServiceDocumentMapping> mappings = selectedDocumentIds.Select(docId => new ServiceDocumentMapping
                    {
                        ServiceID = service.ServiceId,
                        ServiceDocumentListId = docId
                    }).ToList();

                    var mappingsResponse = await _httpClient.PostAsync("ServiceDocumentMapping/CreateMapping", Customs.GetJsonContent(mappings)); // Send POST request to create document mappings
                    if (!mappingsResponse.IsSuccessStatusCode)
                    {
                        _logger.LogError("Failed to create document mappings for ServiceId: {ServiceId}", service.ServiceId); // Log error if document mappings creation fails
                    }

                    TempData["SuccessResponse"] = await mappingsResponse.Content.ReadAsStringAsync(); // Store success response in TempData
                }

                _logger.LogInformation("Service created successfully with ID: {ServiceId}", service.ServiceId); // Log successful service creation
                return RedirectToAction(nameof(MerchantServiceIndex)); // Redirect to the MerchantServiceIndex page
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error occurred during service creation."); // Log HTTP request exception
                ModelState.AddModelError(string.Empty, "Error communicating with the server. Please try again later."); // Add model error for communication issues
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred during service creation."); // Log any other exceptions
                ModelState.AddModelError(string.Empty, "Failed to create service."); // Add generic error message to model state
            }

            // Return view with errors if any
            ViewBag.ServiceListComboData = await ServiceListItems(0); // Populate dropdowns for the view
            ViewBag.ServiceCategoryComboData = await ServiceCategoryItems(0);
            ViewBag.DocumentListComboData = await DocumentListItems(new List<int>());
            return View(service); // Return the view with the service model containing errors
        }

        // GET: Service/Edit/{id}
        public async Task<IActionResult> MerchantServiceEdit(int id)
        {
            _logger.LogInformation("MerchantServiceEdit called with ID: {Id}", id); // Log method entry with the service ID
            string _merchantId = HttpContext.Session.GetEncryptedString("ProviderId", _protector); // Get encrypted Merchant ID from session
            // Validate input parameter
            if (id <= 0)
            {
                _logger.LogWarning("Invalid service ID: {Id}", id); // Log warning if the service ID is invalid
                ModelState.AddModelError(string.Empty, "Invalid service ID."); // Add error message for invalid service ID
                return View("Error", new ErrorViewModel { RequestId = "Invalid service ID." }); // Return error view
            }

            try
            {
                // Fetch service details from API
                _logger.LogInformation("Fetching service details for ID: {Id}", id); // Log fetching service details
                var response = await _httpClient.GetAsync($"service/GetServiceById?id={id}"); // Send GET request to fetch service details
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to fetch service. Status code: {StatusCode}", response.StatusCode); // Log error if fetching service details fails
                    return View("Error", new ErrorViewModel { RequestId = "Error fetching service for edit." }); // Return error view
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                var service = JsonConvert.DeserializeObject<Service>(jsonString); // Deserialize response to Service object
                if (service == null)
                {
                    _logger.LogWarning("Failed to deserialize service data for ID: {Id}", id); // Log warning if deserialization fails
                    return View("Error", new ErrorViewModel { RequestId = "Error processing service data." }); // Return error view
                }
                // Check if an amount change request exists for this service
                _logger.LogInformation("Checking if an amount change request exists for service ID: {Id}", id);
                var requestResponse = await _httpClient.GetAsync($"AmountChangeRequest/AmountChangeRequestExist?serviceId={id}&providerId={_merchantId}"); // Replace with your actual API endpoint
                if (requestResponse.IsSuccessStatusCode)
                {
                    var requestExists = JsonConvert.DeserializeObject<bool>(await requestResponse.Content.ReadAsStringAsync());
                    ViewBag.AmountChangeRequested = requestExists;
                }
                else
                {
                    _logger.LogWarning("Failed to fetch amount change request status for service ID: {Id}");
                    ViewBag.AmountChangeRequested = false; // Default to false if API call fails
                }
                // Fetch document mappings for the service
                _logger.LogInformation("Fetching document mappings for service ID: {Id}", id); // Log fetching document mappings
                var mappingsResponse = await _httpClient.GetAsync($"ServiceDocumentMapping/GetMappingsByService?id={id}"); // Send GET request to fetch document mappings
                List<int> selectedDocumentIds = new List<int>();

                if (mappingsResponse.IsSuccessStatusCode)
                {
                    var mappingsString = await mappingsResponse.Content.ReadAsStringAsync();
                    var serviceDocumentMappings = JsonConvert.DeserializeObject<List<ServiceDocumentMapping>>(mappingsString); // Deserialize response to list of ServiceDocumentMapping
                    if (serviceDocumentMappings != null)
                    {
                        selectedDocumentIds = serviceDocumentMappings.Select(m => m.ServiceDocumentListId).ToList(); // Extract document IDs from the mappings
                    }
                }
                else
                {
                    _logger.LogWarning("Failed to fetch document mappings for service ID: {Id}", id); // Log warning if fetching document mappings fails
                }

                // Populate the document list for selection and pass the selected IDs
                ViewBag.DocumentListComboData = await DocumentListItems(selectedDocumentIds); // Populate document list dropdown
                ViewBag.ServiceListComboData = await ServiceListItems(service.SID); // Populate service list dropdown
                ViewBag.ServiceCategoryComboData = await ServiceCategoryItems(service.CategoryID); // Populate category list dropdown

                _logger.LogInformation("Returning view for editing service ID: {Id}", id); // Log returning the edit view
                service.Eligibility = service.RequiresEligibility == 1 ? true : false;
                var pSlab = await GetProviderSlab();
                ViewBag.ProviderSlabSelectedModel = pSlab.SelectedModel;

                //ViewBag.AmountChangeRequested
                return View(service); // Return the view with the service model
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error occurred while fetching service for edit."); // Log HTTP request exception
                ModelState.AddModelError(string.Empty, "Error communicating with the server. Please try again later."); // Add model error for communication issues
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing JSON response for service ID: {Id}", id); // Log JSON deserialization exception
                ModelState.AddModelError(string.Empty, "Error processing server response. Please contact support."); // Add model error for JSON deserialization issues
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while fetching service for edit."); // Log any other exceptions
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later."); // Add generic error message to model state
            }

            _logger.LogWarning("Returning view with errors for editing service ID: {Id}", id); // Log returning the view with errors
            return View(new Service()); // Return the view with an empty service model containing errors
        }
        // POST: Service/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MerchantServiceEdit(int id, [Bind("ServiceId,CategoryID,SID,Description,ServicePrice,ServiceAmountPaidToAdmin,DeductionType,DeductionValue,Eligibility")] Service service, List<int> selectedDocumentIds)
        {
            _logger.LogInformation("MerchantServiceEdit POST called with ID: {Id}", id); // Log method entry with the service ID

            // Validate input parameters
            if (id != service.ServiceId)
            {
                _logger.LogWarning("Service ID in URL does not match Service ID in the model. ID: {Id}, ServiceId: {ServiceId}", id, service.ServiceId); // Log warning if IDs do not match
                return BadRequest("Service ID mismatch."); // Return BadRequest response
            }

            var merchantId = HttpContext.Session.GetEncryptedString("ProviderId", _protector); // Get Merchant ID from session
            if (string.IsNullOrEmpty(merchantId))
            {
                _logger.LogWarning("Merchant ID not found in session. Redirecting to login."); // Log warning if Merchant ID is not found
                return RedirectToAction("Login", "Account"); // Redirect to login if Merchant ID is not found in session
            }

            try
            {
                // Assign Merchant ID to service
                service.MerchantID = Convert.ToInt32(merchantId); // Convert Merchant ID to integer and assign to the service object
                service.SelectedDocumentIds = string.Join(",", selectedDocumentIds.Select(i => i.ToString()).ToArray()); // Convert selected document IDs to a comma-separated string
                service.RequiresEligibility = service.Eligibility ? 1 : 0;
                decimal deductionAmount = service.DeductionType == "Percentage" ? Convert.ToDecimal(service.ServicePrice * service.DeductionValue) / 100 : service.DeductionValue;

                service.ServiceAmountPaidToAdmin = Convert.ToInt32(service.ServicePrice - deductionAmount);
                // Validate model state
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid service model for service ID: {ServiceId}", service.ServiceId); // Log warning if model state is invalid
                    return View(service); // Return the view with the service model containing errors
                }
                // Perform deduction calculation on the server as well for consistency

                // Update the service via API
                _logger.LogInformation("Sending service update request to API for service ID: {ServiceId}", service.ServiceId); // Log service update request
                var response = await _httpClient.PostAsync($"service/UpdateService?id={id}", Customs.GetJsonContent(service)); // Send POST request to update the service
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Service update failed for service ID: {ServiceId}. Status code: {StatusCode}", service.ServiceId, response.StatusCode); // Log error if service update fails
                    ModelState.AddModelError(string.Empty, "Failed to update service."); // Add error message to model state
                    return View(service); // Return the view with the service model containing errors
                }

                // Update document mappings if selected
                if (selectedDocumentIds != null && selectedDocumentIds.Count > 0)
                {
                    // Delete old mappings
                    _logger.LogInformation("Deleting old document mappings for service ID: {ServiceId}", service.ServiceId); // Log deletion of old mappings
                    var deleteResponse = await _httpClient.GetAsync($"ServiceDocumentMapping/DeleteMappingsByServiceId?Id={id}"); // Send GET request to delete old mappings
                    if (!deleteResponse.IsSuccessStatusCode)
                    {
                        _logger.LogError("Failed to delete old document mappings for service ID: {ServiceId}. Status code: {StatusCode}", service.ServiceId, deleteResponse.StatusCode); // Log error if deletion fails
                    }

                    // Add new mappings
                    _logger.LogInformation("Creating new document mappings for service ID: {ServiceId}", service.ServiceId); // Log creation of new mappings
                    List<ServiceDocumentMapping> mappings = selectedDocumentIds.Select(docId => new ServiceDocumentMapping
                    {
                        ServiceID = service.ServiceId,
                        ServiceDocumentListId = docId
                    }).ToList();

                    var mappingsResponse = await _httpClient.PostAsync("ServiceDocumentMapping/CreateMapping", Customs.GetJsonContent(mappings)); // Send POST request to create new mappings
                    if (!mappingsResponse.IsSuccessStatusCode)
                    {
                        _logger.LogError("Failed to create document mappings for service ID: {ServiceId}. Status code: {StatusCode}", service.ServiceId, mappingsResponse.StatusCode); // Log error if creation of new mappings fails
                    }
                }

                _logger.LogInformation("Service updated successfully for service ID: {ServiceId}", service.ServiceId); // Log successful service update
                return RedirectToAction(nameof(MerchantServiceIndex)); // Redirect to the MerchantServiceIndex page
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error occurred while updating service ID: {ServiceId}", service.ServiceId); // Log HTTP request exception
                ModelState.AddModelError(string.Empty, "Error communicating with the server. Please try again later."); // Add model error for communication issues
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while updating service ID: {ServiceId}", service.ServiceId); // Log any other exceptions
                ModelState.AddModelError(string.Empty, "Failed to update service."); // Add generic error message to model state
            }

            return View(service); // Return the view with the service model containing errors
        }

        // GET: Service/Delete/{id}
        public async Task<IActionResult> MerchantServiceDelete(int id)
        {
            _logger.LogInformation("MerchantServiceDelete called with ID: {Id}", id); // Log method entry with the service ID

            // Validate input parameter
            if (id <= 0)
            {
                _logger.LogWarning("Invalid service ID: {Id}", id); // Log warning if the service ID is invalid
                ModelState.AddModelError(string.Empty, "Invalid service ID."); // Add error message for invalid service ID
                return View("Error", new ErrorViewModel { RequestId = "Invalid service ID." }); // Return error view
            }

            try
            {
                // Fetch service details from API
                _logger.LogInformation("Fetching service details for ID: {Id}", id); // Log fetching service details
                var response = await _httpClient.GetAsync($"service/GetServiceById?id={id}"); // Send GET request to fetch service details
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to fetch service for deletion. Status code: {StatusCode}", response.StatusCode); // Log error if fetching service details fails
                    return View("Error", new ErrorViewModel { RequestId = "Error fetching service for delete." }); // Return error view
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                var service = JsonConvert.DeserializeObject<Service>(jsonString); // Deserialize response to Service object
                if (service == null)
                {
                    _logger.LogWarning("Failed to deserialize service data for ID: {Id}", id); // Log warning if deserialization fails
                    return View("Error", new ErrorViewModel { RequestId = "Error processing service data." }); // Return error view
                }

                // Fetch related document mappings if needed
                _logger.LogInformation("Fetching document mappings for service ID: {Id}", id); // Log fetching document mappings
                var mappingsResponse = await _httpClient.GetAsync($"ServiceDocumentMapping/GetMappingsByServiceId?id={id}"); // Send GET request to fetch document mappings
                if (mappingsResponse.IsSuccessStatusCode)
                {
                    var mappingsString = await mappingsResponse.Content.ReadAsStringAsync();
                    var serviceDocumentMappings = JsonConvert.DeserializeObject<List<ServiceDocumentMapping>>(mappingsString); // Deserialize response to list of ServiceDocumentMapping
                    if (serviceDocumentMappings != null)
                    {
                        ViewBag.ServiceDocumentMappings = serviceDocumentMappings; // Pass the document mappings to the view
                    }
                }
                else
                {
                    _logger.LogWarning("Failed to fetch document mappings for service ID: {Id}", id); // Log warning if fetching document mappings fails
                }

                _logger.LogInformation("Returning view for deleting service ID: {Id}", id); // Log returning the delete view
                return View(service); // Return the view with the service model
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error occurred while fetching service for delete. Service ID: {Id}", id); // Log HTTP request exception
                ModelState.AddModelError(string.Empty, "Error communicating with the server. Please try again later."); // Add model error for communication issues
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing JSON response for service ID: {Id}", id); // Log JSON deserialization exception
                ModelState.AddModelError(string.Empty, "Error processing server response. Please contact support."); // Add model error for JSON deserialization issues
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while fetching service for delete. Service ID: {Id}", id); // Log any other exceptions
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later."); // Add generic error message to model state
            }

            _logger.LogWarning("Returning error view for deleting service ID: {Id}", id); // Log returning the error view
            return View("Error", new ErrorViewModel { RequestId = "Error occurred during delete operation." }); // Return error view
        }


        // Get: Service/DeleteConfirmed/{id}
        [HttpGet]
        public async Task<IActionResult> MerchantServiceDeleteConfirmed(int id)
        {
            _logger.LogInformation("MerchantServiceDeleteConfirmed called with ID: {Id}", id); // Log method entry with service ID

            // Validate input parameter
            if (id <= 0)
            {
                _logger.LogWarning("Invalid service ID: {Id}", id); // Log warning if service ID is invalid
                ModelState.AddModelError(string.Empty, "Invalid service ID."); // Add model error for invalid service ID
                return View("Error", new ErrorViewModel { RequestId = "Invalid service ID." }); // Return error view
            }

            try
            {
                // Delete the service document mappings
                _logger.LogInformation("Deleting document mappings for service ID: {Id}", id); // Log deletion of document mappings
                var mappingsResponse = await _httpClient.GetAsync($"ServiceDocumentMapping/DeleteMappingsByServiceId?Id={id}"); // Send GET request to delete document mappings
                if (!mappingsResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to delete document mappings for service ID: {Id}. Status code: {StatusCode}", id, mappingsResponse.StatusCode); // Log error if deletion fails
                    return View("Error", new ErrorViewModel { RequestId = "Error deleting service document mappings." }); // Return error view
                }

                // Delete the service
                _logger.LogInformation("Deleting service with ID: {Id}", id); // Log deletion of the service
                var response = await _httpClient.GetAsync($"service/DeleteService?id={id}"); // Send GET request to delete the service
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to delete service with ID: {Id}. Status code: {StatusCode}", id, response.StatusCode); // Log error if deletion fails
                    return View("Error", new ErrorViewModel { RequestId = "Error deleting service." }); // Return error view
                }

                _logger.LogInformation("Service deleted successfully with ID: {Id}", id); // Log successful deletion
                return RedirectToAction(nameof(MerchantServiceIndex)); // Redirect to MerchantServiceIndex after successful deletion
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error occurred while deleting service ID: {Id}", id); // Log HTTP request exception
                ModelState.AddModelError(string.Empty, "Error communicating with the server. Please try again later."); // Add model error for communication issues
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while deleting service ID: {Id}", id); // Log any other exceptions
                ModelState.AddModelError(string.Empty, "Error occurred while deleting the service."); // Add generic error message to model state
            }

            return View("Error", new ErrorViewModel { RequestId = "Error occurred while deleting the service." }); // Return error view in case of exception
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

        // Helper method to get service ID by service name
        public async Task<int> GetServiceId(int servicename)
        {
            _logger.LogInformation("GetServiceId called with service name: {ServiceName}", servicename); // Log method entry with service name
            int serviceId = 0;

            try
            {
                var jsonResponse = await _httpClient.GetAsync("ServicesList/GetServicesList"); // Send GET request to get list of services
                if (!jsonResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to fetch services list. Status code: {StatusCode}", jsonResponse.StatusCode); // Log error if fetching services list fails
                    return 0; // Return 0 in case of failure
                }

                var responseString = await jsonResponse.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(responseString))
                {
                    List<ServicesList> serviceList = JsonConvert.DeserializeObject<List<ServicesList>>(responseString); // Deserialize response to list of ServicesList
                    serviceId = serviceList.Where(x => x.ServiceListID == servicename).Select(x => x.ServiceListID).FirstOrDefault(); // Find service ID by name
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

            return serviceId; // Return service ID
        }

        // Helper method to get service category items for dropdown
        public async Task<List<SelectListItem>> ServiceCategoryItems(int? id)
        {
            _logger.LogInformation("ServiceCategoryItems called with category ID: {CategoryId}", id); // Log method entry with category ID

            try
            {
                var jsonResponse = await _httpClient.GetAsync("ServiceCategory/GetAllServiceCategories"); // Send GET request to get list of service categories
                if (!jsonResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to fetch service categories. Status code: {StatusCode}", jsonResponse.StatusCode); // Log error if fetching service categories fails
                    return new List<SelectListItem>(); // Return empty list in case of failure
                }

                var responseString = await jsonResponse.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(responseString))
                {
                    List<ServiceCategory> categoryList = JsonConvert.DeserializeObject<List<ServiceCategory>>(responseString); // Deserialize response to list of ServiceCategory
                    return categoryList.Select(i => new SelectListItem
                    {
                        Text = i.CategoryName,
                        Value = i.CategoryId.ToString(),
                        Selected = id == i.CategoryId // Set selected item based on input parameter
                    }).ToList();
                }
            }
            catch (JsonSerializationException ex)
            {
                _logger.LogError(ex, "JSON deserialization error occurred while fetching service categories."); // Log JSON deserialization exception
                ModelState.AddModelError(string.Empty, "Failed to load Data."); // Add model error for deserialization issues
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error occurred while fetching service categories."); // Log HTTP request exception
                ModelState.AddModelError(string.Empty, "Error communicating with the server. Please try again later."); // Add model error for communication issues
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while fetching service categories."); // Log any other exceptions
                ModelState.AddModelError(string.Empty, "Failed to load Data."); // Add generic error message to model state
            }

            return new List<SelectListItem>(); // Return empty list in case of exception
        }
        // Method to get document list items for dropdown
        public async Task<List<SelectListItem>> GetDocumentListItems()
        {
            _logger.LogInformation("GetDocumentListItems called."); // Log method entry

            try
            {
                var jsonResponse = await _httpClient.GetAsync("ServiceDocumentList/GetAllDocuments"); // Send GET request to get list of documents
                if (!jsonResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to fetch document list. Status code: {StatusCode}", jsonResponse.StatusCode); // Log error if fetching documents fails
                    return new List<SelectListItem>(); // Return empty list in case of failure
                }

                var responseString = await jsonResponse.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(responseString))
                {
                    _logger.LogWarning("Received empty response while fetching document list."); // Log warning if response is empty
                    return new List<SelectListItem>(); // Return empty list if response is empty
                }

                List<ServiceDocumenList> documentList = JsonConvert.DeserializeObject<List<ServiceDocumenList>>(responseString); // Deserialize response to list of ServiceDocumenList
                return documentList.Select(i => new SelectListItem
                {
                    Text = i.ServiceDocumentName,
                    Value = i.ServiceDocumenListtId.ToString()
                }).ToList();
            }
            catch (JsonSerializationException ex)
            {
                _logger.LogError(ex, "JSON deserialization error occurred while fetching document list."); // Log JSON deserialization exception
                ModelState.AddModelError(string.Empty, "Failed to load Document List."); // Add model error for deserialization issues
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error occurred while fetching document list."); // Log HTTP request exception
                ModelState.AddModelError(string.Empty, "Error communicating with the server. Please try again later."); // Add model error for communication issues
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while fetching document list."); // Log any other exceptions
                ModelState.AddModelError(string.Empty, "Failed to load Document List."); // Add generic error message to model state
            }

            return new List<SelectListItem>(); // Return empty list in case of exception
        }

        // Method to get default service document list data
        public async Task<List<int>> GetDefaultServiceDocumentListData(int serviceID)
        {
            _logger.LogInformation("GetDefaultServiceDocumentListData called with Service ID: {ServiceID}", serviceID); // Log method entry with service ID

            // Validate input parameter
            if (serviceID <= 0)
            {
                _logger.LogWarning("Invalid service ID: {ServiceID}", serviceID); // Log warning if service ID is invalid
                ModelState.AddModelError(string.Empty, "Invalid service ID."); // Add model error for invalid service ID
                return new List<int>(); // Return empty list in case of invalid input
            }

            try
            {
                var response = await _httpClient.GetAsync("MServiceDefaultDocument/GetServiceDocuments?serviceID=" + serviceID); // Send GET request to get service documents
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to fetch documents for Service ID: {ServiceID}. Status code: {StatusCode}", serviceID, response.StatusCode); // Log error if fetching documents fails
                    return new List<int>(); // Return empty list in case of failure
                }

                var documents = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(documents))
                {
                    _logger.LogWarning("Received empty response while fetching documents for Service ID: {ServiceID}", serviceID); // Log warning if response is empty
                    return new List<int>(); // Return empty list if response is empty
                }

                List<M_SericeDefaultDocumentList> documentList = JsonConvert.DeserializeObject<List<M_SericeDefaultDocumentList>>(documents); // Deserialize response to list of M_SericeDefaultDocumentList
                _logger.LogInformation("Fetched service documents successfully for Service ID: {ServiceID}", serviceID); // Log successful document fetch
                return documentList.Select(x => x.DocumentID).ToList(); // Return list of document IDs
            }
            catch (JsonSerializationException ex)
            {
                _logger.LogError(ex, "JSON deserialization error occurred while fetching documents for Service ID: {ServiceID}", serviceID); // Log JSON deserialization exception
                ModelState.AddModelError(string.Empty, "Failed to load service documents."); // Add model error for deserialization issues
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error occurred while fetching documents for Service ID: {ServiceID}", serviceID); // Log HTTP request exception
                ModelState.AddModelError(string.Empty, "Error communicating with the server. Please try again later."); // Add model error for communication issues
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while fetching documents for Service ID: {ServiceID}", serviceID); // Log any other exceptions
                ModelState.AddModelError(string.Empty, "Failed to load service documents."); // Add generic error message to model state
            }

            return new List<int>(); // Return empty list in case of exception
        }
        [HttpPost]
        public async Task<IActionResult> RequestAmountChange(int serviceId, decimal requestedAmount)
        {
            _logger.LogInformation("Merchant requested amount change for Service ID: {ServiceId}", serviceId);
            string _merchantId = HttpContext.Session.GetEncryptedString("ProviderId", _protector); // Get encrypted Merchant ID from session
            if (serviceId <= 0 || requestedAmount <= 0)
            {
                _logger.LogWarning("Invalid request: ServiceId or RequestedAmount is invalid.");
                return BadRequest("Invalid request parameters.");
            }

            try
            {
                AmountChangeRequestModel amountChange = new AmountChangeRequestModel();
                amountChange.ServiceId = serviceId;
                amountChange.ProviderId = Convert.ToInt32(_merchantId);
                amountChange.RequestedAmount = requestedAmount;
                var response = await _httpClient.PostAsync("Slabs/RequestAmountChange", Customs.GetJsonContent(amountChange));

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Amount change request sent successfully for Service ID: {ServiceId}", serviceId);
                    //Notification:
                    // Trigger notification
                    var notification = new Notification
                    {
                        UserId = "0",
                        Message = $"Merchant[{_merchantId}] has sent a Amount change request for the Service[{serviceId}].",
                        MerchantId = _merchantId,
                        RedirectToActionUrl = $"/Providers/MerchantServices?MerchantId={_merchantId}",
                        MessageFromId = Convert.ToInt32(_merchantId),
                        SenderType = "Merchant",
                        SID = Convert.ToInt32(serviceId)
                    };

                    var res = await _httpClient.PostAsync("Notifications/CreateNotification?StatusId=25", Customs.GetJsonContent(notification));
                    string resString = await res.Content.ReadAsStringAsync();
                    _logger.LogInformation("Notification Response : " + resString);
                    return Ok("Request sent successfully.");
                }

                _logger.LogError("Failed to send amount change request. Status code: {StatusCode}", response.StatusCode);
                return StatusCode(500, "Failed to send request.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during amount change request for Service ID: {ServiceId}", serviceId);
                return StatusCode(500, "An error occurred while sending the request.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetApplicableSlab(int amount)
        {
            _logger.LogInformation("Fetching applicable slab for amount: {Amount}", amount);

            try
            {
                // Get merchant ID from session
                string merchantId = HttpContext.Session.GetEncryptedString("ProviderId", _protector);

                if (string.IsNullOrEmpty(merchantId))
                {
                    _logger.LogWarning("Merchant ID not found in session");
                    return BadRequest("Merchant ID not found");
                }

                // First, try to get membership-based slab
                var membershipResponse = await _httpClient.GetAsync($"Slabs/GetMembershipApplicableSlab?amount={amount}&merchantId={merchantId}");

                if (membershipResponse.IsSuccessStatusCode)
                {
                    var membershipResponseString = await membershipResponse.Content.ReadAsStringAsync();
                    var membershipSlab = JsonConvert.DeserializeObject<Slab>(membershipResponseString);

                    if (membershipSlab != null)
                    {
                        _logger.LogInformation("Membership slab found for amount: {Amount}, MerchantId: {MerchantId}", amount, merchantId);
                        return Json(membershipSlab);
                    }
                }

                // If no membership slab found, fall back to default slab
                _logger.LogInformation("No membership slab found, falling back to default slab for amount: {Amount}", amount);
                var defaultResponse = await _httpClient.GetAsync($"Slabs/GetDefaultApplicableSlab?amount={amount}");
                var defaultResponseString = await defaultResponse.Content.ReadAsStringAsync();
                var defaultSlab = JsonConvert.DeserializeObject<Slab>(defaultResponseString);

                if (defaultSlab == null)
                {
                    _logger.LogWarning("No applicable slab found for amount: {Amount}", amount);
                    return NotFound("No applicable slab found.");
                }

                return Json(defaultSlab);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching applicable slab for amount: {Amount}", amount);
                return StatusCode(500, "Error fetching applicable slab.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ApplyMembershipDiscount(int providerId)
        {
            _logger.LogInformation("Checking membership discount for Provider ID: {ProviderId}", providerId);

            try
            {
                var response = await _httpClient.GetAsync($"Membership/CheckDiscount?providerId={providerId}");
                var responseString = await response.Content.ReadAsStringAsync();
                var discount = JsonConvert.DeserializeObject<decimal>(responseString);

                return Json(discount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying membership discount for Provider ID: {ProviderId}", providerId);
                return StatusCode(500, "Error applying membership discount.");
            }
        }
        private async Task<ProviderSlab> GetProviderSlab()
        {

            string _merchantId = HttpContext.Session.GetEncryptedString("ProviderId", _protector); // Get encrypted Merchant ID from session
            _logger.LogInformation("Checking Provider Slab for Provider ID: {ProviderId}", _merchantId);
            try
            {

                var response = await _httpClient.GetAsync($"ProviderSlab/GetSlabByProviderId?id={_merchantId}");
                var responseString = await response.Content.ReadAsStringAsync();
                var providerSlab = JsonConvert.DeserializeObject<ProviderSlab>(responseString);
                //Check if the provider slab for this merchant is Fixed or percentage
                if (providerSlab == null)
                {
                    _logger.LogWarning("No applicable provider Slab found for Provider ID: {ProviderId}", _merchantId);
                    return new ProviderSlab();
                }
                if (providerSlab.ProviderSlabID == 0)
                {
                    ProviderSlab pSlab = new ProviderSlab();
                    pSlab.SelectedModel = "Fix";
                    pSlab.ProviderID = Convert.ToInt32(_merchantId);

                    response = await _httpClient.PostAsync($"ProviderSlab", Customs.GetJsonContent(pSlab));
                    responseString = await response.Content.ReadAsStringAsync();
                    providerSlab = JsonConvert.DeserializeObject<ProviderSlab>(responseString);
                }
                //Process Payment for basic
                //Fetch basic membership details

                //var planslist = await _httpClient.GetAsync($"MembershipPlans");
                //var planslistData = await planslist.Content.ReadAsStringAsync();
                //var _data = JsonConvert.DeserializeObject<List<MembershipPlan>>(planslistData);
                //var basicMembership = _data.Where(x => x.Price == 0).FirstOrDefault();

                //if (basicMembership == null)
                //{
                //    throw new Exception("No Membership Details Exist. Please create one to process further");
                //}



                //var paymentHistory = new MembershipPaymentHistory
                //{
                //    PAYMENTTYPE = "Online",
                //    AMOUNT = "0",
                //    PAYERID = Convert.ToInt32(_merchantId),
                //    Duration = basicMembership.Duration,
                //    ISPAYMENTSUCCESS = 1,
                //    MembershipId = basicMembership.Id,
                //    PAYMENTDATETIME = DateTime.Now,
                //    Quantity = 1,
                //    MemberShipname = basicMembership.Name
                //};

                //var PaymentresponseMessage = await _httpClient.PostAsync("MembershipPayment/sendRequestToSaveMembershipPayment", Customs.GetJsonContent(paymentHistory));
                //var PaymentresponseString = await PaymentresponseMessage.Content.ReadAsStringAsync();
                return providerSlab;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error No applicable provider Slab found for merchant: {ProviderId}", _merchantId);
                throw;
            }
        }
    }
    public class AmountChangeRequestModel
    {
        public int ServiceId { get; set; }
        public int ProviderId { get; set; }
        public decimal RequestedAmount { get; set; }
    }
}
