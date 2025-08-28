using Microsoft.AspNetCore.Mvc;

namespace AFFZ_Admin.Controllers
{
    public class VerificationRequests : Controller
    {
        public IActionResult Index()
        {
            return View("VerificationRequests");
        }
    }
}
