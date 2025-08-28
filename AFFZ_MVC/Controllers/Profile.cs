using AFFZ_Customer.Models;
using AFFZ_Customer.Utils;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;

namespace AFFZ_Customer.Controllers
{
    public class Profile : Controller
    {
        private readonly HttpClient _httpClient;
        string _uploadPath;
        IDataProtector _protector;

        public Profile(IHttpClientFactory httpClientFactory, IConfiguration configuration, IDataProtectionProvider provider)
        {
            _httpClient = httpClientFactory.CreateClient("Main");
            _uploadPath = configuration.GetValue<string>("ProfilePicStorage:UploadPath");

            _protector = provider.CreateProtector("Example.SessionProtection");
        }

        public async Task<IActionResult> Index()
        {

            string userId = HttpContext.Session.GetEncryptedString("UserId", _protector);
            var response = await _httpClient.GetAsync($"Customers/{userId}");

            var responseString = await response.Content.ReadAsStringAsync();
            Customers customer = JsonConvert.DeserializeObject<Customers>(responseString);
            ViewBag.CustomerId = HttpContext.Session.GetEncryptedString("UserId", _protector);
            ViewBag.FirstName = HttpContext.Session.GetEncryptedString("FirstName", _protector);
            ViewBag.MemberSince = HttpContext.Session.GetEncryptedString("MemberSince", _protector);
            return View("Profile", customer);
        }


        [HttpPost]
        public async Task<IActionResult> UpdateProfile(Customers model)
        {
            try
            {
                string filePath = string.Empty;
                string fileName = string.Empty;
                if (model.ProfileImage != null)
                {
                    string userId = HttpContext.Session.GetEncryptedString("UserId", _protector);
                    Directory.CreateDirectory(_uploadPath); // Ensure the directory exists
                    fileName = Guid.NewGuid().ToString() + "_" + model.ProfileImage.FileName;
                    fileName = $"Profile_{userId}{fileName.Substring(fileName.LastIndexOf("."))}";
                    filePath = Path.Combine(_uploadPath, fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ProfileImage.CopyToAsync(fileStream);
                    }
                }
                model.ProfilePicture = fileName;
                string temp = JsonConvert.SerializeObject(model);
                var response = await _httpClient.PostAsync("Customers/UpdateProfile", Customs.GetJsonContent(model));
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
    }
}
