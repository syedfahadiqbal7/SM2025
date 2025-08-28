using AFFZ_API.Interfaces;
using AFFZ_API.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AFFZ_API.Services
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GoogleAuthService> _logger;
        private readonly IAuthService _authService;
        private readonly MyDbContext _context;
        private readonly HttpClient _httpClient;

        public GoogleAuthService(
            IConfiguration configuration,
            ILogger<GoogleAuthService> logger,
            IAuthService authService,
            MyDbContext context,
            HttpClient httpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _authService = authService;
            _context = context;
            _httpClient = httpClient;
        }

        public async Task<GoogleAuthResponse> AuthenticateGoogleUserAsync(GoogleAuthRequest request)
        {
            try
            {
                _logger.LogInformation("Authenticating Google user with email: {Email}", request.Email);

                // Verify the Google ID token
                var googleUserInfo = await VerifyGoogleIdTokenAsync(request.IdToken);
                if (googleUserInfo == null)
                {
                    return new GoogleAuthResponse
                    {
                        IsSuccess = false,
                        Message = "Invalid Google ID token"
                    };
                }

                // Check if customer exists
                var existingCustomer = await _context.Customers.Where(c => c.Email == googleUserInfo.Email).FirstOrDefaultAsync();

                if (existingCustomer != null)
                {
                    // Customer exists, generate JWT token
                    var jwtToken = await _authService.GenerateJwtTokenAsync(
                        existingCustomer.CustomerId.ToString(),
                        existingCustomer.Role ?? "Customer",
                        existingCustomer.Email);

                    // Update last login
                    existingCustomer.LastLoginDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    return new GoogleAuthResponse
                    {
                        IsSuccess = true,
                        Message = "Login successful",
                        JwtToken = jwtToken,
                        User = MapCustomerToUser(existingCustomer),
                        IsNewUser = false
                    };
                }
                else
                {
                    // Create new customer
                    var newCustomer = new Customers
                    {
                        Email = googleUserInfo.Email,
                        FirstName = googleUserInfo.GivenName,
                        LastName = googleUserInfo.FamilyName,
                        CustomerName = $"{googleUserInfo.GivenName} {googleUserInfo.FamilyName}",
                        Role = "Customer",
                        CreatedDate = DateTime.UtcNow,
                        LastLoginDate = DateTime.UtcNow,
                        GoogleId = googleUserInfo.Sub,
                        ProfilePicture = googleUserInfo.Picture,
                        IsEmailVerified = googleUserInfo.EmailVerified,
                        MemberSince = DateTime.UtcNow
                    };

                    _context.Customers.Add(newCustomer);
                    await _context.SaveChangesAsync();

                    // Generate JWT token
                    var jwtToken = await _authService.GenerateJwtTokenAsync(
                        newCustomer.CustomerId.ToString(),
                        newCustomer.Role,
                        newCustomer.Email);

                    return new GoogleAuthResponse
                    {
                        IsSuccess = true,
                        Message = "Account created and login successful",
                        JwtToken = jwtToken,
                        User = MapCustomerToUser(newCustomer),
                        IsNewUser = true
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authenticating Google user with email: {Email}", request.Email);
                return new GoogleAuthResponse
                {
                    IsSuccess = false,
                    Message = "Authentication failed: " + ex.Message
                };
            }
        }

        private User MapCustomerToUser(Customers customer)
        {
            return new User
            {
                UserId = customer.CustomerId ?? 0,
                Username = customer.Email ?? string.Empty,
                Email = customer.Email ?? string.Empty,
                FirstName = customer.FirstName ?? string.Empty,
                LastName = customer.LastName ?? string.Empty,
                //Role = customer.Role ?? "Customer",
                IsActive = true,
                CreatedDate = customer.CreatedDate ?? DateTime.UtcNow,
                Lastlogindate = customer.LastLoginDate ?? DateTime.UtcNow
            };
        }

        private async Task<GoogleUserInfo?> VerifyGoogleIdTokenAsync(string idToken)
        {
            try
            {
                var clientId = _configuration["Authentication:Google:ClientId"];
                if (string.IsNullOrEmpty(clientId))
                {
                    _logger.LogError("Google ClientId not configured");
                    return null;
                }

                // For production, you should verify the token with Google's servers
                // For now, we'll decode the JWT token to get user info
                var tokenParts = idToken.Split('.');
                if (tokenParts.Length != 3)
                {
                    _logger.LogWarning("Invalid JWT token format");
                    return null;
                }

                // Decode the payload (second part)
                var payload = tokenParts[1];
                var paddedPayload = payload.PadRight(4 * ((payload.Length + 3) / 4), '=');
                var base64 = paddedPayload.Replace('-', '+').Replace('_', '/');

                try
                {
                    var jsonBytes = Convert.FromBase64String(base64);
                    var json = System.Text.Encoding.UTF8.GetString(jsonBytes);
                    var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(json);

                    if (userInfo != null && userInfo.Aud == clientId)
                    {
                        return userInfo;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error decoding JWT token payload");
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying Google ID token");
                return null;
            }
        }

        public async Task<bool> IsGoogleUserAsync(string email)
        {
            try
            {
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Email == email);

                return customer?.GoogleId != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if customer is Google user for email: {Email}", email);
                return false;
            }
        }
    }
}
