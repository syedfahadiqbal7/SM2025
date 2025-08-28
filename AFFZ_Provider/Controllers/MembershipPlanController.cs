using AFFZ_Provider.Models;
using AFFZ_Provider.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Stripe;
using Stripe.Checkout;
namespace AFFZ_Provider.Controllers
{
    [Authorize]
    public class MembershipPlanController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MembershipPlanController> _logger;
        IDataProtector _protector;
        private string BaseUrl = string.Empty;
        public string BaseUrlIp = string.Empty;
        private string PublicDomain = string.Empty;
        private string ApiHttpsPort = string.Empty;
        private string CustomerHttpsPort = string.Empty;
        private string ProviderHttpsPort = string.Empty;

        public MembershipPlanController(IHttpClientFactory httpClientFactory, ILogger<MembershipPlanController> logger, IDataProtectionProvider provider, IAppSettingsService service, IOptions<AppSettings> options)
        {
            _httpClient = httpClientFactory.CreateClient("Main");
            _protector = provider.CreateProtector("Example.SessionProtection");
            _logger = logger;
            BaseUrl = service.GetBaseIpAddress();
            BaseUrlIp = options.Value.BaseIpAddress;
            PublicDomain = service.GetPublicDomain();
            ApiHttpsPort = service.GetApiHttpsPort();
            CustomerHttpsPort = service.GetCustomerHttpsPort();
            ProviderHttpsPort = service.GetMerchantHttpsPort();
        }

        public async Task<IActionResult> MembershipIndex()
        {
            int MerchantId = Convert.ToInt32(HttpContext.Session.GetEncryptedString("ProviderId", _protector));
            List<MembershipPlan> plans = new List<MembershipPlan>();
            try
            {
                var response = await _httpClient.GetAsync("MembershipPlans");
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    plans = JsonConvert.DeserializeObject<List<MembershipPlan>>(responseString);
                    if (plans.Count > 0)
                    {
                        var mspRequest = await _httpClient.GetAsync("MembershipPayment/CheckMemberShip?merchantId=" + MerchantId);
                        if (mspRequest.IsSuccessStatusCode)
                        {
                            var mspResponseString = await mspRequest.Content.ReadAsStringAsync();
                            var mspMembership = JsonConvert.DeserializeObject<MembershipPaymentHistory>(mspResponseString);
                            if (mspMembership != null)
                            {
                                ViewBag.MembershipId = mspMembership.MembershipId;
                            }
                            else
                            {
                                ViewBag.MembershipId = "0";
                            }
                        }
                        else
                        {
                            TempData["FailMessage"] = "Failed to fetch membership plans.";
                        }
                    }
                }
                else
                {
                    TempData["FailMessage"] = "Failed to fetch membership plans.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching membership plans");
                TempData["FailMessage"] = "An error occurred while fetching membership plans.";
            }

