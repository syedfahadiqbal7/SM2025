using AFFZ_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AFFZ_API.Controllers.AdminControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenusController : ControllerBase
    {
        private readonly MyDbContext _context;

        public MenusController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/Menus
        [HttpGet]
        [Route("GetMenus")]
        public async Task<ActionResult<IEnumerable<Menu>>> GetMenus()
        {
            return await _context.Menus.ToListAsync();
        }

        [HttpGet]
        [Route("GetMenusByRoleId")]
        public async Task<ActionResult<IEnumerable<Menu>>> GetMenusByRoleId(int roleId)
        {
            try
            {
                var temp = await _context.Menus
                            .Where(menu => _context.Permissions.Any(permission => permission.MenuId == menu.MenuId && permission.CanView == true) && menu.UserType == "Customer")
                            .ToListAsync();
                return temp;
            }

            catch (Exception ex)
            {
                return null;
            }
        }

        // GET: api/Menus
        [HttpGet]
        [Route("GetMenusPermissionByRoleId")]
        public async Task<ActionResult<IEnumerable<Menu>>> GetAllMenusPermissionByRoleId(int roleId)
        {
            var menus = await _context.Menus
                          .Include(m => m.Permissions)
                          .ThenInclude(p => p.Role)
                          .ToListAsync();
            foreach (var menu in menus)
            {
                menu.Permissions = menu.Permissions.Where(p => p.Role.RoleId == roleId).ToList();
            }


            return menus;
        }
        [HttpGet]
        [Route("GetMenusByUserType")]
        public async Task<ActionResult<IEnumerable<Menu>>> GetMenusByUserType(string userType)
        {
            try
            {
                // Filter menus based on the provided userType
                var menus = await _context.Menus
                    .Where(menu => menu.UserType == userType && _context.Permissions.Any(permission => permission.MenuId == menu.MenuId && permission.CanView == true))
                    .Include(menu => menu.SubMenus)
                    .ToListAsync();

                return Ok(menus);
            }
            catch (Exception ex)
            {
                // Log error (if needed)
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching menus.");
            }
        }
        // GET: api/Menus/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Menu>> GetMenu(int id)
        {
            var menu = await _context.Menus.FindAsync(id);

            if (menu == null)
            {
                return NotFound();
            }

            return menu;
        }

        // PUT: api/Menus/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMenu(int id, Menu menu)
        {
            if (id != menu.MenuId)
            {
                return BadRequest();
            }

            _context.Entry(menu).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MenuExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Menus
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Menu>> PostMenu(Menu menu)
        {
            _context.Menus.Add(menu);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMenu", new { id = menu.MenuId }, menu);
        }

        // DELETE: api/Menus/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMenu(int id)
        {
            var menu = await _context.Menus.FindAsync(id);
            if (menu == null)
            {
                return NotFound();
            }

            _context.Menus.Remove(menu);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MenuExists(int id)
        {
            return _context.Menus.Any(e => e.MenuId == id);
        }
    }
}
