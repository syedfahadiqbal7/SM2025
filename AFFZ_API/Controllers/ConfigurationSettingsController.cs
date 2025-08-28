using AFFZ_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AFFZ_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigurationSettingsController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly ILogger<ConfigurationSettingsController> _logger;

        public ConfigurationSettingsController(MyDbContext context, ILogger<ConfigurationSettingsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ConfigurationSettings>> GetSettings()
        {
            try
            {
                var setting = await _context.ConfigurationSettings.FirstOrDefaultAsync();
                if (setting == null)
                {
                    return NotFound("Configuration settings not found.");
                }
                return Ok(setting);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching the configuration settings.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> UpdateSettings(int id, ConfigurationSettings settings)
        {
            if (id != settings.Id)
            {
                return BadRequest("ID mismatch.");
            }

            try
            {
                _context.Entry(settings).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the configuration settings.");
                return StatusCode(500, "Internal server error.");
            }
        }
    }

}
