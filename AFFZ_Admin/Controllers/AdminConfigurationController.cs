using AFFZ_Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AFFZ_Admin.Controllers
{
    public class AdminConfigurationController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AdminConfigurationController> _logger;

        public AdminConfigurationController(IHttpClientFactory httpClientFactory, ILogger<AdminConfigurationController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("Main");
            _logger = logger;
        }

        public async Task<IActionResult> SettingIndex()
        {
            ConfigurationSettings setting = null;
            try
            {
                var response = await _httpClient.GetAsync("ConfigurationSettings");
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    setting = JsonConvert.DeserializeObject<ConfigurationSettings>(responseString);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching configuration settings.");
                TempData["FailMessage"] = "An error occurred while loading settings.";
            }
            return View(setting);
        }

        [HttpPost]
        public async Task<IActionResult> Update(ConfigurationSettings setting)
        {
            if (!ModelState.IsValid)
            {
                TempData["FailMessage"] = "Invalid data.";
                return View("Index", setting);
            }

            try
            {
                var response = await _httpClient.PutAsJsonAsync($"ConfigurationSettings/{setting.Id}", setting);
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Configuration settings updated successfully.";
                }
                else
                {
                    TempData["FailMessage"] = "Failed to update configuration settings.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating configuration settings.");
                TempData["FailMessage"] = "An error occurred while updating settings.";
            }

            return RedirectToAction("SettingIndex");
        }
    }
}
