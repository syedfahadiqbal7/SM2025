using AFFZ_Admin.Models;
using AFFZ_Admin.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Net;

namespace AFFZ_Admin.Controllers
{
    public class ServicesListController : Controller
    {
        private readonly HttpClient _httpClient;
        private string BaseUrl = string.Empty;
        private string PublicDomain = string.Empty;
        private string ApiHttpsPort = string.Empty;
        private string MerchantHttpsPort = string.Empty;
        private ILogger<ServicesListController> _logger;

        public ServicesListController(IHttpClientFactory httpClientFactory, ILogger<ServicesListController> logger, IAppSettingsService service)
        {
            _httpClient = httpClientFactory.CreateClient("Main");
            BaseUrl = service.GetBaseIpAddress();
            PublicDomain = service.GetPublicDomain();
            ApiHttpsPort = service.GetApiHttpsPort();
            MerchantHttpsPort = service.GetMerchantHttpsPort();
            _logger = logger;
        }

        // GET: ServicesList
        public async Task<IActionResult> ServicesListIndex()
        {
            try
            {
                //var servicesList = await _httpClient.GetFromJsonAsync<List<ServicesListViewModel>>($"ServicesList/GetServicesList");
                var response = await _httpClient.GetAsync("ServicesList/GetServicesList");
                var responseString = await response.Content.ReadAsStringAsync();
                var servicesList = JsonConvert.DeserializeObject<List<ServiceWithCategoryBinding>>(responseString);
                ViewBag.APILink = _httpClient.BaseAddress;
                ViewBag.MerchantLink = $"{Request.Scheme}://{PublicDomain}:{MerchantHttpsPort}/";
                return View(servicesList);
            }
            catch (Exception)
            {

                throw;
            }
        }

        // GET: ServicesList/Create
        public async Task<IActionResult> ServicesListCreate()
        {
            ViewBag.ServiceCategoryComboData = await ServiceCategoryItems(0);
            return PartialView("ServicesListCreate");
            //return View();
        }

