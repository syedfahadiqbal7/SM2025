using AFFZ_Admin.Models;
using AFFZ_Admin.Utils;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AFFZ_Admin.Controllers
{
    public class ServiceCategoryController : Controller
    {
        private readonly HttpClient _httpClient;
        private string BaseUrl = string.Empty;
        private string PublicDomain = string.Empty;
        private string ApiHttpsPort = string.Empty;
        private string MerchantHttpsPort = string.Empty;
        private ILogger<ServiceCategoryController> _logger;

        public ServiceCategoryController(IHttpClientFactory httpClientFactory, ILogger<ServiceCategoryController> logger, IAppSettingsService service)
        {
            _httpClient = httpClientFactory.CreateClient("Main");
            BaseUrl = service.GetBaseIpAddress();
            PublicDomain = service.GetPublicDomain();
            ApiHttpsPort = service.GetApiHttpsPort();
            MerchantHttpsPort = service.GetMerchantHttpsPort();
            _logger = logger;
        }

        // GET: ServiceCategory
        public async Task<IActionResult> CategoryIndex()
        {
            var response = await _httpClient.GetAsync("ServiceCategory/GetAllServiceCategories");
            var responseString = await response.Content.ReadAsStringAsync();
            var categories = JsonConvert.DeserializeObject<List<ServiceCategoryViewModel>>(responseString);
            ViewBag.APILink = _httpClient.BaseAddress;
            ViewBag.MerchantLink = $"{Request.Scheme}://{PublicDomain}:{MerchantHttpsPort}/";
            return View(categories);
        }

        // GET: ServiceCategory/Create
        public IActionResult CategoryCreate()
        {
            return View();
        }

        // POST: ServiceCategory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CategoryCreate(ServiceCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var response = await _httpClient.PostAsJsonAsync($"ServiceCategory/PostServiceCategory", model);
                if (response.IsSuccessStatusCode)
                    return RedirectToAction(nameof(CategoryIndex));
            }
            return View(model);
        }

        // GET: ServiceCategory/Edit/5
        public async Task<IActionResult> CategoryEdit(int id)
        {
            var response = await _httpClient.GetAsync($"ServiceCategory/GetServiceCategoryById?id={id}");
            var responseString = await response.Content.ReadAsStringAsync();
            var category = JsonConvert.DeserializeObject<ServiceCategoryViewModel>(responseString);

            if (category == null)
                return NotFound();

            return View(category);
        }

        // POST: ServiceCategory/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CategoryEdit(int id, ServiceCategoryViewModel model)
        {
            if (id != model.CategoryId)
                return BadRequest();

            if (ModelState.IsValid)
            {
                var response = await _httpClient.PostAsJsonAsync($"ServiceCategory/UpdateServiceCategory?id={id}", model);
                if (response.IsSuccessStatusCode)
                    return RedirectToAction(nameof(CategoryIndex));
            }
            return View(model);
        }

        // GET: ServiceCategory/Delete/5
        public async Task<IActionResult> CategoryDelete(int id)
        {
            var response = await _httpClient.GetAsync($"ServiceCategory/GetServiceCategoryById?id={id}");
            var responseString = await response.Content.ReadAsStringAsync();
            var category = JsonConvert.DeserializeObject<ServiceCategoryViewModel>(responseString);
            if (category == null)
                return NotFound();

            return View(category);
        }

        // POST: ServiceCategory/Delete/5
        [HttpPost, ActionName("CategoryDeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CategoryDeleteConfirmed(int CategoryId)
        {
            var response = await _httpClient.PostAsync($"ServiceCategory/DeleteServiceCategory?id={CategoryId}", null);
            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(CategoryIndex));

            return RedirectToAction(nameof(CategoryIndex));
        }
    }
}
