using AFFZ_Provider.Models;
using AFFZ_Provider.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace AFFZ_Provider.Controllers
{
    public class SignUp : Controller
    {
        private readonly HttpClient _httpClient;

        public SignUp(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("Main");
        }

        public IActionResult Index()
        {
            return View("SignUp", new ProviderUser());
        }

        [HttpPost]
        public async Task<IActionResult> ProvidersRegister(ProviderUser model)
        {
            try
            {
                var response = await _httpClient.PostAsync("Providers/Register", Customs.GetJsonContent(model));
                var responseString = await response.Content.ReadAsStringAsync();

                // Deserialize the response to extract the message
                var jsonResponse = JsonSerializer.Deserialize<ResponseModel>(responseString);

                if (response.StatusCode == HttpStatusCode.OK && jsonResponse != null && jsonResponse.statusCode == 200)
                {
                    TempData["SuccessMessage"] = jsonResponse.message;
                    return RedirectToAction("Index", "Login");
                }
                else
                {
                    TempData["FailMessage"] = jsonResponse?.message ?? "An error occurred.";
                    return View("Signup");
                }
            }
            catch (Exception ex)
            {
                TempData["FailMessage"] = ex.Message;
                return View("Signup");
            }
        }
    }
    public class ResponseModel
    {
        public int statusCode { get; set; }
        public string message { get; set; }
        public object data { get; set; } // Use a specific type if the data field has a defined structure
    }
}