        // POST: ServicesList/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ServicesListCreate(ServicesListViewModel model, int CategoryID)
        {
            try
            {
                int NewServiceId = 0;
                if (ModelState.IsValid)
                {
                    if (model.UploadedImage != null)
                    {
                        // Save the uploaded image
                        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets/img/services");
                        Directory.CreateDirectory(uploadsFolder); // Ensure the folder exists
                        // Sanitize the ServiceName to ensure it is a valid file name
                        string sanitizedServiceName = string.Concat(model.ServiceName.Split(Path.GetInvalidFileNameChars()));
                        string fileExtension = Path.GetExtension(model.UploadedImage.FileName);
                        string uniqueFileName = $"{sanitizedServiceName}{fileExtension}";
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.UploadedImage.CopyToAsync(stream);
                        }

                        model.ServiceImage = "/assets/img/services/" + uniqueFileName; // Save relative path
                    }

                    // Send data to the API
                    var response = await _httpClient.PostAsJsonAsync("ServicesList/PostServicesList", model);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseString = await response.Content.ReadAsStringAsync();
                        var servicesList = JsonConvert.DeserializeObject<ServicesListViewModel>(responseString);
                        M_SericeDocumentListBinding binding = new M_SericeDocumentListBinding();
                        try
                        {
                            binding.CategoryID = CategoryID;
                            binding.ServiceDocumentListId = servicesList.ServiceListID;
                            var content = new StringContent(JsonConvert.SerializeObject(binding), System.Text.Encoding.UTF8, "application/json");
                            response = await _httpClient.PostAsync("ServiceDocumentListBindings/CreateServiceDocumentListBinding", content);

                            if (response.StatusCode == HttpStatusCode.Created)
                            {
                                TempData["SuccessMessage"] = "Binding Successfully Done.";
                                return RedirectToAction(nameof(ServicesListIndex));
                            }
                            else
                            {
                                ViewBag.ServiceCategoryComboData = await ServiceCategoryItems(0);
                                _logger.LogWarning($"Failed to create service document binding. Status code: {response.StatusCode}");
                                TempData["FailMessage"] = await response.Content.ReadAsStringAsync();

                                return View(binding);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "An error occurred while creating the service and Category binding.");
                            return View(binding);
                        }
                    }

                }
                else
                {
                    ViewBag.ServiceCategoryComboData = await ServiceCategoryItems(0);
                    return View(model);
                }
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service list");
                throw;
            }
        }
        // GET: ServicesList/Edit/5
        public async Task<IActionResult> ServicesListEdit(int id)
        {
            try
            {
                var service = await _httpClient.GetFromJsonAsync<ServicesListViewModel>($"ServicesList/GetServicesListById?id={id}");
                if (service == null)
                    return NotFound();
                try
                {
                    var response = await _httpClient.GetAsync($"ServiceDocumentListBindings/GetBindingByServiceId?id={id}");
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var responseString = await response.Content.ReadAsStringAsync();
                        var binding = JsonConvert.DeserializeObject<M_SericeDocumentListBinding>(responseString);
                        ViewBag.ServiceCategoryComboData = await ServiceCategoryItems(binding.CategoryID);
                        ViewBag.ServiceListBindID = binding.Id;
                        return PartialView("ServicesListEdit", service);
                    }
                    else if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        _logger.LogWarning($"Service document binding with ID {id} not found.");
                        ViewBag.ServiceCategoryComboData = await ServiceCategoryItems(0);
                        ViewBag.ServiceListBindID = 0;
                        return PartialView("ServicesListEdit", service);
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to fetch service document binding. Status code: {response.StatusCode}");
                        return NotFound();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while fetching service document binding details.");
                    return RedirectToAction(nameof(ServicesListIndex));
                }

                //return View(service);
            }
            catch (Exception)
            {
                throw;
            }
        }
        // POST: ServicesList/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ServicesListEdit(int ServiceListID, ServicesListViewModel model, int CategoryID, int ServiceListBindID, string ServiceImage)
        {
            try
            {
                if (ServiceListID != model.ServiceListID)
                    return BadRequest();

                if (ModelState.IsValid)
                {
                    if (model.UploadedImage != null)
                    {
                        // Save the new uploaded image
                        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets/img/services");
                        Directory.CreateDirectory(uploadsFolder); // Ensure the folder exists
                        // Sanitize the ServiceName to ensure it is a valid file name
                        string sanitizedServiceName = string.Concat(model.ServiceName.Split(Path.GetInvalidFileNameChars()));
                        string fileExtension = Path.GetExtension(model.UploadedImage.FileName);
                        string uniqueFileName = $"{sanitizedServiceName}{fileExtension}";
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.UploadedImage.CopyToAsync(stream);
                        }

                        model.ServiceImage = "/assets/img/services/" + uniqueFileName; // Save relative path
                    }
                    else
                    {
                        model.ServiceImage = ServiceImage;
                    }
                    // Send data to the API
                    var response = await _httpClient.PostAsJsonAsync($"ServicesList/UpdateServicesList?id={ServiceListID}", model);
                    if (response.IsSuccessStatusCode)
                    {
                        try
                        {
                            M_SericeDocumentListBinding binding = new M_SericeDocumentListBinding();
                            binding.ServiceDocumentListId = model.ServiceListID;
                            binding.CategoryID = CategoryID;
                            binding.Id = ServiceListBindID;
                            var content = new StringContent(JsonConvert.SerializeObject(binding), System.Text.Encoding.UTF8, "application/json");
                            if (ServiceListBindID == 0)
                            {
                                response = await _httpClient.PostAsync("ServiceDocumentListBindings/CreateServiceDocumentListBinding", content);
                                if (response.StatusCode == HttpStatusCode.Created)
                                {
                                    TempData["SuccessMessage"] = "Binding Successfully Done.";
                                    return RedirectToAction(nameof(ServicesListIndex));
                                }
                                else
                                {
                                    ViewBag.ServiceCategoryComboData = await ServiceCategoryItems(0);
                                    _logger.LogWarning($"Failed to create service document binding. Status code: {response.StatusCode}");
                                    TempData["FailMessage"] = await response.Content.ReadAsStringAsync();

                                    return View(binding);
                                }
                            }
                            else
                            {
                                response = await _httpClient.PostAsync($"ServiceDocumentListBindings/UpdateServiceDocumentListBinding?Id={ServiceListBindID}", content);
                                if (response.StatusCode == HttpStatusCode.NoContent)
                                {
                                    return RedirectToAction(nameof(ServicesListIndex));
                                }
                                else
                                {
                                    _logger.LogWarning($"Failed to update service document binding with ID {ServiceListBindID}. Status code: {response.StatusCode}");
                                    ViewBag.ServiceCategoryComboData = await ServiceCategoryItems(binding.CategoryID);
                                    ViewBag.ServiceListBindID = binding.Id;
                                    return PartialView("ServicesListEdit", model);
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "An error occurred while updating the service document binding.");
                            return RedirectToAction(nameof(ServicesListIndex));
                        }
                    }

                }
                return PartialView("ServicesListEdit", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating service list");
                throw;
            }
        }
        // GET: ServicesList/Delete/5
        public async Task<IActionResult> ServicesListDelete(int id)
        {
            try
            {
                var service = await _httpClient.GetFromJsonAsync<ServicesListViewModel>($"ServicesList/GetServicesListById?id={id}");
                if (service == null)
                    return NotFound();
                return PartialView("ServicesListDelete", service);
                //return View(service);
            }
            catch (Exception)
            {

                throw;
            }
        }

        // POST: ServicesList/Delete/5
        [HttpPost, ActionName("ServicesListDeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ServicesListDeleteConfirmed(int ServiceListID)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"ServicesList/DeleteServicesList?id={ServiceListID}");
                if (response.IsSuccessStatusCode)
                {
                    response = await _httpClient.GetAsync($"ServiceDocumentListBindings/DeleteServiceandCategoryBinding?id={ServiceListID}");
                    if (response.StatusCode == HttpStatusCode.NoContent)
                    {
                        return RedirectToAction(nameof(ServicesListIndex));
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to delete service document binding with ID {ServiceListID}. Status code: {response.StatusCode}");
                        return NotFound();
                    }
                }
                return RedirectToAction(nameof(ServicesListIndex));


            }
            catch (Exception)
            {

                throw;
            }
        }
        private async Task<List<SelectListItem>> ServiceCategoryItems(int? id)
        {
            var jsonResponse = await _httpClient.GetAsync($"ServiceCategory/GetAllServiceCategories");
            var responseString = await jsonResponse.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(responseString))
            {
                try
                {
                    List<ServiceCategory> xerviceList = JsonConvert.DeserializeObject<List<ServiceCategory>>(responseString);
                    return xerviceList.Select(i => new SelectListItem
                    {
                        Text = i.CategoryName,
                        Value = i.CategoryId.ToString(),
                        Selected = (id == i.CategoryId ? true : false)
                    }).ToList();
                }
                catch (JsonSerializationException ex)
                {
                    // Log the exception details
                    _logger.LogError(ex, "JSON deserialization error.");
                    // Handle the error response accordingly
                    ModelState.AddModelError(string.Empty, "Failed to load Data.");
                    return new List<SelectListItem>();
                }
            }
            else
            {
                return new List<SelectListItem>();
            }
        }
    }
    public class M_SericeDocumentListBinding
    {
        public int Id { get; set; } // Consider adding a primary key if not present
        public int? CategoryID { get; set; }
        public int? ServiceDocumentListId { get; set; }
    }
    public class ServiceWithCategoryBinding
    {
        public int ServiceListID { get; set; }
        public string? ServiceName { get; set; }
        public string? ServiceImage { get; set; }
        public string? CategoryName { get; set; }
        public int CategoryID { get; set; }
        public int ServiceCategoryBindingId { get; set; }
        public string? BindingStatus { get; set; } // To store the "No binding found" message
        public string? CategoryStatus { get; set; } // To store the "No category associated" message
    }
}
