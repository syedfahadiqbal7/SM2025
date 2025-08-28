using AFFZ_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AFFZ_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayoutSettingsController : ControllerBase
    {
        private readonly MyDbContext _context;

        public PayoutSettingsController(MyDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<PayoutSettings>> GetPayoutSettings()
        {
            var settings = await _context.PayoutSettings.FirstOrDefaultAsync();
            return settings ?? new PayoutSettings();
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePayoutSettings(PayoutSettings settings)
        {
            var existingSettings = await _context.PayoutSettings.FirstOrDefaultAsync();

            if (existingSettings != null)
            {
                _context.Entry(existingSettings).CurrentValues.SetValues(settings);
            }
            else
            {
                _context.PayoutSettings.Add(settings);
            }

            await _context.SaveChangesAsync();
            return Ok();
        }
    }

}
