using AFFZ_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AFFZ_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly ILogger<MenuController> _logger;
        public MenuController(MyDbContext context, ILogger<MenuController> logger)
        {
            _context = context;
            _logger = logger;
        }
        [HttpGet("GetAllMenus")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllMenus(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Fetching all menus for Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);

                var paginatedMenus = await _context.Menus
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var totalRecords = await _context.Menus.CountAsync();
                Response.Headers.Add("X-Total-Count", totalRecords.ToString());

                return Ok(paginatedMenus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching menus.");
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        // GET: api/Service/5
        [HttpGet("GetMenuById")]
        public async Task<ActionResult<Menu>> GetMenuById(int id)
        {
            _logger.LogInformation("Fetching menu with ID: {Id}", id);
            var menu = await _context.Menus.FindAsync(id);

            if (menu == null)
            {
                _logger.LogWarning("Menu with ID: {Id} not found", id);
                return NotFound();
            }

            return Ok(menu);
        }


        // PUT: api/Service/5
        [HttpPost("UpdateMenu")]
        public async Task<IActionResult> UpdateMenu(int id, Menu menu)
        {
            if (id != menu.MenuId)
            {
                _logger.LogWarning("Menu ID mismatch: {RequestId} != {MenuId}", id, menu.MenuId);
                return BadRequest("ID mismatch");
            }

            _context.Entry(menu).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Menu updated successfully: {Id}", id);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating menu ID: {Id}", id);
                if (!MenuExists(id))
                    return NotFound();
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating menu ID: {Id}", id);
                return StatusCode(500, "An internal server error occurred.");
            }

            return NoContent();
        }
        // POST: api/Service
        [HttpPost("CreateMenu")]
        public async Task<IActionResult> CreateMenu([FromBody] Menu menu)
        {
            try
            {
                if (menu == null || !ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid menu model received.");
                    return BadRequest("Invalid request data.");
                }

                if (MenuNameExists(menu.MenuName))
                {
                    _logger.LogWarning("Menu with name '{MenuName}' already exists.", menu.MenuName);
                    return Conflict("This menu already exists.");
                }

                _context.Menus.Add(menu);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Menu created with ID: {Id}", menu.MenuId);
                return Ok(new { menu.MenuId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating menu.");
                return StatusCode(500, "An internal server error occurred.");
            }
        }
        // DELETE: api/Service/5
        [HttpDelete("DeleteMenu")]
        public async Task<IActionResult> DeleteMenu(int id)
        {
            try
            {
                _logger.LogInformation("Attempting to delete menu with ID: {Id}", id);
                var menu = await _context.Menus.FindAsync(id);
                if (menu == null)
                {
                    _logger.LogWarning("Menu with ID: {Id} not found for deletion", id);
                    return NotFound();
                }

                _context.Menus.Remove(menu);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Menu with ID: {Id} deleted successfully", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting menu with ID: {Id}", id);
                return StatusCode(500, "An internal server error occurred.");
            }
        }
        [HttpGet("MenuNameById")]
        public async Task<ActionResult<string>> MenuName(int id)
        {
            try
            {
                var menu = await _context.Menus.FindAsync(id);
                if (menu == null)
                {
                    _logger.LogWarning("Menu with ID: {Id} not found for name retrieval", id);
                    return NotFound();
                }

                return Ok(menu.MenuName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching menu name for ID: {Id}", id);
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        private bool MenuExists(int id)
        {
            return _context.Menus.Any(e => e.MenuId == id);
        }

        private bool MenuNameExists(string menuName)
        {
            return _context.Menus.Any(e => e.MenuName == menuName);
        }
    }
}
