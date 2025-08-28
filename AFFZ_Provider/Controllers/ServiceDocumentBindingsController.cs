using AFFZ_Provider.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Net;

namespace AFFZ_Provider.Controllers
{
    [Authorize]
    public class ServiceDocumentBindingsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ServiceDocumentBindingsController> _logger;

        public ServiceDocumentBindingsController(IHttpClientFactory httpClientFactory, ILogger<ServiceDocumentBindingsController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("Main");
            _logger = logger;
        }

        // GET: api/ServiceDocumentListBindings
        public async Task<IActionResult> CategoryServiceIndex(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var response = await _httpClient.GetAsync($"ServiceDocumentListBindings/GetServiceDocumentListBindings?pageNumber={pageNumber}&pageSize={pageSize}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var bindings = JsonConvert.DeserializeObject<IEnumerable<M_SericeDocumentListBindingViewModel>>(responseString);
                    return View(bindings);
                }
                else
                {
                    _logger.LogWarning($"Failed to fetch service document bindings. Status code: {response.StatusCode}");
                    return View(new List<M_SericeDocumentListBindingViewModel>());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching service document bindings.");
                return View(new List<M_SericeDocumentListBindingViewModel>());
            }
        }

        // GET: api/ServiceDocumentListBindings/5
        public async Task<IActionResult> CategoryServiceDetails(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"ServiceDocumentListBindings/GetServiceDocumentListBindingByCategoryId?id={id}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var binding = JsonConvert.DeserializeObject<M_SericeDocumentListBindingViewModel>(responseString);
                    return PartialView("_CatServiceDetails", binding);
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogWarning($"Service document binding with ID {id} not found.");
                    return NotFound();
                }
                else
                {
                    _logger.LogWarning($"Failed to fetch service document binding. Status code: {response.StatusCode}");
                    return PartialView("_CatServiceDetails", new M_SericeDocumentListBindingViewModel());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching service document binding details.");
                return View(new M_SericeDocumentListBindingViewModel());
            }
        }
        [HttpGet]
        public async Task<IActionResult> CategoryServiceCreate()
        {
            try
            {
                ViewBag.ServiceListComboData = await ServiceListItems(0);
                ViewBag.ServiceCategoryComboData = await ServiceCategoryItems(0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing service creation page.");
                ModelState.AddModelError(string.Empty, "Failed to load data.");
            }
            return View();
        }
        // POST: api/ServiceDocumentListBindings
        [HttpPost]
        public async Task<IActionResult> CategoryServiceCreate(M_SericeDocumentListBinding binding)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ServiceListComboData = await ServiceListItems(0);
                ViewBag.ServiceCategoryComboData = await ServiceCategoryItems(0);
                return View(binding);
            }

            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(binding), System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("ServiceDocumentListBindings/CreateServiceDocumentListBinding", content);

                if (response.StatusCode == HttpStatusCode.Created)
                {
                    TempData["SuccessMessage"] = "Binding Successfully Done.";
                    return RedirectToAction(nameof(CategoryServiceIndex));
                }
                else
                {
                    ViewBag.ServiceListComboData = await ServiceListItems(0);
                    ViewBag.ServiceCategoryComboData = await ServiceCategoryItems(0);
                    _logger.LogWarning($"Failed to create service document binding. Status code: {response.StatusCode}");
                    TempData["FailMessage"] = await response.Content.ReadAsStringAsync();

                    return View(binding);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the service document binding.");
                return View(binding);
            }
        }
        [HttpGet]
        public async Task<IActionResult> CategoryServiceUpdate(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"ServiceDocumentListBindings/GetBindingById?id={id}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var binding = JsonConvert.DeserializeObject<M_SericeDocumentListBinding>(responseString);
                    ViewBag.ServiceListComboData = await ServiceListItems(binding.ServiceDocumentListId);
                    ViewBag.ServiceCategoryComboData = await ServiceCategoryItems(binding.CategoryID);
                    return View(binding);
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogWarning($"Service document binding with ID {id} not found.");
                    return NotFound();
                }
                else
                {
                    _logger.LogWarning($"Failed to fetch service document binding. Status code: {response.StatusCode}");
                    return View(new M_SericeDocumentListBindingViewModel());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching service document binding details.");
                return View(new M_SericeDocumentListBindingViewModel());
            }
        }
        // POST: api/ServiceDocumentListBindings/Update
        [HttpPost]
        public async Task<IActionResult> CategoryServiceUpdate(int id, M_SericeDocumentListBinding binding)
        {
            if (id != binding.Id || !ModelState.IsValid)
            {
                return BadRequest();
            }

            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(binding), System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"ServiceDocumentListBindings/UpdateServiceDocumentListBinding?Id={id}", content);

                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    return RedirectToAction(nameof(CategoryServiceIndex));
                }
                else
                {
                    _logger.LogWarning($"Failed to update service document binding with ID {id}. Status code: {response.StatusCode}");
                    return View(binding);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the service document binding.");
                return View(binding);
            }
        }
        [HttpGet]
        public async Task<IActionResult> CategoryServiceDeleteGet(int id)
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the service document binding.");
                return NotFound();
            }
        }
        // GET: api/ServiceDocumentListBindings/Delete/5
        [HttpGet]
        public async Task<IActionResult> CategoryServiceDeleteConfirmed(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"ServiceDocumentListBindings/DeleteServiceDocumentListBinding?id={id}");
                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    return RedirectToAction(nameof(CategoryServiceIndex));
                }
                else
                {
                    _logger.LogWarning($"Failed to delete service document binding with ID {id}. Status code: {response.StatusCode}");
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the service document binding.");
                return NotFound();
            }
        }
        private async Task<List<SelectListItem>> ServiceListItems(int? id)
        {
            var jsonResponse = await _httpClient.GetAsync($"ServicesList/GetServicesList");
            var responseString = await jsonResponse.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(responseString))
            {
                try
                {
                    List<ServicesList> xerviceList = JsonConvert.DeserializeObject<List<ServicesList>>(responseString);
                    return xerviceList.Select(i => new SelectListItem
                    {
                        Text = i.ServiceName,
                        Value = i.ServiceListID.ToString(),
                        Selected = (id == i.ServiceListID ? true : false)
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
    public class M_SericeDocumentListBindingViewModel
    {
        public int Id { get; set; }
        public string serviceDocumentListId { get; set; }
        public string categoryID { get; set; }
    }
}
