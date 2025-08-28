using AFFZ_API.Interfaces;
using AFFZ_API.Models;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;

namespace AFFZ_API.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly MyDbContext _context;
        private readonly ILogger<PaymentService> _logger;
        private readonly IConfiguration _configuration;

        public PaymentService(MyDbContext context, ILogger<PaymentService> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;

            // Configure Stripe
            var stripeSecretKey = _configuration["Security:StripeSecretKey"];
            if (!string.IsNullOrEmpty(stripeSecretKey))
            {
                StripeConfiguration.ApiKey = stripeSecretKey;
            }
        }

        public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
        {
            try
            {
                _logger.LogInformation("Processing payment for user {UserId}, service {ServiceId}, amount {Amount}",
                    request.UserId, request.ServiceId, request.Amount);

                var paymentHistory = new PaymentHistory
                {
                    PAYERID = request.UserId,
                    MERCHANTID = request.MerchantId,
                    SERVICEID = request.ServiceId,
                    AMOUNT = request.Amount.ToString(),
                    PAYMENTTYPE = request.PaymentMethod,
                    ISPAYMENTSUCCESS = 1,
                    PAYMENTDATETIME = DateTime.UtcNow,
                    RFDFU = request.RFDFU,
                    Quantity = request.Quantity
                };

                _context.Add(paymentHistory);
                await _context.SaveChangesAsync();

                // Process referral logic if payment successful
                if (paymentHistory.ISPAYMENTSUCCESS == 1)
                {
                    await ProcessReferralLogic(paymentHistory.PAYERID, request.Amount);
                }

                return new PaymentResult
                {
                    IsSuccess = true,
                    TransactionId = paymentHistory.ID.ToString(),
                    Status = "Completed",
                    Message = "Payment processed successfully",
                    PaymentRecord = paymentHistory
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for user {UserId}", request.UserId);
                return new PaymentResult
                {
                    IsSuccess = false,
                    Status = "Failed",
                    Message = "Payment processing failed: " + ex.Message
                };
            }
        }

        public async Task<PaymentHistory> GetPaymentHistoryAsync(int userId)
        {
            try
            {
                return await _context.PaymentHistories
                    .Where(p => p.PAYERID == userId)
                    .OrderByDescending(p => p.PAYMENTDATETIME)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment history for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> RefundPaymentAsync(int paymentId, decimal amount)
        {
            try
            {
                var payment = await _context.PaymentHistories.FindAsync(paymentId);
                if (payment == null)
                {
                    _logger.LogWarning("Payment {PaymentId} not found for refund", paymentId);
                    return false;
                }

                // Update payment status to indicate refund
                payment.ISPAYMENTSUCCESS = 2; // Assuming 2 = refunded
                _context.Update(payment);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Payment {PaymentId} refunded successfully", paymentId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refunding payment {PaymentId}", paymentId);
                return false;
            }
        }

        public async Task<PaymentResult> ProcessMembershipPaymentAsync(PaymentRequest request)
        {
            try
            {
                _logger.LogInformation("Processing membership payment for merchant {MerchantId}, amount {Amount}",
                    request.MerchantId, request.Amount);

                var membershipPayment = new MembershipPaymentHistory
                {
                    PAYERID = request.MerchantId,
                    AMOUNT = request.Amount.ToString(),
                    PAYMENTTYPE = request.PaymentMethod,
                    ISPAYMENTSUCCESS = 1,
                    PAYMENTDATETIME = DateTime.UtcNow,
                    IsActiveMembership = 1
                };

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Deactivate existing memberships
                    var existingMemberships = await _context.MembershipPaymentHistory
                        .Where(m => m.PAYERID == request.MerchantId && m.IsActiveMembership == 1)
                        .ToListAsync();

                    foreach (var membership in existingMemberships)
                    {
                        membership.IsActiveMembership = 0;
                        _context.Update(membership);
                    }

                    // Save new membership payment
                    _context.Add(membershipPayment);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new PaymentResult
                    {
                        IsSuccess = true,
                        TransactionId = membershipPayment.ID.ToString(),
                        Status = "Completed",
                        Message = "Membership payment processed successfully"
                    };
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing membership payment for merchant {MerchantId}", request.MerchantId);
                return new PaymentResult
                {
                    IsSuccess = false,
                    Status = "Failed",
                    Message = "Membership payment processing failed: " + ex.Message
                };
            }
        }

        public async Task<MembershipPaymentHistory> GetMembershipPaymentAsync(int merchantId)
        {
            try
            {
                return await _context.MembershipPaymentHistory
                    .Where(m => m.PAYERID == merchantId && m.IsActiveMembership == 1)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving membership payment for merchant {MerchantId}", merchantId);
                throw;
            }
        }

        public async Task<bool> UpdateMembershipStatusAsync(int merchantId, bool isActive)
        {
            try
            {
                var membership = await _context.MembershipPaymentHistory
                    .Where(m => m.PAYERID == merchantId && m.IsActiveMembership == 1)
                    .FirstOrDefaultAsync();

                if (membership != null)
                {
                    membership.IsActiveMembership = isActive ? 1 : 0;
                    _context.Update(membership);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating membership status for merchant {MerchantId}", merchantId);
                return false;
            }
        }

        public async Task<PaymentResult> ProcessStripePaymentAsync(PaymentRequest request, string stripeToken)
        {
            try
            {
                // Create Stripe payment intent
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(request.Amount * 100), // Convert to cents
                    Currency = request.Currency.ToLower(),
                    Description = request.Description,
                    Metadata = request.Metadata.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString())
                };

                var service = new PaymentIntentService();
                var paymentIntent = service.Create(options);

                // Save payment record
                var paymentHistory = new PaymentHistory
                {
                    PAYERID = request.UserId,
                    MERCHANTID = request.MerchantId,
                    SERVICEID = request.ServiceId,
                    AMOUNT = request.Amount.ToString(),
                    PAYMENTTYPE = "Stripe",
                    ISPAYMENTSUCCESS = 1,
                    PAYMENTDATETIME = DateTime.UtcNow
                };

                _context.Add(paymentHistory);
                await _context.SaveChangesAsync();

                return new PaymentResult
                {
                    IsSuccess = true,
                    TransactionId = paymentIntent.Id,
                    Status = "Completed",
                    Message = "Stripe payment processed successfully",
                    PaymentRecord = paymentHistory,
                    AdditionalData = new Dictionary<string, object>
                    {
                        { "stripe_payment_intent_id", paymentIntent.Id },
                        { "stripe_client_secret", paymentIntent.ClientSecret }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Stripe payment for user {UserId}", request.UserId);
                return new PaymentResult
                {
                    IsSuccess = false,
                    Status = "Failed",
                    Message = "Stripe payment processing failed: " + ex.Message
                };
            }
        }

        public async Task<PaymentResult> ProcessStripeMembershipAsync(PaymentRequest request, string stripeToken)
        {
            try
            {
                // Create Stripe checkout session for membership
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmount = (long)(request.Amount * 100),
                                Currency = request.Currency.ToLower(),
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = "Membership Plan",
                                    Description = request.Description
                                }
                            },
                            Quantity = 1
                        }
                    },
                    Mode = "payment",
                    SuccessUrl = $"{_configuration["AppSettings:CustomerHttpsPort"]}/Payment/Success",
                    CancelUrl = $"{_configuration["AppSettings:CustomerHttpsPort"]}/Payment/Cancel"
                };

                var service = new SessionService();
                var session = service.Create(options);

                return new PaymentResult
                {
                    IsSuccess = true,
                    TransactionId = session.Id,
                    Status = "Pending",
                    Message = "Stripe checkout session created",
                    AdditionalData = new Dictionary<string, object>
                    {
                        { "stripe_session_id", session.Id },
                        { "stripe_checkout_url", session.Url }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Stripe checkout session for merchant {MerchantId}", request.MerchantId);
                return new PaymentResult
                {
                    IsSuccess = false,
                    Status = "Failed",
                    Message = "Stripe checkout session creation failed: " + ex.Message
                };
            }
        }

        public async Task<bool> UpdatePaymentStatusAsync(int paymentId, int status)
        {
            try
            {
                var payment = await _context.PaymentHistories.FindAsync(paymentId);
                if (payment != null)
                {
                    payment.ISPAYMENTSUCCESS = status;
                    _context.Update(payment);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment status for payment {PaymentId}", paymentId);
                return false;
            }
        }

        public async Task<bool> UpdateDiscountRequestAsync(RequestForDisCountToUser request)
        {
            try
            {
                var existingRequest = await _context.RequestForDisCountToUsers
                    .Where(x => x.RFDFU == request.RFDFU)
                    .FirstOrDefaultAsync();

                if (existingRequest != null)
                {
                    // Detach existing entity to avoid tracking conflicts
                    var trackedEntity = _context.ChangeTracker.Entries<RequestForDisCountToUser>()
                        .FirstOrDefault(e => e.Entity.RFDFU == request.RFDFU);
                    if (trackedEntity != null)
                    {
                        trackedEntity.State = EntityState.Detached;
                    }

                    request.FINALPRICE = existingRequest.FINALPRICE;
                    _context.Update(request);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating discount request {RFDFU}", request.RFDFU);
                return false;
            }
        }

        private async Task ProcessReferralLogic(int referredCustomerId, decimal amountSpent)
        {
            try
            {
                // Call CompleteReferral stored procedure
                var commandCompleteReferral = "EXEC CompleteReferral @ReferredCustomerID";
                var referredCustomerIdParam = new Microsoft.Data.SqlClient.SqlParameter("@ReferredCustomerID", referredCustomerId);
                await _context.Database.ExecuteSqlRawAsync(commandCompleteReferral, referredCustomerIdParam);

                // Call UpdateReferrerPoints stored procedure
                var commandUpdateReferrerPoints = "EXEC UpdateReferrerPoints @ReferredCustomerID, @AmountSpent";
                var amountSpentParam = new Microsoft.Data.SqlClient.SqlParameter("@AmountSpent", amountSpent);
                await _context.Database.ExecuteSqlRawAsync(commandUpdateReferrerPoints, referredCustomerIdParam, amountSpentParam);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing referral logic for customer {CustomerId}", referredCustomerId);
            }
        }
    }
}
