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
    public class Profile : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _uploadPath;
        private readonly IDataProtector _protector;
        private readonly ILogger<Profile> _logger;

        public Profile(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IDataProtectionProvider provider,
            ILogger<Profile> logger)
        {
            _httpClient = httpClientFactory.CreateClient("Main");
            _uploadPath = configuration.GetValue<string>("ProfilePicStorage:UploadPath") ?? "Uploads";
            _protector = provider.CreateProtector("Example.SessionProtection");
            _logger = logger;
        }
        public async Task<IActionResult> Index()
        {
            try
            {
                string providerId = HttpContext.Session.GetEncryptedString("ProviderId", _protector);
                if (string.IsNullOrEmpty(providerId))
                {
                    _logger.LogWarning("ProviderId is missing from the session.");
                    return RedirectToAction("Login", "Account");
                }

                ProviderUser customer = await FetchProviderDataAsync(providerId);
                if (customer == null)
                {
                    _logger.LogWarning($"No provider found with ID: {providerId}");
                    return RedirectToAction("Error", "Home");
                }

                await PopulateMerchantDetails(providerId);
                ViewBag.ProviderId = providerId;



                // Fetch MerchantVerificationDocumentList
                var documentListResponse = await _httpClient.GetAsync($"Providers/FetchMerchantVerificationDocumentList");
                var documentListString = await documentListResponse.Content.ReadAsStringAsync();
                List<MerchantVerificationDocumentViewModel> documents = JsonConvert.DeserializeObject<List<MerchantVerificationDocumentViewModel>>(documentListString);

                // Fetch MerchantDocuments
                var merchantDocumentsResponse = await _httpClient.GetAsync($"Providers/GetMerchantDocs/{providerId}");
                var merchantDocumentsString = await merchantDocumentsResponse.Content.ReadAsStringAsync();
                List<MerchantDocuments> merchantDocuments = JsonConvert.DeserializeObject<List<MerchantDocuments>>(merchantDocumentsString);
                if (merchantDocuments == null || merchantDocuments.Count == 0)
                {
                    // Merge document list with status information
                    foreach (var document in documents)
                    {
                        var uploadedDoc = document.MerchantVerificationDocumentName;
                        document.Status = "Not Yet Verified";
                        document.FilePath = "";
                    }
                }
                else
                {
                    // Merge document list with status information
                    foreach (var document in documents)
                    {
                        var uploadedDoc = merchantDocuments.FirstOrDefault(md => md.FileName.Contains(document.MerchantVerificationDocumentName));
                        document.Status = uploadedDoc != null ? uploadedDoc.Status : "Not Yet Verified";
                        document.FilePath = uploadedDoc?.FolderName;
                        document.MDID = uploadedDoc == null ? 0 : uploadedDoc.MDID;
                    }
                }

                ViewBag.Documents = documents;

                return View("Profile", customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching document list.");
                return RedirectToAction("Login", "Account");
            }
        }
        // Action to update general profile information
        [HttpPost]
        public async Task<IActionResult> UpdateProfileInformation(ProviderUser model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid profile information submitted.");
                // Extract model errors into TempData
                var errorMessages = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                TempData["FailMessage"] = JsonConvert.SerializeObject(errorMessages);
                return RedirectToAction("Index"); // Redirect to the Index action to stay on /Profile
            }

            try
            {
                // Fetch existing data
                string providerId = HttpContext.Session.GetEncryptedString("ProviderId", _protector);
                if (string.IsNullOrEmpty(providerId))
                {
                    TempData["ErrorMessage"] = "Session expired. Please log in again.";
                    return RedirectToAction("Login", "Account");
                }

                var existingProviderUser = await FetchProviderDataAsync(providerId);
                if (existingProviderUser == null)
                {
                    TempData["ErrorMessage"] = "Failed to retrieve existing provider information.";
                    return RedirectToAction("Index");
                }

                // Check if any values have changed
                bool isModified = IsProviderUserModified(existingProviderUser, model);

                if (isModified)
                {
                    model.IsActive = false; // Deactivate user if values are modified
                    model.ModifyDate = DateTime.Now;

                    var response = await _httpClient.PostAsync("Providers/UpdateProfile", Customs.GetJsonContent(model));
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Profile updated successfully. Profile need to be re evaluate. Please wait till verification.";
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning($"Failed to update profile: {error}");
                        TempData["FailMessage"] = "Failed to update profile.";
                    }
                }
                else
                {
                    TempData["SuccessMessage"] = "No changes detected. Profile not updated.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating profile information.");
                TempData["FailMessage"] = "An error occurred while updating the profile.";
            }

            // Redirect to the Index action
            return RedirectToAction("Index");
        }

        // Action to update address information
        [HttpPost]
        public async Task<IActionResult> UpdateAddressInformation(ProviderUser model)
        {
            try
            {
                // Fetch existing data
                string providerId = HttpContext.Session.GetEncryptedString("ProviderId", _protector);
                if (string.IsNullOrEmpty(providerId))
                {
                    TempData["ErrorMessage"] = "Session expired. Please log in again.";
                    return RedirectToAction("Login", "Account");
                }

                var existingProviderUser = await FetchProviderDataAsync(providerId);
                if (existingProviderUser == null)
                {
                    TempData["ErrorMessage"] = "Failed to retrieve existing provider information.";
                    return RedirectToAction("Index");
                }

                // Check if any values have changed
                bool isModified = IsProviderUserAddressModified(existingProviderUser, model);

                if (isModified)
                {
                    model.IsActive = false; // Deactivate user if values are modified
                    model.ModifyDate = DateTime.Now;

                    var response = await _httpClient.PostAsync("Providers/UpdateProfile", Customs.GetJsonContent(model));
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Profile Address updated successfully. Profile need to be re evaluate. Please wait till verification.";
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning($"Failed to update Profile Address : {error}");
                        TempData["FailMessage"] = "Failed to update Profile Address.";
                    }
                }
                else
                {
                    TempData["SuccessMessage"] = "No changes detected. Profile Address not updated.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating Profile Address information.");
                TempData["FailMessage"] = "An error occurred while updating the Profile Address.";
            }

            return RedirectToAction("Index");
        }

        // Action to upload documents
        [HttpPost]
        public async Task<IActionResult> UploadDocuments(IFormFile file, string documentType)
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("Invalid file upload attempt.");
                TempData["ErrorMessage"] = "Please upload a valid file.";
                return RedirectToAction("Index");
            }

            try
            {
                string providerId = HttpContext.Session.GetEncryptedString("ProviderId", _protector);
                if (string.IsNullOrEmpty(providerId))
                {
                    _logger.LogWarning("ProviderId is missing from the session.");
                    TempData["ErrorMessage"] = "Session expired. Please log in again.";
                    return RedirectToAction("Login", "Account");
                }

                string providerDirectory = Path.Combine(_uploadPath, $"User_{providerId}");
                Directory.CreateDirectory(providerDirectory);

                string fileName = $"{documentType}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(file.FileName)}";
                string filePath = Path.Combine(providerDirectory, fileName);

                await using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                ProviderUser customer = await FetchProviderDataAsync(providerId);
                if (customer == null)
                {
                    TempData["ErrorMessage"] = "Failed to retrieve provider information.";
                    return RedirectToAction("Index");
                }
                if (!string.IsNullOrEmpty(customer.ProfilePicture) && System.IO.File.Exists(customer.ProfilePicture))
                {
                    System.IO.File.Delete(customer.ProfilePicture);
                    _logger.LogInformation($"Deleted existing file: {customer.ProfilePicture}");
                }
                // Update the corresponding field
                UpdateDocumentField(customer, documentType, filePath);

                customer.ModifyDate = DateTime.Now;
                customer.ModifiedBy = int.Parse(providerId);

                var response = await _httpClient.PostAsync("Providers/UpdateProfile", Customs.GetJsonContent(customer));
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = $"{documentType} uploaded successfully.";
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        HttpContext.Session.SetEncryptedString("ProfilePicturePath", filePath == null ? "" : filePath, _protector);
                    }
                }
                else
                {
                    _logger.LogWarning($"Failed to update profile after uploading {documentType}.");
                    TempData["ErrorMessage"] = "Failed to save the uploaded document.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while uploading {documentType}.");
                TempData["ErrorMessage"] = $"An error occurred while uploading {documentType}.";
            }

            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> UploadMerchantDocuments(IFormFile file, string documentMerchantType, int documentId)
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("Invalid file upload attempt.");
                TempData["ErrorMessage"] = "Please upload a valid file.";
                return RedirectToAction("Index");
            }

            try
            {
                string providerId = HttpContext.Session.GetEncryptedString("ProviderId", _protector);
                if (string.IsNullOrEmpty(providerId))
                {
                    _logger.LogWarning("ProviderId is missing from the session.");
                    TempData["ErrorMessage"] = "Session expired. Please log in again.";
                    return RedirectToAction("Index", "Account");
                }

                string providerDirectory = Path.Combine(_uploadPath, $"User_{providerId}");
                Directory.CreateDirectory(providerDirectory);

                string fileName = $"{documentMerchantType}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(file.FileName)}";
                string filePath = Path.Combine(providerDirectory, fileName);

                await using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                // Determine if this is a new document or an update to an existing one
                MerchantDocuments document;
                if (documentId > 0)
                {
                    // Fetch the existing document from the API

                    // Fetch MerchantDocuments
                    var fetchResponse = await _httpClient.GetAsync($"Providers/GetMerchantDocs/{providerId}");


                    if (!fetchResponse.IsSuccessStatusCode)
                    {
                        _logger.LogWarning($"Failed to fetch document with ID: {documentId}");
                        TempData["ErrorMessage"] = "Failed to retrieve existing document for update.";
                        return RedirectToAction("Index");
                    }

                    var existingDocumentString = await fetchResponse.Content.ReadAsStringAsync();
                    List<MerchantDocuments> merchantDocuments = JsonConvert.DeserializeObject<List<MerchantDocuments>>(existingDocumentString);
                    document = merchantDocuments.Where(x => x.MDID == documentId).FirstOrDefault();

                    if (document == null)
                    {
                        _logger.LogWarning($"Document with ID: {documentId} does not exist.");
                        TempData["ErrorMessage"] = "Document not found for update.";
                        return RedirectToAction("Index");
                    }
                    // Check and delete the existing file
                    if (!string.IsNullOrEmpty(document.FolderName) && System.IO.File.Exists(document.FolderName))
                    {
                        System.IO.File.Delete(document.FolderName);
                        _logger.LogInformation($"Deleted existing file: {document.FolderName}");
                    }
                    document.FileName = fileName;
                    document.ContentType = file.ContentType;
                    document.FileSize = file.Length;
                    document.FolderName = filePath;
                    document.Status = "Under Review";
                    document.DocumentAddedDate = DateTime.Now;
                    document.UploadedBy = providerId;
                    document.MDID = documentId;
                }
                else
                {
                    // Create a new document entry
                    document = new MerchantDocuments
                    {
                        FileName = fileName,
                        ContentType = file.ContentType,
                        FileSize = file.Length,
                        FolderName = filePath,
                        Status = "Under Review",
                        DocumentAddedDate = DateTime.Now,
                        MerchantId = int.Parse(providerId),
                        UploadedBy = providerId
                    };
                }

                // Save the document (either create or update)
                var saveResponse = await _httpClient.PostAsync("Providers/SaveMerchantDocument", Customs.GetJsonContent(document));
                if (saveResponse.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = $"{documentMerchantType} uploaded successfully.";
                }
                else
                {
                    _logger.LogWarning($"Failed to save the document {documentMerchantType}.");
                    TempData["ErrorMessage"] = "Failed to save the uploaded document.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while uploading {documentMerchantType}.");
                TempData["ErrorMessage"] = $"An error occurred while uploading {documentMerchantType}.";
            }

            return RedirectToAction("Index");
        }

        private async Task<ProviderUser?> FetchProviderDataAsync(string providerId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Providers/{providerId}");
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<ProviderUser>(responseString);
                }

                _logger.LogWarning($"Failed to fetch provider data for ID: {providerId}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching provider data for ID: {providerId}");
                return null;
            }
        }

        private void UpdateDocumentField(ProviderUser customer, string documentType, string filePath)
        {
            switch (documentType)
            {
                case "ProfilePicture":
                    customer.ProfilePicture = filePath;
                    break;
                case "EmiratesId":
                    customer.EmiratesId = filePath;
                    break;
                case "Passport":
                    customer.Passport = filePath;
                    break;
                case "DrivingLicense":
                    customer.DrivingLicense = filePath;
                    break;
                default:
                    throw new ArgumentException("Invalid document type.", nameof(documentType));
            }
        }

        private async Task PopulateMerchantDetails(string providerId)
        {
            try
            {
                Merchant merchant = new Merchant();
                var merchantResponse = await _httpClient.GetAsync($"Providers/GetByProvider/{providerId}");
                if (merchantResponse.IsSuccessStatusCode)
                {
                    var merchantResponseString = await merchantResponse.Content.ReadAsStringAsync();
                    var providerMerchant = JsonConvert.DeserializeObject<ProviderMerchant>(merchantResponseString);

                    ViewBag.MerchantStatus = providerMerchant?.IsActive == true ? "Active" : "Under Review";

                    if (providerMerchant != null)
                    {
                        var merchantDetailResponse = await _httpClient.GetAsync($"Providers/GetMerchantDetail/{Convert.ToInt32(providerId)}");
                        if (merchantDetailResponse.IsSuccessStatusCode)
                        {
                            var merchantDetailString = await merchantDetailResponse.Content.ReadAsStringAsync();
                            merchant = JsonConvert.DeserializeObject<Merchant>(merchantDetailString);
                            ViewBag.MerchantDetail = merchant;
                            // Define Merchant Locations

                        }
                    }
                }
                else
                {
                    ViewBag.MerchantStatus = "Not Linked";
                    ViewBag.MerchantDetail = new Merchant();
                }
                var locations = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Sharjah", Text = "Sharjah" },
                    new SelectListItem { Value = "Dubai", Text = "Dubai" },
                    new SelectListItem { Value = "Ras-al-khaimah", Text = "Ras-al-khaimah" },
                    new SelectListItem { Value = "Ajman", Text = "Ajman" },
                    new SelectListItem { Value = "Umm-al-quain", Text = "Umm-al-quain" },
                    new SelectListItem { Value = "AbuDhabhi", Text = "Abu Dhabi" },
                    new SelectListItem { Value = "Fujairah", Text = "Fujairah" }
                };

                ViewBag.Locations = new SelectList(locations, "Value", "Text", merchant.MerchantLocation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while populating merchant details.");
                ViewBag.MerchantStatus = "Error";
                ViewBag.MerchantDetail = new Merchant();
            }
        }
        // Action to link merchant
        [HttpPost]
        public async Task<IActionResult> LinkMerchant(Merchant model, int providerId, int MerchantId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Please fill in all required fields.";
                    return View("Index", model); // Pass the current model back to the view
                }

                model.ModifiedBy = providerId;
                model.CreatedBy = providerId;

                var response = await _httpClient.PostAsync(
                    $"Providers/ProviderMerchantLink?providerId={providerId}&MerchantId={MerchantId}",
                    Customs.GetJsonContent(model)
                );

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Merchant linked successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to link merchant.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> GetUserProfile(int providerId)
        {
            ProviderUser customer = null;
            var response = await _httpClient.GetAsync($"Providers/{providerId}");
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                customer = JsonConvert.DeserializeObject<ProviderUser>(responseString);
            }
            if (customer == null)
                return NotFound();

            customer.ProfilePicture = Url.Content($"~/uploads/profile_pictures/{customer.ProfilePicture}");
            customer.Passport = Url.Content($"~/uploads/passports/{customer.Passport}");
            customer.EmiratesId = Url.Content($"~/uploads/emirates_ids/{customer.EmiratesId}");
            customer.DrivingLicense = Url.Content($"~/uploads/driving_licenses/{customer.DrivingLicense}");

            return View(customer);
        }
        private bool IsProviderUserModified(ProviderUser existing, ProviderUser updated)
        {
            // Compare each field to determine if a change has occurred
            return !string.Equals(existing.FirstName, updated.FirstName, StringComparison.OrdinalIgnoreCase) ||
                   !string.Equals(existing.LastName, updated.LastName, StringComparison.OrdinalIgnoreCase) ||
                   !string.Equals(existing.Email, updated.Email, StringComparison.OrdinalIgnoreCase) ||
                   !string.Equals(existing.PhoneNumber, updated.PhoneNumber, StringComparison.OrdinalIgnoreCase) ||
                   !string.Equals(existing.ProviderName, updated.ProviderName, StringComparison.OrdinalIgnoreCase); // Compare other relevant fields as needed

        }
        private bool IsProviderUserAddressModified(ProviderUser existing, ProviderUser updated)
        {
            // Compare each field to determine if a change has occurred
            return !string.Equals(existing.Address, updated.Address, StringComparison.OrdinalIgnoreCase) ||
                   !string.Equals(existing.PostalCode, updated.PostalCode, StringComparison.OrdinalIgnoreCase);

        }
    }
}
