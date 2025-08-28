using AFFZ_Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AFFZ_Admin.Controllers
{
    public class MembershipPlanController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MembershipPlanController> _logger;

        public MembershipPlanController(IHttpClientFactory httpClientFactory, ILogger<MembershipPlanController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("Main");
            _logger = logger;
        }

        public async Task<IActionResult> MembershipPlanIndex()
        {
            List<MembershipPlan> plans = new List<MembershipPlan>();
            try
            {
                var response = await _httpClient.GetAsync("MembershipPlans");
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    plans = JsonConvert.DeserializeObject<List<MembershipPlan>>(responseString);
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

        public async Task<IActionResult> MembershipPlanCreate()
        {
            try
            {
                var response = await _httpClient.GetAsync("Slabs");
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var slabs = JsonConvert.DeserializeObject<List<Slab>>(responseString);
                    ViewBag.SlabList = slabs;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching slabs for membership plan.");
                TempData["FailMessage"] = "An error occurred while loading slabs.";
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> MembershipPlanCreate(MembershipPlan plan)
        {
            if (!ModelState.IsValid)
            {
                TempData["FailMessage"] = "Invalid data.";
                return View(plan);
            }

            try
            {
                // Step 1: Create the membership plan and get the plan ID
                var response = await _httpClient.PostAsJsonAsync("MembershipPlans/CreatePlan", plan);
                if (response.IsSuccessStatusCode)
                {
                    var planId = await response.Content.ReadAsStringAsync();

                    // Step 2: Add slabs to the created plan
                    var _slabResponse = await _httpClient.PostAsJsonAsync($"MembershipPlans/AddSlabsToPlan/{planId}", plan.SelectedSlabIds);
                    if (_slabResponse.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Membership plan created successfully.";
                        return RedirectToAction("MembershipPlanIndex");
                    }
                    else
                    {
                        TempData["FailMessage"] = "Failed to add slabs to the membership plan.";
                    }
                }
                else
                {
                    TempData["FailMessage"] = "Failed to create membership plan.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating membership plan.");
                TempData["FailMessage"] = "An error occurred while creating the membership plan.";
            }

            // Reload slabs in case of failure
            var slabResponse = await _httpClient.GetAsync("ProviderSlabs");
            if (slabResponse.IsSuccessStatusCode)
            {
                var responseString = await slabResponse.Content.ReadAsStringAsync();
                ViewBag.SlabList = JsonConvert.DeserializeObject<List<Slab>>(responseString);
            }

            return View(plan);
        }

        public async Task<IActionResult> MembershipPlanEdit(int id)
        {
            var response = await _httpClient.GetAsync($"MembershipPlans/{id}");
            if (response.IsSuccessStatusCode)
            {
                var plan = JsonConvert.DeserializeObject<MembershipPlan>(await response.Content.ReadAsStringAsync());

                // Populate SelectedSlabIds from the MembershipPlanSlabs
                plan.SelectedSlabIds = plan.MembershipPlanSlabs.Select(s => s.SlabId).ToList();

                var slabResponse = await _httpClient.GetAsync("Slabs");
                if (slabResponse.IsSuccessStatusCode)
                {
                    var slabs = JsonConvert.DeserializeObject<List<Slab>>(await slabResponse.Content.ReadAsStringAsync());
                    ViewBag.SlabList = slabs;
                }

                return View(plan);
            }

            TempData["FailMessage"] = "Failed to load membership plan.";
            return RedirectToAction("MembershipPlanIndex");
        }


        [HttpPost]
        public async Task<IActionResult> MembershipPlanEdit(MembershipPlan plan)
        {
            if (!ModelState.IsValid)
            {
                TempData["FailMessage"] = "Invalid data.";
                return View(plan);
            }

            try
            {
                // Step 1: Update the membership plan
                var updateResponse = await _httpClient.PostAsJsonAsync($"MembershipPlans/{plan.Id}", plan);
                if (updateResponse.IsSuccessStatusCode)
                {
                    // Step 2: Update slabs for the plan
                    var slabResponse = await _httpClient.PostAsJsonAsync($"MembershipPlans/AddSlabsToPlan/{plan.Id}", plan.SelectedSlabIds);
                    if (slabResponse.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Membership plan updated successfully.";
                        return RedirectToAction("MembershipPlanIndex");
                    }
                }

                TempData["FailMessage"] = "Failed to update membership plan.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating membership plan.");
                TempData["FailMessage"] = "An error occurred while updating the membership plan.";
            }

            return View(plan);
        }


        public async Task<IActionResult> MembershipPlanDelete(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"MembershipPlans/{id}");
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Membership plan deleted successfully.";
                }
                else
                {
                    TempData["FailMessage"] = "Failed to delete membership plan.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting membership plan");
                TempData["FailMessage"] = "An error occurred while deleting membership plan.";
            }

            return RedirectToAction("MembershipPlanIndex");
        }
    }
}
