using AFFZ_Customer.Models;
using AFFZ_Customer.Models.Partial;
using AFFZ_Customer.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Security.Claims;

namespace AFFZ_Customer.Controllers
{
    public class Login : Controller
    {

        private readonly HttpClient _httpClient;
        private readonly IDataProtector _protector;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<Login> _logger;
        private readonly IConfiguration _configuration;
        // Constructor to initialize dependencies
        public Login(IHttpClientFactory httpClientFactory, IDataProtectionProvider provider, IWebHostEnvironment environment, ILogger<Login> logger, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient("Main");
            _protector = provider.CreateProtector("Example.SessionProtection");
            _environment = environment;
            _logger = logger;
            _logger.LogDebug("Login controller initialized.");
            _configuration = configuration;
        }

        // GET: Login page


        public IActionResult Index(string returnUrl = "")
        {
            try
            {
                // Get Google Client ID from configuration
                var googleClientId = _configuration["Authentication:Google:ClientId"] ?? "YOUR_GOOGLE_CLIENT_ID_HERE";
                ViewData["GoogleClientId"] = googleClientId;

                if (!string.IsNullOrEmpty(returnUrl))
                {
                    ViewBag.ReturnUrl = returnUrl;
                }
                _logger.LogDebug("Index action called with returnUrl: {ReturnUrl}", returnUrl);
                ViewBag.ReturnUrl = returnUrl; // Store return URL to navigate back after login
                string IsloginExist = HttpContext.Session.GetEncryptedString("UserId", _protector);
                if (!string.IsNullOrEmpty(IsloginExist))
                {
                    // Redirect to the last visited page if available
                    var referer = Request.Headers["Referer"].ToString();
                    if (!string.IsNullOrEmpty(referer))
                    {
                        _logger.LogInformation("CustomerId exists. Redirecting to: {Referer}", referer);
                        return Redirect(referer);
                    }

                    // Fallback to dashboard if no referer is available
                    _logger.LogInformation("CustomerId exists. Redirecting to Dashboard.");
                    return RedirectToAction("Index", "Dashboard");
                }
                else
                {
                    return View("Login", new LoginModel()); // Render the login view
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }




        // POST: Login action
        [HttpPost]
        public async Task<IActionResult> CustomersLogin(LoginModel model, string returnUrl = null)
        {
            _logger.LogDebug("CustomersLogin action called with model: {@Model} and returnUrl: {ReturnUrl}", model, returnUrl);

            // Validate the model state
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid login model state.");
                return View("Login", model);
            }

            try
            {
                _logger.LogDebug("Sending login request to API.");
                var response = await _httpClient.PostAsync("Customers/Login", Customs.GetJsonContent(model));
                _logger.LogDebug("Received response with status code: {StatusCode}", response.StatusCode);

                // Check if the response was successful
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Login request failed with status code: {StatusCode}", response.StatusCode);
                    var Error = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("Received response content: {Error}", Error);
                    SResponse sResponseError = JsonConvert.DeserializeObject<SResponse>(Error);
                    ModelState.AddModelError(string.Empty, sResponseError.Message);
                    return View("Login", model);
                }

                // Deserialize the response
                var responseString = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Received response content: {ResponseString}", responseString);
                SResponse sResponse = JsonConvert.DeserializeObject<SResponse>(responseString);

                // Check the status of the response and process the data
                if (sResponse.StatusCode == HttpStatusCode.OK && sResponse.Data != null)
                {
                    Customers customerDetail = JsonConvert.DeserializeObject<Customers>(sResponse.Data.ToString());
                    if (customerDetail == null)
                    {
                        _logger.LogError("Failed to deserialize customer details.");
                        ModelState.AddModelError(string.Empty, "Invalid response from server.");
                        return View("Login", model);
                    }

                    // Store customer details in session
                    _logger.LogDebug("Storing customer details in session.");
                    HttpContext.Session.SetEncryptedString("UserId", customerDetail.CustomerId.ToString(), _protector);
                    HttpContext.Session.SetEncryptedString("RoleId", customerDetail.RoleId.ToString(), _protector);
                    HttpContext.Session.SetEncryptedString("ProfilePicturePath", string.IsNullOrEmpty(customerDetail.ProfilePicture) ? "" : customerDetail.ProfilePicture, _protector);
                    HttpContext.Session.SetEncryptedString("Email", customerDetail.Email, _protector);
                    HttpContext.Session.SetEncryptedString("MemberSince", customerDetail.CreatedDate.ToString("MMM yyyy"), _protector);
                    HttpContext.Session.SetEncryptedString("FirstName", string.IsNullOrEmpty(customerDetail.FirstName) ? "" : customerDetail.FirstName, _protector);
                    HttpContext.Session.SetEncryptedString("CustomerName", customerDetail.CustomerName, _protector);

                    // Retrieve additional customer information
                    string refcode = await GetReferralCode(customerDetail.CustomerId);
                    decimal walletPoints = await GetWalletData(customerDetail.CustomerId);

                    // Store additional information in session
                    HttpContext.Session.SetEncryptedString("walletPoints", walletPoints.ToString(), _protector);
                    HttpContext.Session.SetEncryptedString("ReferralCode", refcode, _protector);

                    // Redirect to the original return URL or to the dashboard
                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        _logger.LogDebug("Redirecting to returnUrl: {ReturnUrl}", returnUrl);
                        return Redirect(returnUrl);
                    }
                    _logger.LogDebug("Redirecting to Dashboard.");
                    return RedirectToAction("Index", "UserDashboard");
                }
                else
                {
                    _logger.LogWarning("Login failed. StatusCode: {StatusCode}, Data: {Data}", sResponse.StatusCode, sResponse.Data);
                    var _Error = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("Received response content: {_Error}", _Error);
                    SResponse _sResponseError = JsonConvert.DeserializeObject<SResponse>(_Error);
                    ModelState.AddModelError(string.Empty, _sResponseError.Message);
                    return View("Login", model);
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request failed during customer login.");
                ModelState.AddModelError(string.Empty, "Unable to connect to the server. Please try again later.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during customer login.");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
            }

            // Ensure all model errors are returned to the view for validation
            foreach (var modelError in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogWarning("Model error: {ErrorMessage}", modelError.ErrorMessage);
            }

            return View("Login", model);
        }