            return View(plans);
        }

        public ActionResult PaymentGateway(string price, string Duration, string Servicename, string MembershipId)
        {
            int MerchantId = Convert.ToInt32(HttpContext.Session.GetEncryptedString("ProviderId", _protector));
            _logger.LogDebug("Storing relevant data in session.");
            HttpContext.Session.SetEncryptedString("price", price, _protector);
            HttpContext.Session.SetEncryptedString("Duration", Duration, _protector);
            HttpContext.Session.SetEncryptedString("payerId", MerchantId.ToString(), _protector);
            HttpContext.Session.SetEncryptedString("MembershipId", MembershipId, _protector);
            HttpContext.Session.SetEncryptedString("NoOfQuantity", "1", _protector);
            HttpContext.Session.SetEncryptedString("Servicename", Servicename, _protector);



            // Get Stripe API key from configuration
            var stripeSecretKey = HttpContext.RequestServices.GetService<IOptions<AppSettings>>()?.Value?.StripeSecretKey;
            if (string.IsNullOrEmpty(stripeSecretKey))
            {
                _logger.LogError("Stripe secret key not configured");
                return BadRequest("Payment gateway configuration error");
            }
            StripeConfiguration.ApiKey = stripeSecretKey;
            var domain = ProviderHttpsPort + "/MembershipPlan/";
            var optionsProduct = new ProductCreateOptions
            {
                Name = Servicename,
                Description = "Merchant is purchasing the " + Servicename + " membership plan",
            };
            var serviceProduct = new ProductService();
            Product product = serviceProduct.Create(optionsProduct);
            // Console.Write("Success! Here is your starter subscription product id: {0}\n", product.Id);
            double priceValue = Convert.ToDouble(price);

            // Multiply by 100 and then convert to long
            long finalPrice = Convert.ToInt64(priceValue * 100);
            var optionsPrice = new PriceCreateOptions
            {
                UnitAmount = finalPrice,
                Currency = "aed",
                //Recurring = new PriceRecurringOptions
                //{
                //    Interval = "one-time",
                //},
                Product = product.Id
            };
            var servicePrice = new PriceService();
            Price _price = servicePrice.Create(optionsPrice);
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>
                {
                  new SessionLineItemOptions
                  {
                    // Provide the exact Price ID (for example, pr_1234) of the product you want to sell
                    Price = _price.Id,
                    Quantity = 1,
                  },
                },
                Mode = "payment",
                SuccessUrl = domain + "success",
                CancelUrl = domain + "cancel",


            };
            var service = new SessionService();
            Session session = service.Create(options);

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }
        [HttpGet]
        public async Task<ActionResult> Cancel()
        {
            _logger.LogDebug("Cancel action called.");
            try
            {
                // Retrieve session values and validate them
                string amount = HttpContext.Session.GetEncryptedString("price", _protector);
                string Duration = HttpContext.Session.GetEncryptedString("Duration", _protector);
                string MembershipId = HttpContext.Session.GetEncryptedString("MembershipId", _protector);
                int payerId = Convert.ToInt32(HttpContext.Session.GetEncryptedString("payerId", _protector));
                string Servicename = HttpContext.Session.GetEncryptedString("Servicename", _protector);
                int NoOfQuantity = Convert.ToInt32(HttpContext.Session.GetEncryptedString("NoOfQuantity", _protector));
                int MerchantId = Convert.ToInt32(HttpContext.Session.GetEncryptedString("ProviderId", _protector));
                _logger.LogDebug("Session values retrieved: amount: {Amount}", amount);


                var paymentHistory = new MembershipPaymentHistory
                {
                    PAYMENTTYPE = "Online",
                    AMOUNT = amount,
                    PAYERID = payerId,
                    Duration = Duration,
                    ISPAYMENTSUCCESS = 0,
                    MembershipId = Convert.ToInt32(MembershipId),
                    PAYMENTDATETIME = DateTime.Now,
                    Quantity = NoOfQuantity,
                    MemberShipname = Servicename
                };

                // Save payment history via API call
                _logger.LogDebug("Saving payment history via API.");
                var responseMessage = await _httpClient.PostAsync("MembershipPayment/sendRequestToSaveMembershipPayment", Customs.GetJsonContent(paymentHistory));
                if (!responseMessage.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to save payment history.");
                    return StatusCode(500, "Failed to save payment history.");
                }


                // Update discount information via API call
                //_logger.LogDebug("Updating discount information via API.");
                //responseMessage = await _httpClient.PostAsync("Payment/UpdateRequestForDisCountToUserForPaymentDone", Customs.GetJsonContent(discountUpdateInfo));
                //if (!responseMessage.IsSuccessStatusCode)
                //{
                //_logger.LogWarning("Failed to update discount information.");
                //return StatusCode(500, "Failed to update discount information.");
                //}



                // Create notification object to notify merchant
                _logger.LogDebug("Creating notification object.");
                var notification = new Notification
                {
                    UserId = payerId.ToString(),
                    Message = $"Merchant[{payerId.ToString()}] your membership plan was not completed because your payment was declined. Please try again.",
                    MerchantId = payerId.ToString(),
                    RedirectToActionUrl = "",
                    MessageFromId = Convert.ToInt32(payerId),
                    SenderType = "Merchant",
                    SID = 0
                };

                // Send notification request via API
                _logger.LogDebug("Sending notification request via API.");
                var res = await _httpClient.PostAsync("Notifications/CreateNotification?StatusId=11", Customs.GetJsonContent(notification));
                string resString = await res.Content.ReadAsStringAsync();
                _logger.LogInformation("Notification Response: {Response}", resString);

                // Set success response in TempData and return view
                _logger.LogDebug("Setting success response and returning view.");
                return View();
            }
            catch (Exception ex)
            {
                // Log the error and return a 500 status code
                _logger.LogError(ex, "An error occurred while processing the payment.");
                return StatusCode(500, "An internal server error occurred. Please try again later.");
            }
        }
        [HttpGet]
        public async Task<ActionResult> success()
        {
            try
            {
                string amount = HttpContext.Session.GetEncryptedString("price", _protector);
                string Duration = HttpContext.Session.GetEncryptedString("Duration", _protector);
                string MembershipId = HttpContext.Session.GetEncryptedString("MembershipId", _protector);
                int payerId = Convert.ToInt32(HttpContext.Session.GetEncryptedString("payerId", _protector));
                string Servicename = HttpContext.Session.GetEncryptedString("Servicename", _protector);
                int NoOfQuantity = Convert.ToInt32(HttpContext.Session.GetEncryptedString("NoOfQuantity", _protector));
                //"Duration":["The Duration field is required."],"MembershipId":["The MembershipId field is required."],"MemberShipname":["The MemberShipname field is required."]
                var paymentHistory = new MembershipPaymentHistory
                {
                    PAYMENTTYPE = "Online",
                    AMOUNT = amount,
                    PAYERID = payerId,
                    Duration = Duration,
                    ISPAYMENTSUCCESS = 1,
                    MembershipId = Convert.ToInt32(MembershipId),
                    PAYMENTDATETIME = DateTime.Now,
                    Quantity = NoOfQuantity,
                    MemberShipname = Servicename
                };

                var responseMessage = await _httpClient.PostAsync("MembershipPayment/sendRequestToSaveMembershipPayment", Customs.GetJsonContent(paymentHistory));
                var responseString = await responseMessage.Content.ReadAsStringAsync();
                MembershipPaymentHistory UpdatedPaymentHistory = JsonConvert.DeserializeObject<MembershipPaymentHistory>(responseString);
                if (UpdatedPaymentHistory.ID > 0)
                {


                    // Trigger notification
                    var notification = new Notification
                    {
                        UserId = payerId.ToString(),
                        Message = $"Merchant[{payerId.ToString()}] has paid the amount for membership [{Servicename}] plan and it has been received. Please contact the admin to update your services.",
                        MerchantId = payerId.ToString(),
                        RedirectToActionUrl = "",
                        MessageFromId = Convert.ToInt32(payerId),
                        SenderType = "Merchant",
                        SID = 0
                    };

                    var res = await _httpClient.PostAsync("Notifications/CreateNotification?StatusId=11", Customs.GetJsonContent(notification));
                    string resString = await res.Content.ReadAsStringAsync();
                    _logger.LogInformation("Notification Response : " + resString);
                }

                TempData["SuccessResponse"] = responseString;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the payment.");
                return StatusCode(500, "An internal server error occurred. Please try again later.");
            }
        }

    }
}
