using AFFZ_API.Interfaces;
using AFFZ_API.Models;
using AFFZ_API.Models.Partial;
using AFFZ_API.Utils;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace AFFZ_API.Controllers.CustomerControllers
{
    [Route("api/Customers")]
    [ApiController]
    public class CustomerUserManagmentController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly MyDbContext _context;
        private readonly IConfiguration _Config;
        private readonly ILogger<CustomerUserManagmentController> _logger;
        private readonly string _jsonFilePath;
        private string BaseUrl = string.Empty;
        private string PublicDomain = string.Empty;
        private string ApiHttpsPort = string.Empty;
        private string CustomerHttpsPort = string.Empty;
        public CustomerUserManagmentController(MyDbContext context, IEmailService emailService, IConfiguration config, ILogger<CustomerUserManagmentController> logger, IAppSettingsService service)
        {
            _context = context;
            _emailService = emailService;
            _Config = config;
            _logger = logger;
            _jsonFilePath = Path.Combine(AppContext.BaseDirectory, "Resources", "CEmailTemplates.json");
            BaseUrl = service.GetBaseIpAddress();
            PublicDomain = service.GetPublicDomain();
            ApiHttpsPort = service.GetApiHttpsPort();
            CustomerHttpsPort = service.GetCustomerHttpsPort();
        }

        [HttpPost]
        [Route("Login")]
        public async Task<ActionResult<SResponse>> Login(LoginModel loginDetail)
        {
            try
            {
                if (loginDetail == null)
                {
                    return new SResponse
                    {
                        StatusCode = HttpStatusCode.InternalServerError,
                        Message = $"Please Fill Email and Password.",
                    };
                }
                string encryptedPassword = Cryptography.Encrypt(loginDetail.Password);
                Customers customerdetail = await _context.Customers.Where(x => x.Email == loginDetail.Email && x.Password == encryptedPassword).FirstOrDefaultAsync();
                if (customerdetail == null)
                {
                    return new SResponse
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = "Your Email/Password is wrong!"
                    };
                }
                string temp = Cryptography.Decrypt(customerdetail.Password);



                return new SResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = "User Login Success!",
                    Data = customerdetail,
                };
            }
            catch (Exception ex)
            {
                return new SResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = $"Exception: {ex.Message}",
                };
            }
        }

        // GET: api/Customers

        [HttpPost]
        [Route("Register")]
        public async Task<ActionResult<SResponse>> PostCustomer(Customers customers, string referralCode = null)
        {
            try
            {
                _logger.LogInformation("Attempting to register new customer with email: {Email}", customers.Email);

                // Validate input
                if (string.IsNullOrEmpty(customers.Email) || string.IsNullOrEmpty(customers.Password) || string.IsNullOrEmpty(customers.CustomerName))
                {
                    _logger.LogWarning("Missing required fields for registration.");
                    return BadRequest(new SResponse
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Message = "Email, Password, and Customer Name are required."
                    });
                }

                // Check for duplicate email or phone number
                var existingCustomer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Email == customers.Email);// || c.PhoneNumber == customers.PhoneNumber);

                if (existingCustomer != null)
                {
                    _logger.LogWarning("Customer with the same email or phone number already exists.");
                    return new SResponse
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Message = "A customer with the same Email Address or Contact Number already exists."
                    };
                }

                // Encrypt password and set customer properties
                customers.RoleId = _context.Roles.Where(x => x.RoleName.ToLower() == "customers")
                                                 .Select(x => x.RoleId)
                                                 .FirstOrDefault();

                customers.CreatedBy = 1; // System ID or Admin ID
                customers.CreatedDate = DateTime.Now;
                customers.Password = Cryptography.Encrypt(customers.Password);
                customers.IsEmailVerified = false;

                // Generate email verification token
                customers.VerificationToken = Guid.NewGuid().ToString();
                customers.TokenExpiry = DateTime.Now.AddHours(24); // Token valid for 24 hours



                _context.Customers.Add(customers);
                await _context.SaveChangesAsync();
                // Call stored procedure to update referral table if referral code is provided
                if (!string.IsNullOrEmpty(referralCode))
                {
                    var commandText = "EXEC UpdateReferralOnSignup @ReferredCustomerID, @ReferralCode";
                    var referredCustomerIdParam = new SqlParameter("@ReferredCustomerID", customers.CustomerId);
                    var referralCodeParam = new SqlParameter("@ReferralCode", referralCode);

                    await _context.Database.ExecuteSqlRawAsync(commandText, referredCustomerIdParam, referralCodeParam);
                }

                // Generate email verification Link
                //var verificationLink = $"{Request.Scheme}://{PublicDomain}:{ApiHttpsPort}/api/Customers/VerifyEmail?token={customers.VerificationToken}";
                var verificationLink = $"{ApiHttpsPort}/api/Customers/VerifyEmail?token={customers.VerificationToken}";
                // Load email template
                var loader = new EmailTemplateLoader(_jsonFilePath);
                var emailTemplate = loader.GetEmailTemplate("EmailVerification");

                // Replace placeholders
                var emailBody = emailTemplate.Body
                    .Replace("{{CustomerName}}", customers.CustomerName)
                    .Replace("{{VerificationLink}}", verificationLink)
                    .Replace("{{CurrentYear}}", DateTime.Now.Year.ToString());


                // Send verification email
                bool emailSent = await _emailService.SendEmail(customers.Email, "Welcome to Smart Center - Verify Your Email.", emailBody, customers.CustomerName, isHtml: true);

                if (!emailSent)
                {
                    _logger.LogWarning("Failed to send verification email to: {Email}", customers.Email);
                    return new SResponse
                    {
                        StatusCode = HttpStatusCode.InternalServerError,
                        Message = "Registration succeeded, but failed to send the verification email. Please contact support."
                    };
                }

                _logger.LogInformation("Customer registered successfully with email: {Email}", customers.Email);
                return new SResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = "Customer successfully registered! Please verify your email.",
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during customer registration for email: {Email}", customers.Email);
                return new SResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = "Error Message:" + ex.Message + " Exception:" + ex.ToString()
                };
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customers>>> GetCustomers()
        {
            return await _context.Customers.ToListAsync();
        }

        // GET: api/Customers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Customers>> GetCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
            {
                return NotFound();
            }

            return customer;
        }

        // PUT: api/Customers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCustomer(int id, Customers customers)
        {
            if (id != customers.CustomerId)
            {
                return BadRequest();
            }

            _context.Entry(customers).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPost]
        [Route("UpdateProfile")]
        public async Task<SResponse> UpdateProfile([FromBody] Customers model)
        {
            try
            {
                var existingProfile = _context.Customers.Where(x => x.CustomerId == model.CustomerId).FirstOrDefault();



                if (existingProfile != null)
                {
                    existingProfile.FirstName = model.FirstName;
                    existingProfile.CustomerName = model.CustomerName;
                    existingProfile.LastName = model.LastName;
                    existingProfile.PhoneNumber = model.PhoneNumber;
                    existingProfile.Address = model.Address;
                    existingProfile.DOB = model.DOB;
                    existingProfile.PostalCode = model.PostalCode;

                    if (!string.IsNullOrEmpty(model.ProfilePicture))
                    {
                        existingProfile.ProfilePicture = model.ProfilePicture;
                    }
                }


                await _context.SaveChangesAsync();
                return new SResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = "Profile Updated!",
                };
            }
            catch (Exception ex)
            {
                return new SResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = $"Exception: {ex.Message}",
                };
            }
        }


        // DELETE: api/Customers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerId == id);
        }

        [HttpGet("CheckEmail/{email}")]
        public async Task<IActionResult> CheckEmail(string email)
        {
            try
            {
                var user = await _context.Customers.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    return NotFound("Email not registered.");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error occurred while checking email.");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An error occurred while processing your request.");
            }
        }


        //[HttpGet]
        //[Route("VerifyEmail")]
        //public async Task<IActionResult> VerifyEmail(string token)
        //{
        //    var customer = await _context.Customers.FirstOrDefaultAsync(c => c.VerificationToken == token && c.TokenExpiry > DateTime.Now);

        //    if (customer == null)
        //    {
        //        return Redirect("/Login?message=Invalid or expired verification link.&status=fail");
        //    }

        //    customer.IsEmailVerified = true;
        //    customer.VerificationToken = null; // Remove token
        //    customer.TokenExpiry = null;

        //    await _context.SaveChangesAsync();

        //    return Redirect("/Login?message=Your email has been successfully verified! You can now log in.&status=success");
        //}

        [HttpGet]
        [Route("VerifyEmail")]
        public async Task<IActionResult> VerifyEmail(string token)
        {
            try
            {
                // Get Customer URL from configuration
                // string CustomerUrl = $"{Request.Scheme}://{PublicDomain}:{CustomerHttpsPort}/";
                string CustomerUrl = $"{CustomerHttpsPort}/";
                // Find the customer by verification token and ensure token is not expired
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.VerificationToken == token && c.TokenExpiry > DateTime.Now);

                // Handle invalid or expired token
                if (customer == null)
                {
                    return Redirect($"{CustomerUrl}Login?message=Invalid or expired verification link.&status=fail");
                }

                // Update email verification status
                customer.IsEmailVerified = true;
                customer.VerificationToken = null; // Clear token
                customer.TokenExpiry = null;       // Clear expiry time

                await _context.SaveChangesAsync();

                // Redirect to login with success message
                return Redirect($"{CustomerUrl}Login?message=Your email has been successfully verified. You can now log in.&status=success");
            }
            catch (Exception ex)
            {
                // Return BadRequest with exception details for debugging
                return BadRequest(ex.Message);
            }
        }


        [HttpPost("SendPasswordResetEmail")]
        public async Task<IActionResult> SendPasswordResetEmail([FromBody] string email)
        {
            try
            {
                var user = await _context.Customers.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    return NotFound("Email not registered.");
                }

                user.VerificationToken = Guid.NewGuid().ToString(); // Generate a unique token                
                user.TokenExpiry = DateTime.Now.AddHours(1); // Token valid for 1 hour


                //string resetLink = $"{Request.Scheme}://{PublicDomain}:{CustomerHttpsPort}/ResetPassword?token={user.VerificationToken}";
                string resetLink = $"{CustomerHttpsPort}/ResetPassword?token={user.VerificationToken}";

                // Load email template
                var loader = new EmailTemplateLoader(_jsonFilePath);
                var emailTemplate = loader.GetEmailTemplate("PasswordReset");

                // Replace placeholders
                var emailBody = emailTemplate.Body
                    .Replace("{{CustomerName}}", user.CustomerName)
                    .Replace("{{ResetLink}}", resetLink)
                    .Replace("{{CurrentYear}}", DateTime.Now.Year.ToString());

                await _context.SaveChangesAsync();

                bool emailSent = await _emailService.SendEmail(email, "Password Reset Request", emailBody, user.CustomerName, isHtml: true);
                if (!emailSent)
                {
                    _logger.LogWarning("Failed to send password reset email to: {Email}", email);
                    return StatusCode((int)HttpStatusCode.InternalServerError, "Failed to send email.");
                }

                return Ok("Password reset email sent successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending password reset email.");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An error occurred while processing your request.");
            }
        }

        //Reset Password

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] CResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var user = await _context.Customers.FirstOrDefaultAsync(u => u.VerificationToken == model.Token && u.TokenExpiry > DateTime.Now);

                if (user == null)
                {
                    return BadRequest("Invalid or expired token.");
                }

                user.Password = Cryptography.Encrypt(model.NewPassword);
                user.VerificationToken = null;
                user.TokenExpiry = null;
                await _context.SaveChangesAsync();

                return Ok("Password reset successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during password reset.");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An error occurred while processing your request.");
            }
        }
        [HttpPost]
        [Route("GoogleLogin")]
        public async Task<ActionResult<SResponse>> GoogleLogin([FromBody] GoogleLoginModel googleLoginModel)
        {
            try
            {
                var payload = await VerifyGoogleTokenAsync(googleLoginModel.IdToken);

                // Check if user exists, if not, create new customer
                var existingCustomer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Email == payload.Email);

                if (existingCustomer == null)
                {
                    // Create new customer from Google profile
                    var newCustomer = new Customers
                    {
                        CustomerName = payload.Name,
                        Email = payload.Email,
                        Password = Cryptography.Encrypt(Guid.NewGuid().ToString()), // Set a dummy password as it's not needed for Google login
                        IsEmailVerified = true, // Google verifies the email
                        CreatedBy = 1, // System ID
                        CreatedDate = DateTime.Now
                    };

                    _context.Customers.Add(newCustomer);
                    await _context.SaveChangesAsync();
                    existingCustomer = newCustomer;
                }

                // Success response
                return Ok(new SResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = "Google Login successful",
                    Data = existingCustomer
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during Google Login.");
                return new SResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = "Error during Google Login: " + ex.Message
                };
            }
        }
        private async Task<GoogleJsonWebSignature.Payload> VerifyGoogleTokenAsync(string idToken)
        {
            // Load your Google service account credentials or OAuth2 credentials
            var credential = await GoogleCredential.GetApplicationDefaultAsync();

            // Alternatively, use a specific credentials file:
            // var credential = GoogleCredential.FromFile("path_to_your_service_account_file.json");

            // Use GoogleJsonWebSignature to validate the token
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                // Optionally specify audience here, but it should be set as required
                Audience = new[] { "982691889737-shum2qj5ad523tkd25sie7j7aeo2k1vh.apps.googleusercontent.com" }
            };

            // Validate the ID token
            var validPayload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            return validPayload;
        }


    }
    public class GoogleLoginModel
    {
        public string IdToken { get; set; }
    }
}
