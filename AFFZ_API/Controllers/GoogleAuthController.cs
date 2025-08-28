using AFFZ_API.Interfaces;
using AFFZ_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AFFZ_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoogleAuthController : ControllerBase
    {
        private readonly IGoogleAuthService _googleAuthService;
        private readonly ILogger<GoogleAuthController> _logger;

        public GoogleAuthController(IGoogleAuthService googleAuthService, ILogger<GoogleAuthController> logger)
        {
            _googleAuthService = googleAuthService;
            _logger = logger;
        }

        [HttpPost("signin")]
        public async Task<IActionResult> GoogleSignIn([FromBody] GoogleAuthRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.IdToken))
                {
                    return BadRequest("ID token is required");
                }

                var result = await _googleAuthService.AuthenticateGoogleUserAsync(request);
                
                if (result.IsSuccess)
                {
                    _logger.LogInformation("Google authentication successful for user: {Email}", request.Email);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Google authentication failed for user: {Email}, reason: {Message}", 
                        request.Email, result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Google sign-in for user: {Email}", request.Email);
                return StatusCode(500, "Internal server error during authentication");
            }
        }

        [HttpGet("check-google-user/{email}")]
        public async Task<IActionResult> CheckGoogleUser(string email)
        {
            try
            {
                var isGoogleUser = await _googleAuthService.IsGoogleUserAsync(email);
                return Ok(new { IsGoogleUser = isGoogleUser });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user is Google user for email: {Email}", email);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
