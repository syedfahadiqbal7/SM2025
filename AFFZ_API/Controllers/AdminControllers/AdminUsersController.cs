using AFFZ_API.Models;
using AFFZ_API.Models.Partial;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace AFFZ_API.Controllers.AdminControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminUsersController : ControllerBase
    {
        private readonly MyDbContext _context;

        public AdminUsersController(MyDbContext context)
        {
            _context = context;
        }


        [HttpPost]
        [Route("Login")]
        public async Task<ActionResult<SResponse>> Login(LoginModel loginDetail)
        {
            try
            {
                AdminUser adminDetail = await _context.AdminUsers.Where(x => x.Email == loginDetail.Email && x.Password == loginDetail.Password).FirstAsync();

                if (adminDetail == null)
                {
                    return new SResponse
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = "Your Email/Password is wrong!",
                    };
                }

                return new SResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = "Login Success!",
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


        // GET: api/AdminUsers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdminUser>>> GetAdminUsers()
        {
            return await _context.AdminUsers.ToListAsync();
        }

        // GET: api/AdminUsers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AdminUser>> GetAdminUser(int id)
        {
            var adminUser = await _context.AdminUsers.FindAsync(id);

            if (adminUser == null)
            {
                return NotFound();
            }

            return adminUser;
        }

        // PUT: api/AdminUsers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAdminUser(int id, AdminUser adminUser)
        {
            if (id != adminUser.UserId)
            {
                return BadRequest();
            }

            _context.Entry(adminUser).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AdminUserExists(id))
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

        // POST: api/AdminUsers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<AdminUser>> PostAdminUser(AdminUser adminUser)
        {
            _context.AdminUsers.Add(adminUser);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAdminUser", new { id = adminUser.UserId }, adminUser);
        }

        // DELETE: api/AdminUsers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdminUser(int id)
        {
            var adminUser = await _context.AdminUsers.FindAsync(id);
            if (adminUser == null)
            {
                return NotFound();
            }

            _context.AdminUsers.Remove(adminUser);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AdminUserExists(int id)
        {
            return _context.AdminUsers.Any(e => e.UserId == id);
        }
    }
}
