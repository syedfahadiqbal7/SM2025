using AFFZ_Customer.Models;
using AFFZ_Customer.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;

namespace AFFZ_Customer.Controllers
{
    public class SignUp : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SignUp> _logger;
        private string BaseUrlIp = string.Empty;
        private string PublicDomain = string.Empty;
        private string ApiHttpsPort = string.Empty;
        private string CustomerHttpsPort = string.Empty;
        public SignUp(IHttpClientFactory httpClientFactory, ILogger<SignUp> logger, IOptions<AppSettings> options, IAppSettingsService service)
        {
            _httpClient = httpClientFactory.CreateClient("Main");
            _logger = logger;
            _logger.LogDebug("SignUp controller initialized.");
            BaseUrlIp = options.Value.BaseIpAddress;
            PublicDomain = service.GetPublicDomain();
            ApiHttpsPort = service.GetApiHttpsPort();
        }

        // GET: SignUp page
        public IActionResult Index()
        {
            _logger.LogDebug("SignUp page requested.");
            var verificationLink = $"{ApiHttpsPort}";
            ViewBag.APIUrl = verificationLink;
            return View("SignUp", new Customers());
        }

        // POST: Register a new customer
        [HttpPost]
        public async Task<IActionResult> CustomersRegister(Customers model)
        {
            try
            {
                _logger.LogDebug("CustomersRegister action called with model: {@Model}", model);

                // Validate ModelState
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Model validation failed.");
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        _logger.LogWarning("Validation error: {ErrorMessage}", error.ErrorMessage);
                    }
                    return View("SignUp", model);
                }

                try
                {
                    if (await GetReferralCode(model.ReferrerCode))
                    {
                        TempData["FailMessage"] = "Referral Code is already in use. Try another one.";
                        return View("SignUp", model);
                    }
                    _logger.LogInformation("Sending registration request to API.");
                    var response = await _httpClient.PostAsync(
                        $"Customers/Register?referralCode={(!string.IsNullOrEmpty(model.ReferrerCode) ? model.ReferrerCode : " ")}",
                        Customs.GetJsonContent(model)
                    );

                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("API Response: {ResponseContent}", responseContent);

                    // Handle non-success HTTP status
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("API returned error status: {StatusCode}", response.StatusCode);
                        ModelState.AddModelError(string.Empty, "Registration failed. Please try again.");
                        return View("SignUp", model);
                    }

                    // Deserialize API response
                    var sResponse = JsonConvert.DeserializeObject<SResponse>(responseContent);
                    if (sResponse == null)
                    {
                        _logger.LogWarning("Failed to parse API response.");
                        ModelState.AddModelError(string.Empty, "Unexpected response from the server.");
                        return View("SignUp", model);
                    }

                    if (sResponse.StatusCode != HttpStatusCode.OK)
                    {
                        _logger.LogWarning("API returned failure status: {StatusCode}, Message: {Message}",
                            sResponse.StatusCode, sResponse.Message);
                        ModelState.AddModelError(string.Empty, sResponse.Message ?? "Registration failed.");
                        return View("SignUp", model);
                    }

                    // Success scenario
                    _logger.LogInformation("User successfully registered.");
                    TempData["SuccessMessage"] = "User registered successfully! Please verify your email.";
                    return RedirectToAction("Index", "Login");
                }
                catch (HttpRequestException httpEx)
                {
                    _logger.LogError(httpEx, "HTTP request failed during customer registration.");
                    ModelState.AddModelError(string.Empty, "Unable to connect to the server. Please try again later.");
                    return View("SignUp", model);
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "Failed to deserialize API response.");
                    ModelState.AddModelError(string.Empty, "Invalid response from the server. Please contact support.");
                    return View("SignUp", model);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unexpected error occurred during customer registration.");
                    ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
                    return View("SignUp", model);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        private async Task<bool> GetReferralCode(string? referrercode)
        {
            bool res = false;
            _logger.LogDebug("GetReferralCode called with referrerCustomerId: {referrercode}", referrercode);
            if (!string.IsNullOrEmpty(referrercode))
            {
                _logger.LogWarning("Referrer customer ID is null.");
            }

            try
            {
                // Send request to retrieve referral code
                var response = await _httpClient.GetAsync($"Referral/CheckReferral?referrercode={referrercode}");
                _logger.LogDebug("Received response with status code: {StatusCode}", response.StatusCode);

                // Check if the response was successful
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to retrieve referral code. StatusCode: {StatusCode}", response.StatusCode);
                }

                // Deserialize the response
                var responseString = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Received response content: {ResponseString}", responseString);
                res = Convert.ToBoolean(responseString);
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request failed while retrieving referral code.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while retrieving referral code.");
            }
            return res;
        }
    }

    // Utility class for converting content
    public static class Customs
    {
        public static StringContent GetJsonContent(object obj)
        {
            var jsonString = JsonConvert.SerializeObject(obj);
            return new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json");
        }
    }

    // Model for standardized API response
    public class SResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }
}