        // GET: Logout action
        public IActionResult Logout()
        {
            _logger.LogDebug("Logout action called.");
            try
            {
                // Clear the session
                HttpContext.Session.Clear();
                _logger.LogDebug("Session cleared successfully.");
                return RedirectToAction("Index", "Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during logout.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while logging out.");
            }
        }

        // GET: Retrieve profile picture
        public IActionResult GetImage()
        {
            _logger.LogDebug("GetImage action called.");
            try
            {
                // Retrieve profile picture path from session
                string profilePicturePath = HttpContext.Session.GetEncryptedString("ProfilePicturePath", _protector);
                _logger.LogDebug("Retrieved profile picture path from session: {ProfilePicturePath}", profilePicturePath);

                if (string.IsNullOrEmpty(profilePicturePath))
                {
                    _logger.LogWarning("Profile picture path is empty or null.");
                    return NotFound();
                }

                // Construct the file path for the profile picture
                var filePath = Path.Combine(_environment.WebRootPath, "ProfilePictures", profilePicturePath);
                _logger.LogDebug("Constructed file path: {FilePath}", filePath);

                // Check if the file exists and return it
                if (System.IO.File.Exists(filePath))
                {
                    var imageBytes = System.IO.File.ReadAllBytes(filePath);
                    string contentType = profilePicturePath.EndsWith("png") ? "image/png" : "image/jpeg";
                    _logger.LogDebug("Returning profile picture with content type: {ContentType}", contentType);
                    return File(imageBytes, contentType);
                }
                else
                {
                    _logger.LogWarning("Profile picture not found at path: {FilePath}", filePath);
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the profile picture.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the profile picture.");
            }
        }

        // Helper method to get the referral code
        private async Task<string> GetReferralCode(int? referrerCustomerId)
        {
            _logger.LogDebug("GetReferralCode called with referrerCustomerId: {ReferrerCustomerId}", referrerCustomerId);
            if (!referrerCustomerId.HasValue)
            {
                _logger.LogWarning("Referrer customer ID is null.");
                return "No Code.";
            }

            try
            {
                // Send request to retrieve referral code
                _logger.LogDebug("Sending request to retrieve referral code for CustomerId: {CustomerId}", referrerCustomerId);
                var response = await _httpClient.GetAsync($"Referral/GetUserReferral?CustomerId={referrerCustomerId}");
                _logger.LogDebug("Received response with status code: {StatusCode}", response.StatusCode);

                // Check if the response was successful
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to retrieve referral code. StatusCode: {StatusCode}", response.StatusCode);
                    return "No Code.";
                }

                // Deserialize the response
                var responseString = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Received response content: {ResponseString}", responseString);
                SResponse sResponse = JsonConvert.DeserializeObject<SResponse>(responseString);

                // Return the referral code if available
                if (sResponse.StatusCode == HttpStatusCode.OK && sResponse.Data != null)
                {
                    _logger.LogDebug("Referral code retrieved successfully.");
                    return sResponse.Data.ToString();
                }
                else
                {
                    _logger.LogWarning("Failed to retrieve referral code. StatusCode: {StatusCode}, Data: {Data}", sResponse.StatusCode, sResponse.Data);
                    return "No Code.";
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request failed while retrieving referral code.");
                return "Error retrieving code.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while retrieving referral code.");
                return "Error retrieving code.";
            }
        }

