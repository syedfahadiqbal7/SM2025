using AFFZ_Customer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AFFZ_Customer.Controllers
{
    public class ChooseSignupController : Controller
    {
        private readonly string _userUrl;
        private readonly string _providerUrl;
        public ChooseSignupController(IOptions<AppSettings> appSettings)
        {
            _userUrl = appSettings.Value.CustomerHttpsPort;
            _providerUrl = appSettings.Value.MerchantHttpsPort;
        }
        public IActionResult ChooseSignupIndex()
        {
            ViewBag.UserUrl = _userUrl;
            ViewBag.ProviderUrl = _providerUrl;
            return View();
        }
    }
}
