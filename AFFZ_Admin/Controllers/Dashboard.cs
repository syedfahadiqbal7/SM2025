using Microsoft.AspNetCore.Mvc;

namespace AFFZ_Admin.Controllers
{
    [Route("AdminDashboard")]
    public class Dashboard : Controller
    {
        public IActionResult Index()
        {
            return View("Dashboard");
        }
    }
}
