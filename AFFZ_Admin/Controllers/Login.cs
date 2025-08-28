
using AFFZ_Admin.Models;
using AFFZ_Admin.Models.Partial;
using AFFZ_Admin.Utils;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;

namespace AFFZ_Admin.Controllers
{
    public class Login : Controller
    {
        private readonly HttpClient _httpClient;

        public Login(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("Main");
        }

        public IActionResult Index()
        {
            return View("Login", new AdminLogin());
        }

        [HttpPost]
        public async Task<IActionResult> AdminLogin(AdminLogin model)
        {
            try
            {
                var response = await _httpClient.PostAsync("AdminUsers/Login", Customs.GetJsonContent(model));
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                SResponse sResponse = JsonConvert.DeserializeObject<SResponse>(responseString);

                if (sResponse.StatusCode == HttpStatusCode.OK)
                {
                    return RedirectToAction("Index", "Dashboard");
                }
                else
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Login");
            }
        }
        public IActionResult Logout()
        {
            try
            {
                //HttpContext.Session.Clear();
                return RedirectToAction("Index", "Login");
            }
            catch (Exception ex) { return null; }
        }
    }
}