        // Helper method to get wallet data
        private async Task<decimal> GetWalletData(int? referrerCustomerId)
        {
            _logger.LogDebug("GetWalletData called with referrerCustomerId: {ReferrerCustomerId}", referrerCustomerId);
            if (!referrerCustomerId.HasValue)
            {
                _logger.LogWarning("Referrer customer ID is null.");
                return 0.0m;
            }

            try
            {
                // Send request to retrieve wallet data
                _logger.LogDebug("Sending request to retrieve wallet data for CustomerId: {CustomerId}", referrerCustomerId);
                var response = await _httpClient.GetAsync($"Wallet/GetWalletPoints?CustomerId={referrerCustomerId}");
                _logger.LogDebug("Received response with status code: {StatusCode}", response.StatusCode);

                // Check if the response was successful
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to retrieve wallet data. StatusCode: {StatusCode}", response.StatusCode);
                    return 0.0m;
                }

                // Deserialize the response
                var responseString = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Received response content: {ResponseString}", responseString);
                SResponse sResponse = JsonConvert.DeserializeObject<SResponse>(responseString);

                // Return the wallet points if available
                if (sResponse.StatusCode == HttpStatusCode.OK && sResponse.Data != null)
                {
                    var wallet = JsonConvert.DeserializeObject<WalletModel>(sResponse.Data.ToString());
                    _logger.LogDebug("Wallet data retrieved successfully. Points: {Points}", wallet?.Points);
                    return wallet?.Points ?? 0.0m;
                }
                else
                {
                    _logger.LogWarning("Failed to retrieve wallet data. StatusCode: {StatusCode}, Data: {Data}", sResponse.StatusCode, sResponse.Data);
                    return 0.0m;
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request failed while retrieving wallet data.");
                return 0.0m;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while retrieving wallet data.");
                return 0.0m;
            }
        }

        [HttpGet]

        public IActionResult ForgotPassword()
        {
            _logger.LogInformation("ForgotPassword GET action invoked.");

            return View("ForgotPassword", new CForgotPasswordModel());
        }


        [HttpPost]
        public async Task<IActionResult> ForgotPassword(CForgotPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid forgot password model state.");
                return View("ForgotPassword", model);
            }

            try
            {
                if (string.IsNullOrWhiteSpace(model.Email))
                {
                    ModelState.AddModelError(string.Empty, "Email is required.");
                    return View("ForgotPassword", model);
                }

                var response = await _httpClient.GetAsync($"Customers/CheckEmail/{model.Email}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Email not registered: {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, "This email address is not registered with us.");
                    return View("ForgotPassword", model);
                }

                // Send password reset link
                var emailResponse = await _httpClient.PostAsync("Customers/SendPasswordResetEmail", Customs.GetJsonContent(model.Email));
                if (!emailResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to send password reset email for: {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, "An error occurred while sending the reset email.");
                    return View("ForgotPassword", model);
                }

                TempData["SuccessMessage"] = "A password reset link has been sent to your email.";
                return RedirectToAction("Index", "Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during forgot password.");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
                return View("ForgotPassword", model);
            }
        }

