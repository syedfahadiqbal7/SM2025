using AFFZ_API.Models;

namespace AFFZ_API.Interfaces
{
    public interface IUserService
    {
        Task<User?> GetUserByIdAsync(int id);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> CreateUserAsync(User user);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> UserExistsAsync(int id);
    }

    public interface IAuthService
    {
        Task<string> GenerateJwtTokenAsync(string userId, string role, string email);
        Task<bool> ValidateTokenAsync(string token);
        Task<bool> RevokeTokenAsync(string token);
    }

    public interface IPaymentService
    {
        // Basic payment operations
        Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request);
        Task<PaymentHistory> GetPaymentHistoryAsync(int userId);
        Task<bool> RefundPaymentAsync(int paymentId, decimal amount);
        
        // Membership payment operations
        Task<PaymentResult> ProcessMembershipPaymentAsync(PaymentRequest request);
        Task<MembershipPaymentHistory> GetMembershipPaymentAsync(int merchantId);
        Task<bool> UpdateMembershipStatusAsync(int merchantId, bool isActive);
        
        // Stripe integration
        Task<PaymentResult> ProcessStripePaymentAsync(PaymentRequest request, string stripeToken);
        Task<PaymentResult> ProcessStripeMembershipAsync(PaymentRequest request, string stripeToken);
        
        // Payment status updates
        Task<bool> UpdatePaymentStatusAsync(int paymentId, int status);
        Task<bool> UpdateDiscountRequestAsync(RequestForDisCountToUser request);
    }
}
