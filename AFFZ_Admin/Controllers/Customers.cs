using AFFZ_Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AFFZ_Admin.Controllers
{
    public class Customers : Controller
    {

        private readonly HttpClient _httpClient;

        public Customers(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("Main");
        }


        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _httpClient.GetAsync("Customers");

                var responseString = await response.Content.ReadAsStringAsync();
                List<Customer> customers = JsonConvert.DeserializeObject<List<Customer>>(responseString);
                return View("Customers", customers);

            }
            catch (Exception ex)
            {
                return View("Dashboard");
            }
        }
    }
}
