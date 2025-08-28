using AFFZ_Admin.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AFFZ_Admin.Controllers
{
    public class Roles : Controller
    {
        private readonly HttpClient _httpClient;

        public Roles(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("Main");
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _httpClient.GetAsync("Roles");
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                List<Role> rolesList = JsonSerializer.Deserialize<List<Role>>(responseString);

                return View("Roles", rolesList);
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }
    }
}