        //Reset Password:

        [HttpGet]
        [Route("ResetPassword")]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                TempData["FailMessage"] = "Invalid or expired token.";
                return RedirectToAction("ForgotPassword");
            }

            return View("ResetPassword", new CResetPasswordModel { Token = token });
        }

        [HttpPost]
        [Route("ResetPassword")]
        public async Task<IActionResult> ResetPassword(CResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("ResetPassword", model);
            }

            try
            {
                var content = Customs.GetJsonContent(model);
                var response = await _httpClient.PostAsync("Customers/ResetPassword", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    TempData["FailMessage"] = string.IsNullOrEmpty(errorResponse) ? "Failed to reset password." : errorResponse;
                    return View("ResetPassword", model);
                }

                TempData["SuccessMessage"] = "Your password has been reset successfully.";
                return RedirectToAction("Index", "Login");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error during password reset.");
                ModelState.AddModelError(string.Empty, "An error occurred while connecting to the server.");
                return View("ResetPassword", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during password reset.");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
                return View("ResetPassword", model);
            }
        }
        //Google Login
        [HttpGet]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Login", new { returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, provider);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return View("Login");
            }

            var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!authenticateResult.Succeeded || authenticateResult.Principal == null)
            {
                ModelState.AddModelError(string.Empty, "External authentication failed.");
                return View("Login");
            }

            // Extract user info
            var email = authenticateResult.Principal.FindFirstValue(ClaimTypes.Email);
            var name = authenticateResult.Principal.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError(string.Empty, "Email claim not received.");
                return View("Login");
            }

            _logger.LogDebug("External login successful with email: {Email}", email);

            // Send email to your existing API to get or register the customer
            var externalLoginModel = new
            {
                Email = email,
                Name = name,
                LoginProvider = "Google"
            };

            try
            {
                var response = await _httpClient.PostAsync("Customers/ExternalLogin", Customs.GetJsonContent(externalLoginModel));
                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("External login API failed: {Error}", errorMsg);
                    ModelState.AddModelError(string.Empty, "Login via Google failed.");
                    return View("Login");
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var sResponse = JsonConvert.DeserializeObject<SResponse>(responseString);

                if (sResponse.StatusCode == HttpStatusCode.OK && sResponse.Data != null)
                {
                    Customers customerDetail = JsonConvert.DeserializeObject<Customers>(sResponse.Data.ToString());

                    if (customerDetail == null)
                    {
                        _logger.LogError("Failed to deserialize external login customer.");
                        ModelState.AddModelError(string.Empty, "Invalid response from server.");
                        return View("Login");
                    }

                    // Set encrypted session data
                    HttpContext.Session.SetEncryptedString("UserId", customerDetail.CustomerId.ToString(), _protector);
                    HttpContext.Session.SetEncryptedString("RoleId", customerDetail.RoleId.ToString(), _protector);
                    HttpContext.Session.SetEncryptedString("Email", customerDetail.Email, _protector);
                    HttpContext.Session.SetEncryptedString("CustomerName", customerDetail.CustomerName, _protector);

                    string refcode = await GetReferralCode(customerDetail.CustomerId);
                    decimal walletPoints = await GetWalletData(customerDetail.CustomerId);
                    HttpContext.Session.SetEncryptedString("walletPoints", walletPoints.ToString(), _protector);
                    HttpContext.Session.SetEncryptedString("ReferralCode", refcode, _protector);

                    return !string.IsNullOrEmpty(returnUrl)
                        ? Redirect(returnUrl)
                        : RedirectToAction("Index", "UserDashboard");
                }

                ModelState.AddModelError(string.Empty, "Login failed.");
                return View("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in ExternalLoginCallback.");
                ModelState.AddModelError(string.Empty, "An error occurred during external login.");
                return View("Login");
            }
        }



    }
}
