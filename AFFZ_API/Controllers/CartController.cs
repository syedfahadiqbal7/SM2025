using AFFZ_API.Models;
using AFFZ_API.Models.Partial;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace AFFZ_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly ILogger<CartController> _logger;

        public CartController(MyDbContext context, ILogger<CartController> logger)
        {
            _context = context;
            _logger = logger;
        }
        // GET: api/Cart/GetCartItems/{customerId}
        [HttpGet("GetCartItems/{customerId}")]
        public async Task<ActionResult<SResponse>> GetCartItems(int customerId)
        {
            try
            {
                var cart = await _context.Cart
                    .Include(c => c.CartItem)
                    .Where(c => c.CustomerID == customerId)
                    .FirstOrDefaultAsync();

                if (cart == null || cart.CartItem == null || cart.CartItem.Count == 0)
                {
                    return new SResponse
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = "No items found in the cart"
                    };
                }

                var cartItems = new List<CartItemDTO>();

                foreach (var cartItem in cart.CartItem)
                {
                    var service = await (from s in _context.Services
                                         join sl in _context.ServicesLists
                                         on s.SID equals sl.ServiceListID
                                         where s.ServiceId == cartItem.ServiceID
                                         select new
                                         {
                                             s.ServiceId,
                                             s.ServicePrice,
                                             ServiceName = sl.ServiceName
                                         }).FirstOrDefaultAsync();

                    if (service != null)
                    {
                        cartItems.Add(new CartItemDTO
                        {
                            CartID = cartItem.CartID,
                            CartItemID = cartItem.CartItemID,
                            CustomerId = customerId,
                            ServiceID = cartItem.ServiceID,
                            ServiceName = service.ServiceName, // Correctly assign the ServiceName here
                            ServicePrice = (decimal)service.ServicePrice,
                            Quantity = cartItem.Quantity,
                            AddedDate = cartItem.AddedDate
                        });
                    }
                }


                return new SResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Data = cartItems
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching cart items.");
                return new SResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = $"Exception: {ex.Message}"
                };
            }
        }


        // GET: api/Cart/{customerId}
        [HttpGet("{customerId}")]
        public async Task<ActionResult<SResponse>> GetCart(int customerId)
        {
            try
            {
                var cart = await _context.Cart
                    .Include(c => c.CartItem)
                    .Where(c => c.CustomerID == customerId)
                    .FirstOrDefaultAsync();

                if (cart == null || cart.CartItem == null || cart.CartItem.Count == 0)
                {
                    return new SResponse
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = "No items found in the cart"
                    };
                }

                var cartItems = new List<CartItemDTO>();

                foreach (var cartItem in cart.CartItem)
                {
                    var service = await (from s in _context.Services
                                         join sl in _context.ServicesLists
                                         on s.SID equals sl.ServiceListID
                                         where s.ServiceId == cartItem.ServiceID
                                         select new
                                         {
                                             s.ServiceId,
                                             s.ServicePrice,
                                             s.MerchantID,
                                             ServiceName = sl.ServiceName
                                         }).FirstOrDefaultAsync();

                    if (service != null)
                    {
                        cartItems.Add(new CartItemDTO
                        {
                            CartID = cartItem.CartID,
                            CartItemID = cartItem.CartItemID,
                            CustomerId = customerId,
                            ServiceID = cartItem.ServiceID,
                            MID = service.MerchantID,
                            ServiceName = service.ServiceName, // Correctly assign the ServiceName here
                            ServicePrice = (decimal)service.ServicePrice,
                            Quantity = cartItem.Quantity,
                            AddedDate = cartItem.AddedDate
                        });
                    }
                }

                var cartData = new CartItems()
                {
                    CartID = cart.CartID,
                    CreatedDate = cart.CreatedDate,
                    CustomerID = customerId,
                    CartItem = cartItems
                };

                return new SResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Data = cartData
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching cart items.");
                return new SResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = $"Exception: {ex.Message}"
                };
            }
        }

        // POST: api/Cart/AddToCart
        [HttpPost("AddToCart")]
        public async Task<ActionResult<SResponse>> AddToCart(CartRequest cartreq)
        {
            _logger.LogInformation("AddToCart initiated for CustomerId: {CustomerId}, ServiceId: {ServiceId}", cartreq.CustomerID, cartreq.ServiceID);
            try
            {
                // Step 1: Check if the user already has a cart
                var existingCart = await _context.Cart.FirstOrDefaultAsync(c => c.CustomerID == cartreq.CustomerID);

                if (existingCart == null)
                {
                    // Step 2: Create a new cart if none exists
                    existingCart = new Cart
                    {
                        CustomerID = cartreq.CustomerID,
                        CreatedDate = DateTime.Now
                    };
                    _context.Cart.Add(existingCart);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("New cart created for CustomerId: {CustomerId}", cartreq.CustomerID);
                }

                // Step 3: Check if the service already exists in the cart to avoid duplicates
                var existingCartItem = await _context.CartItem
                    .FirstOrDefaultAsync(ci => ci.CartID == existingCart.CartID && ci.ServiceID == cartreq.ServiceID);

                if (existingCartItem != null)
                {
                    return new SResponse
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Message = "Service is already in the cart."
                    };
                }

                // Step 4: Add the service to the cart
                var newCartItem = new CartItem
                {
                    CartID = existingCart.CartID,
                    ServiceID = cartreq.ServiceID,
                    Quantity = cartreq.Quantity, // Assuming default quantity is 1
                    AddedDate = DateTime.Now
                };

                _context.CartItem.Add(newCartItem);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Service successfully added to cart for CustomerId: {CustomerId}, ServiceId: {ServiceId}", cartreq.CustomerID, cartreq.ServiceID);

                return new SResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = "Service added to cart successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding service to cart.");
                return new SResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = $"An error occurred while adding the service to the cart: {ex.Message}"
                };
            }
        }

        // POST: api/Cart/AddToCart
        [HttpPost("UpdateCart")]
        public async Task<ActionResult<SResponse>> UpdateCart(int cartItemId, int quantity)
        {
            _logger.LogInformation("UpdateCart initiated for CartItemId: {cartItemId}, Quantity: {quantity}", cartItemId, quantity);
            try
            {
                // Step 1: Check if the cart item exists
                var cartItem = await _context.CartItem.FindAsync(cartItemId);
                if (cartItem == null)
                {
                    return new SResponse
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = "Cart item not found"
                    };
                }

                // Step 2: Update the quantity of the cart item
                if (quantity <= 0)
                {
                    // If the quantity is less than or equal to zero, remove the item from the cart
                    _context.CartItem.Remove(cartItem);
                    _logger.LogInformation("Cart item removed due to quantity being zero or less.");
                }
                else
                {
                    cartItem.Quantity = quantity;
                    _context.CartItem.Update(cartItem);
                    _logger.LogInformation("Cart item quantity updated to: {quantity}", quantity);
                }

                await _context.SaveChangesAsync();

                return new SResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = "Cart item successfully updated"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating cart item.");
                return new SResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = $"An error occurred while updating the cart item: {ex.Message}"
                };
            }
        }

        // DELETE: api/Cart/RemoveFromCart/{cartItemId}
        [HttpDelete("RemoveFromCart/{cartItemId}")]
        public async Task<ActionResult<SResponse>> RemoveFromCart(int cartItemId)
        {
            try
            {
                var cartItem = await _context.CartItem.FindAsync(cartItemId);
                if (cartItem == null)
                {
                    return new SResponse
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = "Cart item not found"
                    };
                }

                _context.CartItem.Remove(cartItem);
                await _context.SaveChangesAsync();

                return new SResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = "Product successfully removed from the cart"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while removing the product from the cart.");
                return new SResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = $"Exception: {ex.Message}",
                };
            }
        }


    }
    public class CartRequest
    {
        public int CustomerID { get; set; }
        public int ServiceID { get; set; }
        public int Quantity { get; set; }
    }
}
