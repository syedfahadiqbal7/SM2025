using AFFZ_Customer.Utils;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;


namespace AFFZ_Customer.Controllers
{
    //[Route("UserBookings")]
    public class Bookings : Controller
    {
        private readonly IDataProtector _protector;

        public Bookings(IHttpClientFactory httpClientFactory, IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("Example.SessionProtection");
        }

        //[Authorize(Policy = "AdminPolicy")]
        public IActionResult Index()
        {
            ViewBag.CustomerId = HttpContext.Session.GetEncryptedString("UserId", _protector);
            ViewBag.FirstName = HttpContext.Session.GetEncryptedString("FirstName", _protector);
            ViewBag.MemberSince = HttpContext.Session.GetEncryptedString("MemberSince", _protector);
            return View("Bookings");
        }
    }
}
