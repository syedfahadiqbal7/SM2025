namespace AFFZ_API.Models
{
    public class GoogleAuthRequest
    {
        public string IdToken { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Picture { get; set; } = string.Empty;
    }

    public class GoogleAuthResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string JwtToken { get; set; } = string.Empty;
        public User? User { get; set; }
        public bool IsNewUser { get; set; }
    }

    public class GoogleUserInfo
    {
        public string Sub { get; set; } = string.Empty; // Google's unique user ID
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string GivenName { get; set; } = string.Empty;
        public string FamilyName { get; set; } = string.Empty;
        public string Picture { get; set; } = string.Empty;
        public bool EmailVerified { get; set; }
        public string Locale { get; set; } = string.Empty;
        public string Aud { get; set; } = string.Empty; // Audience (Client ID)
        public string Iss { get; set; } = string.Empty; // Issuer
        public long Exp { get; set; } // Expiration time
        public long Iat { get; set; } // Issued at time
    }
}
