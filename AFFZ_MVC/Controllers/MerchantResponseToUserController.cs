using AFFZ_Customer.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AFFZ_Customer.Controllers
{
    public class MerchantResponseToUserController : Controller
    {
        private readonly ILogger<MerchantResponseToUserController> _logger;
        private readonly IWebHostEnvironment _environment;

        public MerchantResponseToUserController(ILogger<MerchantResponseToUserController> logger, IWebHostEnvironment environment, WebApiHelper webApiHelper)
        {
            _logger = logger;
            _environment = environment;
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            string userId = HttpContext.Session.GetString("UserId"); // Placeholder for session user ID retrieval
            try
            {
                var jsonResponse = await WebApiHelper.GetData($"/api/CategoryWithMerchant/AllResponsesFromMerchant?Uid={userId}");

                if (!string.IsNullOrEmpty(jsonResponse))
                {
                    List<RequestForDisCountToUserViewModel> categories = JsonConvert.DeserializeObject<List<RequestForDisCountToUserViewModel>>(jsonResponse);
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

        [HttpPost]
        public async Task<ActionResult> PaymentDone(string amount, string paymentType, string payerId, string merchantId, string serviceId, string rfdfu)
        {
            _logger.LogInformation("PaymentDone called with amount: {Amount}, paymentType: {PaymentType}, payerId: {PayerId}, merchantId: {MerchantId}, serviceId: {ServiceId}, rfdfu: {RFDFU}", amount, paymentType, payerId, merchantId, serviceId, rfdfu);

            if (string.IsNullOrEmpty(amount) || string.IsNullOrEmpty(paymentType) || string.IsNullOrEmpty(payerId) || string.IsNullOrEmpty(merchantId) || string.IsNullOrEmpty(serviceId) || string.IsNullOrEmpty(rfdfu))
            {
                _logger.LogWarning("Invalid parameters passed to PaymentDone");
                return BadRequest("Invalid parameters.");
            }

            try
            {
                var paymentHistory = new PaymentHistory
                {
                    PAYMENTTYPE = paymentType,
                    AMOUNT = amount,
                    PAYERID = Convert.ToInt32(payerId),
                    MERCHANTID = Convert.ToInt32(merchantId),
                    ISPAYMENTSUCCESS = 1,
                    SERVICEID = Convert.ToInt32(serviceId),
                    PAYMENTDATETIME = DateTime.Now,
                };

                string responseMessage = await WebApiHelper.PostData("/api/Payment/sendRequestToSavePayment", paymentHistory);

                if (responseMessage.Contains("Payment Done Successfully"))
                {
                    var discountUpdateInfo = new RequestForDisCountToUser
                    {
                        RFDFU = Convert.ToInt32(rfdfu),
                        SID = Convert.ToInt32(serviceId),
                        MID = Convert.ToInt32(merchantId),
                        UID = Convert.ToInt32(payerId),
                        ResponseDateTime = DateTime.Now,
                        IsMerchantSelected = 1,
                        FINALPRICE = Convert.ToInt32(amount.Split('.')[0]),
                        IsPaymentDone = 1
                    };

                    responseMessage = await WebApiHelper.PostData("/api/Payment/UpdateRequestForDisCountToUserForPaymentDont", discountUpdateInfo);
                }

                TempData["SaveResponse"] = responseMessage;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the payment.");
                return StatusCode(500, "An internal server error occurred. Please try again later.");
            }
        }

        [HttpGet]
        public async Task<ActionResult> Payment(string rfdfu, string uid, string merchantId)
        {
            string userId = HttpContext.Session.GetString("UserId"); ; // Placeholder for session user ID retrieval

            try
            {
                var jsonResponse = await WebApiHelper.GetData($"/api/CategoryWithMerchant/AllResponsesFromMerchant?Uid={userId}");

                if (!string.IsNullOrEmpty(jsonResponse))
                {
                    int selectedServiceId = Convert.ToInt32(rfdfu);
                    List<RequestForDisCountToUserViewModel> categories = JsonConvert.DeserializeObject<List<RequestForDisCountToUserViewModel>>(jsonResponse);
                    ViewBag.ResponseForDisCountFromMerchant = categories.FirstOrDefault(x => x.RFDFU == selectedServiceId);
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
                    IsMerchantSelected = 1
                };

                string responseMessage = await WebApiHelper.PostData("/api/CategoryWithMerchant/SelectFinalMerchant", srbm);

                TempData["SaveResponse"] = responseMessage;
                return RedirectToAction("Index");
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
                    IsMerchantSelected = 0
                };

                string responseMessage = await WebApiHelper.PostData("/api/CategoryWithMerchant/DeSelectFinalMerchant", srbm);

                TempData["SaveResponse"] = responseMessage;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deselecting the final merchant.");
                return StatusCode(500, "An internal server error occurred. Please try again later.");
            }
        }

        [HttpGet]
        public async Task<ActionResult> UploadDocuments()
        {
            try
            {
                int UserId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));//Convert.ToInt32(HttpContext.Session.GetString("UserId"));

                var jsonResponse = await WebApiHelper.GetData("/api/FileUpload/GetFilesList");
                FileUploadViewModel model = new FileUploadViewModel();

                if (!string.IsNullOrEmpty(jsonResponse))
                {
                    List<UploadedFile> documentList = JsonConvert.DeserializeObject<List<UploadedFile>>(jsonResponse);
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
        public async Task<ActionResult> ReviewDocument(int userId)
        {
            try
            {
                var jsonResponse = await WebApiHelper.GetData($"/api/FileUpload/ReviewDocuments/{userId}");

                FileUploadViewModel model = new FileUploadViewModel();

                if (!string.IsNullOrEmpty(jsonResponse))
                {
                    List<UploadedFile> documentList = JsonConvert.DeserializeObject<List<UploadedFile>>(jsonResponse);
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
        public async Task<IActionResult> UploadDocuments(FileUploadViewModel model)
        {
            if (model == null || model.Files == null || model.Files.Count == 0)
            {
                _logger.LogWarning("Invalid model passed to UploadDocuments");
                return BadRequest("Invalid model.");
            }

            _logger.LogInformation("UploadDocuments called with model: {Model}", model);

            try
            {
                if (ModelState.IsValid)
                {
                    int loginUserId = 1; // Placeholder for session user ID retrieval
                    var username = $"Documents_{loginUserId}";

                    var folderPath = Path.Combine(_environment.WebRootPath, "uploads", username);
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    var fileUploadModelAPI = new FileUploadModelAPI
                    {
                        UserId = Convert.ToInt32(HttpContext.Session.GetString("UserId")),
                        UploadedFiles = new List<UploadedFile>()
                    };

                    foreach (var file in model.Files)
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
                                UserId = Convert.ToInt32(HttpContext.Session.GetString("UserId"))
                            };

                            fileUploadModelAPI.UploadedFiles.Add(uploadedFile);
                        }
                    }

                    var responseMessage = await WebApiHelper.PostData("/api/FileUpload/UploadFiles", fileUploadModelAPI);

                    TempData["SaveResponse"] = responseMessage;
                    return RedirectToAction("UploadDocuments");
                }

                return RedirectToAction("UploadDocuments");
            }
            catch (Exception ex)
            {
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

            var filePath = Path.Combine(_environment.WebRootPath, "uploads", folderName, fileName);
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
        public async Task<IActionResult> GetUsersWithDocuments()
        {
            int merchantId = 1; // Placeholder for session merchant ID retrieval

            try
            {
                var jsonResponse = await WebApiHelper.GetData("/api/FileUpload/UsersWithDocuments");

                UserDocumentsViewModel model = new UserDocumentsViewModel();

                if (!string.IsNullOrEmpty(jsonResponse))
                {
                    List<UserDocumentsViewModel> documentList = JsonConvert.DeserializeObject<List<UserDocumentsViewModel>>(jsonResponse);
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
                string responseMessage = await WebApiHelper.PostData($"/api/FileUpload/VerifyDocument/{documentId}", documentId);


                _logger.LogInformation($"Document verification response for DocumentId: {documentId}. Response : {responseMessage}");
                //TempData["SaveResponse"] = responseMessage;
                ViewBag.SaveResponse = responseMessage;
                return RedirectToAction("ReviewDocument", new { userId });
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
                string responseMessage = await WebApiHelper.PostData($"/api/FileUpload/DeleteDocument/{documentId}", documentId);
                _logger.LogInformation($"Document Deletion response for DocumentId: {documentId}. Response : {responseMessage}");
                //TempData["SaveResponse"] = responseMessage;
                ViewBag.SaveResponse = responseMessage;
                return RedirectToAction("UploadDocuments");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while verifying document for DocumentId: {DocumentId}", documentId);
                return StatusCode(500, "Internal server error.");
            }
        }

    }
    //public class RequestForDisCountToUserViewModel
    //{
    //    public int SID { get; set; }
    //    public int ServicePrice { get; set; }
    //    public string ServiceName { get; set; }
    //    public int MerchantID { get; set; }
    //    public decimal FINALPRICE { get; set; }
    //    public int UID { get; set; }
    //    public int RFDFU { get; set; }
    //    public int IsMerchantSelected { get; set; }
    //    public int IsPaymentDone { get; set; }
    //    public DateTime ResponseDateTime { get; set; }
    //}
    //public class FileUploadViewModel
    //{
    //    [Required]
    //    public IFormFileCollection Files { get; set; }
    //    public int UserId { get; set; }
    //    public List<UploadedFile>? UploadedFiles { get; set; }
    //}
    public class UserDocumentsViewModel
    {
        public int UserId { get; set; }
        public int MID { get; set; }
        public int DocumentCount { get; set; }
    }
    //public class FileUploadModelAPI
    //{

    //    public int UserId { get; set; }
    //    public List<UploadedFile>? UploadedFiles { get; set; }
    //}
}
