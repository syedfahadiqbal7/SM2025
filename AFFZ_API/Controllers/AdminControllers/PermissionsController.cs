using AFFZ_API.Models;
using AFFZ_API.Models.Partial;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace AFFZ_API.Controllers.AdminControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionsController : ControllerBase
    {
        private readonly MyDbContext _context;

        public PermissionsController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/Permissions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Permission>>> GetPermissions()
        {
            return await _context.Permissions.ToListAsync();
        }

        // GET: api/Permissions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Permission>> GetPermission(int id)
        {
            var permission = await _context.Permissions.FindAsync(id);

            if (permission == null)
            {
                return NotFound();
            }

            return permission;
        }

        [HttpPost]
        [Route("UpdatePermissions")]
        public async Task<SResponse> UpdatePermissions(List<Permission> permissions)
        {
            try
            {
                foreach (Permission model in permissions)
                {
                    var existingPermission = await _context.Permissions.Where(x => x.RoleId == model.RoleId && x.MenuId == model.MenuId).FirstOrDefaultAsync();

                    //update
                    if (existingPermission != null)
                    {
                        existingPermission.CanCreate = model.CanCreate;
                        existingPermission.CanRead = model.CanRead;
                        existingPermission.CanUpdate = model.CanUpdate;
                        existingPermission.CanDelete = model.CanDelete;
                        existingPermission.CanView = model.CanView;
                    }
                    //add new
                    else
                    {
                        model.CreatedBy = 1;
                        _context.Permissions.Add(model);
                    }
                }
                await _context.SaveChangesAsync();
                return new SResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = "Permissions Updated!",
                };
            }
            catch (Exception ex)
            {
                return new SResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = $"Exception: {ex.Message}",
                };
            }
        }

        // PUT: api/Permissions/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPermission(int id, Permission permission)
        {
            if (id != permission.PermissionId)
            {
                return BadRequest();
            }

            _context.Entry(permission).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PermissionExists(id))
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

        // POST: api/Permissions
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Permission>> PostPermission(Permission permission)
        {
            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPermission", new { id = permission.PermissionId }, permission);
        }

        // DELETE: api/Permissions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePermission(int id)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null)
            {
                return NotFound();
            }

            _context.Permissions.Remove(permission);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PermissionExists(int id)
        {
            return _context.Permissions.Any(e => e.PermissionId == id);
        }
    }
}
