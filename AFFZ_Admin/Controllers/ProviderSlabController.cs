using AFFZ_Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AFFZ_Admin.Controllers
{
    public class ProviderSlabController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProviderSlabController> _logger;

        public ProviderSlabController(IHttpClientFactory httpClientFactory, ILogger<ProviderSlabController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("Main");
            _logger = logger;
        }

        public async Task<IActionResult> ProviderSlabIndex()
        {
            List<ProviderSlab> slabs = new List<ProviderSlab>();
            try
            {
                var response = await _httpClient.GetAsync("ProviderSlabs");
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    slabs = JsonConvert.DeserializeObject<List<ProviderSlab>>(responseString);
                }
                else
                {
                    TempData["FailMessage"] = "Failed to fetch provider slabs.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching provider slabs");
                TempData["FailMessage"] = "An error occurred while fetching provider slabs.";
            }

            return View(slabs);
        }

        public IActionResult ProviderSlabCreate()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProviderSlabCreate(ProviderSlab slab)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("ProviderSlabs", slab);
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Provider slab created successfully.";
                    return RedirectToAction("ProviderSlabIndex");
                }
                else
                {
                    TempData["FailMessage"] = "Failed to create provider slab.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating provider slab");
                TempData["FailMessage"] = "An error occurred while creating provider slab.";
            }

            return View(slab);
        }

        public async Task<IActionResult> ProviderSlabEdit(int id)
        {
            var response = await _httpClient.GetAsync($"ProviderSlabs/{id}");
            if (response.IsSuccessStatusCode)
            {
                var slab = JsonConvert.DeserializeObject<ProviderSlab>(await response.Content.ReadAsStringAsync());
                return View(slab);
            }
            TempData["FailMessage"] = "Failed to load provider slab.";
            return RedirectToAction("ProviderSlabIndex");
        }

        [HttpPost]
        public async Task<IActionResult> ProviderSlabEdit(ProviderSlab slab)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"ProviderSlabs/{slab.ProviderSlabID}", slab);
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Provider slab updated successfully.";
                    return RedirectToAction("ProviderSlabIndex");
                }
                else
                {
                    TempData["FailMessage"] = "Failed to update provider slab.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating provider slab");
                TempData["FailMessage"] = "An error occurred while updating provider slab.";
            }

            return View(slab);
        }

        public async Task<IActionResult> ProviderSlabDelete(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"ProviderSlabs/{id}");
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Provider slab deleted successfully.";
                }
                else
                {
                    TempData["FailMessage"] = "Failed to delete provider slab.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting provider slab");
                TempData["FailMessage"] = "An error occurred while deleting provider slab.";
            }

            return RedirectToAction("ProviderSlabIndex");
        }
    }
}
