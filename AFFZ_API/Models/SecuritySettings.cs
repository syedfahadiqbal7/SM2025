namespace AFFZ_API.Models
{
    public class SecuritySettings
    {
        public string JwtSecretKey { get; set; } = string.Empty;
        public string StripeSecretKey { get; set; } = string.Empty;
        public string StripePublishableKey { get; set; } = string.Empty;
        public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
        public int JwtExpirationHours { get; set; } = 24;
        public bool RequireHttps { get; set; } = true;
    }

    public class CorsSettings
    {
        public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
        public string[] AllowedMethods { get; set; } = new[] { "GET", "POST", "PUT", "DELETE", "OPTIONS" };
        public string[] AllowedHeaders { get; set; } = new[] { "Authorization", "Content-Type", "Accept" };
        public bool AllowCredentials { get; set; } = true;
        public int PreflightMaxAgeMinutes { get; set; } = 10;
    }
}
