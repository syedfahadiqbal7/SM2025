using AFFZ_Provider.Models;
using AFFZ_Provider.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace AFFZ_Provider.Controllers
{
    [Authorize]
    public class MerchantResponseToUser : Controller
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
        public MerchantResponseToUser(ILogger<MerchantResponseToUser> logger, IWebHostEnvironment environment, IHttpClientFactory httpClientFactory, IDataProtectionProvider provider, IAppSettingsService service)
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

        [HttpGet]
        public async Task<ActionResult> MerchantResponseIndex()
        {
            string userId = HttpContext.Session.GetEncryptedString("ProviderId", _protector); // Placeholder for session user ID retrieval
            try
            {
                var jsonResponse = await _httpClient.GetAsync($"CategoryWithMerchant/AllResponsesFromMerchant?Uid={userId}");
                jsonResponse.EnsureSuccessStatusCode();
                if (jsonResponse != null)
                {
                    var responseString = await jsonResponse.Content.ReadAsStringAsync();
                    List<RequestForDisCountToUserViewModel> categories = JsonConvert.DeserializeObject<List<RequestForDisCountToUserViewModel>>(responseString);
                    ViewBag.ResponseForDisCountFromMerchant = categories;
                }
                else
                {
                    _logger.LogWarning("Empty response received from API for userId: {UserId}", userId);
                    ViewBag.ResponseForDisCountFromMerchant = new List<RequestForDisCountToUserViewModel>();
                }
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

        [HttpGet]
        public async Task<ActionResult> UploadDocuments()
        {
            try
            {
                int UserId = Convert.ToInt32(HttpContext.Session.GetEncryptedString("ProviderId", _protector));//Convert.ToInt32(HttpContext.Session.GetString("UserId"));

                var jsonResponse = await _httpClient.GetAsync("FileUpload/GetFilesList");
                string responseString = await jsonResponse.Content.ReadAsStringAsync();
                FileUploadViewModel model = new FileUploadViewModel();

                if (!string.IsNullOrEmpty(responseString))
                {
                    List<UploadedFile> documentList = JsonConvert.DeserializeObject<List<UploadedFile>>(responseString);
                    model.UploadedFiles = documentList.Where(x => x.UserId == UserId).ToList();
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
        public async Task<ActionResult> ReviewDocument(int userId, int rfdfu, int quantity, int sid)
        {
            try
            {
                var jsonResponse = await _httpClient.GetAsync($"FileUpload/ReviewDocuments/{userId}/{rfdfu}");
                string responseString = await jsonResponse.Content.ReadAsStringAsync();
                FileUploadViewModel model = new FileUploadViewModel();

                if (!string.IsNullOrEmpty(responseString))
                {
                    List<UploadedFile> documentList = JsonConvert.DeserializeObject<List<UploadedFile>>(responseString);
                    model.UploadedFiles = documentList.Where(x => x.UserId == userId && x.UploadedBy == "Customer").ToList();
                }
                ViewBag.UserId = userId;
                ViewBag.SaveResponse = model;
                ViewBag.ServiceId = sid;
                ViewBag.RFDFU = rfdfu;
                ViewBag.StatusList = await StatusList(0);
                ViewBag.quantity = quantity;
                ViewBag.UserUrl = $"{CustomerHttpsPort}";
                var GetServicename = await _httpClient.GetAsync($"Service/GetSerViceNameByRFDFUID?id=" + rfdfu);
                ViewBag.ServiceName = await GetServicename.Content.ReadAsStringAsync();
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
        public async Task<IActionResult> UploadDocuments(FileUploadViewModel model)
        {
            if (model == null || model.UserDocuments == null || model.UserDocuments.Count == 0)
            {
                _logger.LogWarning("Invalid model passed to UploadDocuments");
                return BadRequest("Invalid model.");
            }

            _logger.LogInformation("UploadDocuments called with model: {Model}", model);

            try
            {
                if (ModelState.IsValid)
                {
                    int loginUserId = Convert.ToInt32(HttpContext.Session.GetEncryptedString("UserId", _protector)); // Placeholder for session user ID retrieval
                    var username = $"Documents_{loginUserId}";

                    var folderPath = Path.Combine(_environment.WebRootPath, "uploads", username);
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    var fileUploadModelAPI = new FileUploadModelAPI
                    {
                        UserId = loginUserId,
                        UploadedFiles = new List<UploadedFile>()
                    };

                    foreach (var file in model.UserDocuments)
                    {
                        if (file.Length > 0)
                        {
                            var fileName = Path.GetFileName(file.FileName);
                            var filePath = Path.Combine(folderPath, fileName);

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
                                UserId = loginUserId
                            };

                            fileUploadModelAPI.UploadedFiles.Add(uploadedFile);
                        }
                    }

                    var responseMessage = await _httpClient.PostAsync("FileUpload/UploadFiles", Customs.GetJsonContent(fileUploadModelAPI));
                    string responseString = await responseMessage.Content.ReadAsStringAsync();
                    TempData["SuccessMessage"] = responseString;
                    return RedirectToAction("UploadDocuments");
                }

                return RedirectToAction("UploadDocuments");
            }
            catch (Exception ex)
            {
                TempData["FailMessage"] = ex.Message;
                _logger.LogError(ex, "An error occurred while uploading documents.");
                return StatusCode(500, "An error occurred. Please try again later.");

            }
        }

        [HttpGet]
        public IActionResult DownloadFile(string fileName, string folderName)
        {
            _logger.LogInformation("DownloadFile called with fileName: {FileName}, folderName: {FolderName}", fileName, folderName);

            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(folderName))
            {
                _logger.LogWarning("Invalid parameters passed to DownloadFile");
                return BadRequest("Invalid parameters.");
            }

            var filePath = Path.Combine(_environment.WebRootPath.Replace("AFFZ_Provider", "AFFZ_MVC"), "uploads", folderName, fileName);
            if (System.IO.File.Exists(filePath))
            {
                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, "application/octet-stream", fileName);
            }
            else
            {
                _logger.LogWarning("File not found: {FilePath}", filePath);
                return NotFound();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUsersWithDocuments(int? statusid = null)
        {
            int merchantId = Convert.ToInt32(HttpContext.Session.GetEncryptedString("ProviderId", _protector)); // Placeholder for session merchant ID retrieval

            try
            {
                // Build the query string based on available parameters
                var queryParams = $"merchantId={merchantId}";
                if (statusid.HasValue)
                {
                    queryParams += $"&statusid={statusid.Value}";
                }

                var jsonResponse = await _httpClient.GetAsync("FileUpload/UsersWithDocuments?" + queryParams);
                string responseString = await jsonResponse.Content.ReadAsStringAsync();
                UserDocumentsViewModel model = new UserDocumentsViewModel();

                List<UserDocumentsViewModel> documentList = new List<UserDocumentsViewModel>();
                if (!string.IsNullOrEmpty(responseString))
                {
                    documentList = JsonConvert.DeserializeObject<List<UserDocumentsViewModel>>(responseString);
                    documentList = documentList.Where(x => x.MID == merchantId).ToList();

                }
                foreach (var document in documentList)
                {
                    int uid = document.UserId;
                    int RFDFUID = document.RFDFU;

                    // Fetch file status
                    jsonResponse = await _httpClient.GetAsync($"FileUpload/AllFileVerifies?userId={uid}&RFDFU={RFDFUID}");
                    responseString = await jsonResponse.Content.ReadAsStringAsync();
                    FileStatus fstatusmodel = new FileStatus();
                    if (!string.IsNullOrEmpty(responseString))
                    {
                        document.FileStatus = JsonConvert.DeserializeObject<FileStatus>(responseString);

                    }

                    // Fetch current service status
                    jsonResponse = await _httpClient.GetAsync($"CategoryWithMerchant/GetCurrentServiceStatusById?customerId={uid}&RFDFU={RFDFUID}&mid={merchantId}");
                    responseString = await jsonResponse.Content.ReadAsStringAsync();
                    document.ServiceStatus = JsonConvert.DeserializeObject<RequestStatus>(responseString);
                }
                ViewBag.UsersWithDocuments = documentList;
                ViewBag.CurrentStatus = await GetStatusNameByRFDFU(documentList[0].RFDFU);
                ViewBag.StatusList = await StatusList(0);
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
        public async Task<IActionResult> VerifyDocument(string documentId, string documentName, int RFDFU, string userId, string quantity, string serviceId)
        {
            _logger.LogInformation("VerifyDocument method called for DocumentId: {DocumentId}", documentId);
            try
            {
                var responseMessage = await _httpClient.PostAsync($"FileUpload/VerifyDocument/{documentId}", Customs.GetJsonContent(documentId));
                string responseString = await responseMessage.Content.ReadAsStringAsync();
                string _merchantId = HttpContext.Session.GetEncryptedString("ProviderId", _protector);
                _logger.LogInformation($"Document verification response for DocumentId: {documentId}. Response : {responseString}");
                TempData["SuccessMessage"] = responseString;
                ViewBag.SaveResponse = responseString;
                //Tracker
                var TrackerUpdate = new TrackServiceStatusHistory
                {
                    ChangedByID = Convert.ToInt32(_merchantId),
                    StatusID = 14,
                    RFDFU = RFDFU,
                    ChangedByUserType = "Merchant",
                    ChangedOn = DateTime.Now,
                    Comments = $"Document [{documentName}] Verified by Merchant"
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
                    UserId = userId.ToString(),
                    Message = $"Merchant[{_merchantId.ToString()}] has Verify your Document[{documentId}] for the Service[{serviceId}].",
                    MerchantId = _merchantId,
                    RedirectToActionUrl = "/MerchantResponseToUser/CheckDocumentStatus?RFDFU=" + RFDFU + "&UID=" + userId + "&MerchantID=" + _merchantId + "",
                    MessageFromId = Convert.ToInt32(_merchantId),
                    SID = Convert.ToInt32(serviceId),
                    SenderType = "Merchant"
                };
                int sid = Convert.ToInt32(serviceId);
                var res = await _httpClient.PostAsync("Notifications/CreateNotification?StatusId=14", Customs.GetJsonContent(notification));
                string resString = await res.Content.ReadAsStringAsync();
                _logger.LogInformation("Notification Response : " + resString);
                return RedirectToAction("ReviewDocument", new { userId, RFDFU, quantity, sid });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while verifying document for DocumentId: {DocumentId}", documentId);
                return StatusCode(500, "Internal server error.");
            }
        }
        [HttpPost]
        public async Task<IActionResult> ResendDocument(string documentId, string documentName, int RFDFU, string userId, string quantity)
        {
            _logger.LogInformation("ResendDocument method called for DocumentId: {DocumentId}", documentId);
            try
            {
                var responseMessage = await _httpClient.PostAsync($"FileUpload/ResendDocument/{documentId}", Customs.GetJsonContent(documentId));
                string responseString = await responseMessage.Content.ReadAsStringAsync();
                string _merchantId = HttpContext.Session.GetEncryptedString("ProviderId", _protector);
                _logger.LogInformation($"Document verification response for DocumentId: {documentId}. Response : {responseString}");
                TempData["SuccessMessage"] = responseString;
                ViewBag.SaveResponse = responseString;

                //Tracker
                var TrackerUpdate = new TrackServiceStatusHistory
                {
                    ChangedByID = Convert.ToInt32(_merchantId),
                    StatusID = 15,
                    RFDFU = RFDFU,
                    ChangedByUserType = "Merchant",
                    ChangedOn = DateTime.Now,
                    Comments = $"Document [{documentName}] Rejected by Merchant"
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
                //Get Service Id for Tracking and notification Purpose:
                //
                var jsonServiceIdResponse = await _httpClient.GetAsync("RequestForDisCountToUser/GetServiceIdFromRfdfu");
                string ServiceIdString = await jsonServiceIdResponse.Content.ReadAsStringAsync();

                // Trigger notification
                var notification = new Notification
                {
                    UserId = userId.ToString(),
                    Message = $"Merchant[{_merchantId.ToString()}] has Declined your Document[{documentId}]. Please Resend. You Can Contact over the chat with Merchant to know more on document rejection for the Service[{ServiceIdString}].",
                    MerchantId = _merchantId,
                    RedirectToActionUrl = "/MerchantResponseToUser/CheckDocumentStatus?RFDFU=" + RFDFU + "&UID=" + userId + "&MerchantID=" + _merchantId + "",
                    MessageFromId = Convert.ToInt32(_merchantId),
                    SenderType = "Merchant"
                };
                int sid = Convert.ToInt32(ServiceIdString);
                var res = await _httpClient.PostAsync("Notifications/CreateNotification?StatusId=15", Customs.GetJsonContent(notification));
                string resString = await res.Content.ReadAsStringAsync();
                _logger.LogInformation("Notification Response : " + resString);
                return RedirectToAction("ReviewDocument", new { userId, RFDFU, quantity, sid });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while verifying document for DocumentId: {DocumentId}", documentId);
                return StatusCode(500, "Internal server error.");
            }
        }
        [HttpGet]
        public async Task<IActionResult> DeleteDocument(string documentId)
        {
            _logger.LogInformation("DeleteDocument method called for DocumentId: {DocumentId}", documentId);
            try
            {
                var responseMessage = await _httpClient.PostAsync($"FileUpload/DeleteDocument/{documentId}", Customs.GetJsonContent(documentId));
                string responseString = await responseMessage.Content.ReadAsStringAsync();
                _logger.LogInformation($"Document Deletion response for DocumentId: {documentId}. Response : {responseString}");
                TempData["SuccessMessage"] = responseString;
                ViewBag.SaveResponse = responseString;
                return RedirectToAction("UploadDocuments");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while verifying document for DocumentId: {DocumentId}", documentId);
                return StatusCode(500, "Internal server error.");
            }
        }
        [HttpPost]
        public async Task<IActionResult> ProcessToApplyForServiceStatus(string CurrentStatus, string ServiceID, string UserId, string RFDFU, IFormFile UploadedFile)
        {
            var userId = int.Parse(UserId ?? "0"); // Placeholder for session-based User ID retrieval
            var merchantId = int.Parse(HttpContext.Session.GetEncryptedString("ProviderId", _protector) ?? "0"); // Placeholder for session-based Merchant ID retrieval
            var merchantName = HttpContext.Session.GetEncryptedString("ProviderName", _protector) ?? "0"; // Placeholder for session-based Merchant ID retrieval
            var _serviceId = int.Parse(ServiceID ?? "0"); // Placeholder for Service ID retrieval

            var statusUpdate = new CurrentServiceStatusViewModel
            {
                UId = userId,
                MId = merchantId,
                RFDFU = Convert.ToInt32(RFDFU),
                CurrentStatus = CurrentStatus
            };

            // Send the request to the AFFZ_API
            var response = await _httpClient.PostAsJsonAsync("CategoryWithMerchant/UpdateServiceStatus", statusUpdate);

            if (response.IsSuccessStatusCode)
            {
                //Get Status Id
                if ((CurrentStatus == "19" || CurrentStatus == "17") && UploadedFile != null)
                {
                    var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                    if (!Directory.Exists(uploadsPath))
                    {
                        Directory.CreateDirectory(uploadsPath);
                    }

                    var filePath = Path.Combine(uploadsPath, UploadedFile.FileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await UploadedFile.CopyToAsync(fileStream);
                    }

                    // Log file upload or save to DB
                    _logger.LogInformation("File uploaded successfully: {FilePath}", filePath);

                    // Optional: Call API to save file info in DB
                    var fileUploadModelAPI = new FileUploadModelAPI
                    {
                        UserId = userId,
                        RFDFU = Convert.ToInt32(RFDFU),
                        MID = merchantId,
                        UploadedBy = "Provider",
                        UploadedFiles = new List<UploadedFile>()
                    };

                    if (UploadedFile.Length > 0)
                    {
                        var fileName = Path.GetFileName(UploadedFile.FileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await UploadedFile.CopyToAsync(stream);
                        }

                        var uploadedFile = new UploadedFile
                        {
                            FileName = fileName,
                            ContentType = UploadedFile.ContentType,
                            FileSize = UploadedFile.Length,
                            FolderName = $"{merchantName}_{userId}_{RFDFU}",
                            Status = "Verified",
                            UserId = userId,
                            MerchantId = merchantId,
                            RFDFU = int.Parse(RFDFU),
                            UploadedBy = "Provider"
                        };

                        fileUploadModelAPI.UploadedFiles.Add(uploadedFile);
                    }
                    var fileResponse = await _httpClient.PostAsync("FileUpload/UploadFiles", Customs.GetJsonContent(fileUploadModelAPI));

                    if (!fileResponse.IsSuccessStatusCode)
                    {
                        TempData["FailMessage"] = $"File upload failed with Error{await fileResponse.Content.ReadAsStringAsync()}, but status was updated.";
                        return RedirectToAction("GetUsersWithDocuments");
                    }
                }
                //"Your service process has started. You will be notified once updated by the merchant.";//Notifiaction
                string StatusDesc = await getStatusName(Convert.ToInt32(CurrentStatus));

                var TrackerUpdate = new TrackServiceStatusHistory
                {
                    ChangedByID = merchantId,
                    StatusID = Convert.ToInt32(CurrentStatus),
                    RFDFU = int.Parse(RFDFU ?? "0"),
                    ChangedByUserType = "Merchant",
                    ChangedOn = DateTime.Now,
                    Comments = StatusDesc
                };

                // Send the request to the AFFZ_API
                var TrackerUpdateResponse = await _httpClient.PostAsJsonAsync("TrackServiceStatusHistory/CreateStatus", TrackerUpdate);

                if (TrackerUpdateResponse.IsSuccessStatusCode)
                {

                    //"Your service process has started. You will be notified once updated by the merchant.";//Notifiaction
                    TempData["SuccessMessage"] = "Service and Tracker process Status Updated Successfully.";
                    // Trigger notification
                    var notification = new Notification
                    {
                        UserId = userId.ToString(),
                        Message = $"Merchant[{merchantId.ToString()}] has {StatusDesc} the Service[{ServiceID}]",
                        MerchantId = merchantId.ToString(),
                        RedirectToActionUrl = "/MerchantResponseToUser/MerchantResponseIndex",
                        MessageFromId = Convert.ToInt32(merchantId),
                        SenderType = "Merchant",
                        SID = Convert.ToInt32(ServiceID)
                    };

                    var res = await _httpClient.PostAsync("Notifications/CreateNotification?StatusId=" + CurrentStatus, Customs.GetJsonContent(notification));
                    string resString = await res.Content.ReadAsStringAsync();
                    _logger.LogInformation("Notification Response : " + resString);
                    return RedirectToAction("GetUsersWithDocuments");
                }
                else
                {
                    TempData["FailMessage"] = "Service updated but Failed to update the Tracker process.";
                    return RedirectToAction("GetUsersWithDocuments");
                }
            }
            else
            {
                TempData["FailMessage"] = "Failed to update the service process.";
                return RedirectToAction("GetUsersWithDocuments");
            }
        }
        [HttpGet]
        public async Task<string> getStatusName(int statusId)
        {
            string StatusDesc = string.Empty;
            var jsonResponse = await _httpClient.GetAsync("RequestStatus/GetStatusById?id=" + statusId);
            var responseString = await jsonResponse.Content.ReadAsStringAsync();

            // Initialize an empty list for the documents
            var statusList = new List<SelectListItem>();

            if (!string.IsNullOrEmpty(responseString))
            {
                try
                {
                    // Deserialize the JSON response into a list of documents
                    RequestStatus documents = JsonConvert.DeserializeObject<RequestStatus>(responseString);

                    StatusDesc = documents.StatusDescription;
                }
                catch (JsonSerializationException ex)
                {
                    // Log any deserialization errors
                    _logger.LogError(ex, "JSON deserialization error.");
                    StatusDesc = ex.Message;
                }
                catch (Exception ex)
                {
                    StatusDesc = ex.Message;
                }
            }
            return StatusDesc;
            // Return the list of SelectListItem objects

        }
        public async Task<List<SelectListItem>> StatusListUpdates(int currentStatusId, int userId, int merchantId)
        {
            var jsonResponse = await _httpClient.GetAsync("RequestStatus/GetAllStatusesToUpdateMerchant?UserType=Merchant");
            var responseString = await jsonResponse.Content.ReadAsStringAsync();

            // Initialize an empty list for the documents
            var statusList = new List<SelectListItem>();

            if (!string.IsNullOrEmpty(responseString))
            {
                try
                {
                    // Deserialize the JSON response into a list of documents
                    List<RequestStatus> _documents = JsonConvert.DeserializeObject<List<RequestStatus>>(responseString);
                    List<RequestStatus> documents = _documents.Where(x => x.StatusID > currentStatusId).ToList();
                    // Map each document to a SelectListItem
                    statusList = documents.Select(doc => new SelectListItem
                    {
                        Text = doc.StatusDescription,
                        Value = doc.StatusID.ToString(),
                    }).ToList();
                }
                catch (JsonSerializationException ex)
                {
                    // Log any deserialization errors
                    _logger.LogError(ex, "JSON deserialization error.");
                }
            }

            // Return the list of SelectListItem objects
            return statusList;
        }
        public async Task<List<SelectListItem>> StatusList(int statusId)
        {
            var jsonResponse = await _httpClient.GetAsync("RequestStatus/GetAllStatusesToUpdateMerchant?UserType=Merchant");
            var responseString = await jsonResponse.Content.ReadAsStringAsync();

            // Initialize an empty list for the documents
            var statusList = new List<SelectListItem>();

            if (!string.IsNullOrEmpty(responseString))
            {
                try
                {
                    // Deserialize the JSON response into a list of documents
                    List<RequestStatus> documents = JsonConvert.DeserializeObject<List<RequestStatus>>(responseString);

                    // Map each document to a SelectListItem
                    statusList = documents.Select(doc => new SelectListItem
                    {
                        Text = doc.StatusDescription,
                        Value = doc.StatusID.ToString(),
                        Selected = doc.StatusID == statusId // Set selected based on IDs
                    }).ToList();
                }
                catch (JsonSerializationException ex)
                {
                    // Log any deserialization errors
                    _logger.LogError(ex, "JSON deserialization error.");
                }
            }

            // Return the list of SelectListItem objects
            return statusList;
        }
        public async Task<string> GetStatusName(int statusId)
        {
            try
            {
                var jsonResponse = await _httpClient.GetAsync("RequestStatus/GetAllStatuses?UserType=Merchant");
                var responseString = await jsonResponse.Content.ReadAsStringAsync();

                // Initialize an empty list for the documents
                string statusName = string.Empty;

                if (!string.IsNullOrEmpty(responseString))
                {
                    try
                    {
                        // Deserialize the JSON response into a list of documents
                        List<RequestStatus> documents = JsonConvert.DeserializeObject<List<RequestStatus>>(responseString);

                        // Map each document to a SelectListItem
                        var result = documents.Find(x => x.StatusID.Equals(statusId));
                        if (result != null)
                        {
                            statusName = documents.Where(x => x.StatusID == statusId).Select(x => x.StatusName).First();
                        }
                        else { statusName = "Pending"; }
                    }
                    catch (JsonSerializationException ex)
                    {
                        // Log any deserialization errors
                        _logger.LogError(ex, "JSON deserialization error.");
                        statusName = ex.Message;
                    }
                }

                // Return the list of SelectListItem objects
                return statusName;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<string> GetStatusNameByRFDFU(int Rfdfu)
        {
            var jsonResponse = await _httpClient.GetAsync("CategoryWithMerchant/GetAllCurrentServiceStatus?UserType=Merchant");
            var responseString = await jsonResponse.Content.ReadAsStringAsync();
            int statusId = 0;
            // Initialize an empty list for the documents
            string statusName = string.Empty;

            if (!string.IsNullOrEmpty(responseString))
            {
                try
                {
                    // Deserialize the JSON response into a list of documents
                    List<CurrentServiceStatusViewModel> CurrentStatus = JsonConvert.DeserializeObject<List<CurrentServiceStatusViewModel>>(responseString);

                    // Map each document to a SelectListItem
                    statusId = int.Parse(CurrentStatus.Where(x => x.RFDFU == Rfdfu).Select(x => x.CurrentStatus).First());
                    statusName = await GetStatusName(statusId);
                }
                catch (JsonSerializationException ex)
                {
                    // Log any deserialization errors
                    _logger.LogError(ex, "JSON deserialization error.");
                    statusName = ex.Message;
                }
            }

            // Return the list of SelectListItem objects
            return statusName;
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
    }
    public class FileUploadViewModel
    {
        [Required]
        public IFormFileCollection UserDocuments { get; set; }
        public int UserId { get; set; }
        public List<UploadedFile>? UploadedFiles { get; set; }
    }
    public class UserDocumentsViewModel
    {
        public int DocumentCount { get; set; }
        public int ServiceID { get; set; }
        public string ServiceName { get; set; }
        public int ServicePrice { get; set; }
        public int ServiceAmountPaidToAdmin { get; set; }
        public string SelectedDocumentIds { get; set; }
        public int CategoryID { get; set; }
        public int FINALPRICE { get; set; }
        public int RFDFU { get; set; }
        public int IsPaymentDone { get; set; }
        public int IsMerchantSelected { get; set; }
        public string CompanyName { get; set; }
        public string CustomerName { get; set; }
        public int MID { get; set; }
        public int Quantity { get; set; }
        public int ISPAYMENTSUCCESS { get; set; }
        public string PAYMENTTYPE { get; set; }
        public int UserId { get; set; }//
        public string MerchantName { get; set; }
        public RequestStatus? ServiceStatus { get; set; }
        public FileStatus? FileStatus { get; set; }
    }

    public class FileStatus
    {
        public int TotalFiles { get; set; }
        public int TotalVerified { get; set; }
        public int TotalPending { get; set; }
    }
    public class FileUploadModelAPI
    {
        public int UserId { get; set; }
        public int MID { get; set; }
        public int RFDFU { get; set; }
        public string? UploadedBy { get; set; }

        public List<UploadedFile>? UploadedFiles { get; set; }
    }
}
