using AFFZ_Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
namespace AFFZ_Admin.Controllers
{
    public class SlabController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SlabController> _logger;

        public SlabController(IHttpClientFactory httpClientFactory, ILogger<SlabController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("Main");
            _logger = logger;
        }

        public async Task<IActionResult> SlabIndex()
        {
            List<Slab> plans = new List<Slab>();
            try
            {
                var response = await _httpClient.GetAsync("Slabs");
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    plans = JsonConvert.DeserializeObject<List<Slab>>(responseString);
                }
                else
                {
                    TempData["FailMessage"] = "Failed to fetch membership plans.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching membership plans");
                TempData["FailMessage"] = "An error occurred while fetching membership plans.";
            }

            return View(plans);
        }

        public IActionResult SlabCreate()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SlabCreate(Slab plan)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("Slabs", plan);
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Membership plan created successfully.";
                    return RedirectToAction("SlabIndex");
                }
                else
                {
                    TempData["FailMessage"] = "Failed to create membership plan.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating membership plan");
                TempData["FailMessage"] = "An error occurred while creating membership plan.";
            }

            return View(plan);
        }

        public async Task<IActionResult> SlabEdit(int id)
        {
            var response = await _httpClient.GetAsync($"Slabs/GetSlabs?id={id}");
            if (response.IsSuccessStatusCode)
            {
                var plan = JsonConvert.DeserializeObject<Slab>(await response.Content.ReadAsStringAsync());
                return View(plan);
            }
            TempData["FailMessage"] = "Failed to load slab.";
            return RedirectToAction("SlabIndex");
        }

        [HttpPost]
        public async Task<IActionResult> SlabEdit(Slab plan)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"Slabs/{plan.SlabID}", plan);
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Slab updated successfully.";
                    return RedirectToAction("SlabIndex");
                }
                else
                {
                    TempData["FailMessage"] = "Failed to update Slab.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Slab");
                TempData["FailMessage"] = "An error occurred while updating Slab.";
            }

            return View(plan);
        }

        public async Task<IActionResult> SlabDelete(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Slabs/DeleteSlab?id={id}");
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Slab deleted successfully.";
                }
                else
                {
                    TempData["FailMessage"] = "Failed to delete Slab.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Slab");
                TempData["FailMessage"] = "An error occurred while deleting Slab.";
            }

            return RedirectToAction("SlabIndex");
        }
    }
}
