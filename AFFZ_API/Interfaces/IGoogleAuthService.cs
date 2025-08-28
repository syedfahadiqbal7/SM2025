using AFFZ_API.Models;

namespace AFFZ_API.Interfaces
{
    public interface IGoogleAuthService
    {
        Task<GoogleAuthResponse> AuthenticateGoogleUserAsync(GoogleAuthRequest request);
        Task<bool> IsGoogleUserAsync(string email);
    }
}
