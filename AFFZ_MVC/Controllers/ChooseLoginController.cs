using AFFZ_Customer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AFFZ_Customer.Controllers
{
    public class ChooseLoginController : Controller
    {
        private readonly string _userUrl;
        private readonly string _providerUrl;
        public ChooseLoginController(IOptions<AppSettings> appSettings)
        {
            _userUrl = appSettings.Value.CustomerHttpsPort;
            _providerUrl = appSettings.Value.MerchantHttpsPort;
        }
        public IActionResult ChooseLoginIndex()
        {
            ViewBag.UserUrl = _userUrl;
            ViewBag.ProviderUrl = _providerUrl;

            return View();
        }
    }
}
