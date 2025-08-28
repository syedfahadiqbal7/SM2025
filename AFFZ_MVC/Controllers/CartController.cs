using AFFZ_Customer.Models;
using AFFZ_Customer.Utils;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;

namespace AFFZ_Customer.Controllers
{
    public class CartController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CartController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        IDataProtector _protector;
        public CartController(IHttpClientFactory httpClientFactory, ILogger<CartController> logger, IHttpContextAccessor httpContextAccessor, IDataProtectionProvider provider)
        {
            _httpClient = httpClientFactory.CreateClient("Main");
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _protector = provider.CreateProtector("Example.SessionProtection");
        }
        // View Cart
        public async Task<IActionResult> Index(int customerId)
        {
            try
            {
                customerId = Convert.ToInt32(_httpContextAccessor.HttpContext.Session.GetEncryptedString("UserId", _protector));
                var response = await _httpClient.GetAsync($"Cart/{customerId}");
                response.EnsureSuccessStatusCode();
                var responseString = await response.Content.ReadAsStringAsync();
                SResponse sResponse = JsonConvert.DeserializeObject<SResponse>(responseString);

                if (sResponse.StatusCode == HttpStatusCode.OK)
                {
                    var cart = JsonConvert.DeserializeObject<Cart>(sResponse.Data.ToString());
                    return View("Cart", cart);
                }

                return View("Cart", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching the cart.");
                return RedirectToAction("Error", "Home");
            }
        }

        // Add to Cart
        [HttpPost]
        public async Task<IActionResult> AddToCart(CartItem cartItem)
        {
            try
            {
                var response = await _httpClient.PostAsync("Cart/AddToCart", Customs.GetJsonContent(cartItem));
                response.EnsureSuccessStatusCode();
                return RedirectToAction("Index", new { customerId = cartItem.CartID });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding the product to the cart.");
                return RedirectToAction("Error", "Home");
            }
        }

        // Remove from Cart
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"Cart/RemoveFromCart/{cartItemId}");
                response.EnsureSuccessStatusCode();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while removing the product from the cart.");
                return RedirectToAction("Error", "Home");
            }
        }
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            try
            {
                var response = await _httpClient.PostAsync($"Cart/UpdateCart?cartItemId={cartItemId}&quantity={quantity}", null);
                response.EnsureSuccessStatusCode();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while removing the product from the cart.");
                return RedirectToAction("Error", "Home");
            }
        }
    }
}
