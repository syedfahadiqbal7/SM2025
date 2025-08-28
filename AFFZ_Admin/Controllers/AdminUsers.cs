using AFFZ_Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AFFZ_Admin.Controllers
{
    public class AdminUsers : Controller
    {
        private readonly HttpClient _httpClient;

        public AdminUsers(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("Main");
        }


        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _httpClient.GetAsync("AdminUsers");

                var responseString = await response.Content.ReadAsStringAsync();
                List<AdminUser> users = JsonConvert.DeserializeObject<List<AdminUser>>(responseString);
                return View("AdminUsers", users);

            }
            catch (Exception ex)
            {
                return View("Dashboard");
            }
        }
    }
}
