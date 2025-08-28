using AFFZ_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AFFZ_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceCategoryController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly ILogger<ServiceCategoryController> _logger;
        public ServiceCategoryController(MyDbContext context, ILogger<ServiceCategoryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/ServiceCategory
        [HttpGet("GetAllServiceCategories")]
        public async Task<ActionResult<IEnumerable<ServiceCategory>>> GetAllServiceCategories()
        {
            return await _context.ServiceCategories.Include(sc => sc.Services).ToListAsync();
        }

        // GET: api/ServiceCategory/5
        [HttpGet("GetServiceCategoryById")]
        public async Task<ActionResult<ServiceCategory>> GetServiceCategoryById(int id)
        {
            var serviceCategory = await _context.ServiceCategories
                .Include(sc => sc.Services)
                .FirstOrDefaultAsync(sc => sc.CategoryId == id);

            if (serviceCategory == null)
            {
                return NotFound();
            }

            return serviceCategory;
        }

        // POST: api/ServiceCategory
        [HttpPost("PostServiceCategory")]
        public async Task<ActionResult<ServiceCategory>> PostServiceCategory(ServiceCategory serviceCategory)
        {
            try
            {
                _context.ServiceCategories.Add(serviceCategory);
                await _context.SaveChangesAsync();
                return CreatedAtAction("GetServiceCategory", new { id = serviceCategory.CategoryId }, serviceCategory);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        // PUT: api/ServiceCategory/5
        [HttpPost("UpdateServiceCategory")]
        public async Task<IActionResult> UpdateServiceCategory(int id, ServiceCategory serviceCategory)
        {
            if (id != serviceCategory.CategoryId)
            {
                return BadRequest();
            }

            _context.Entry(serviceCategory).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceCategoryExists(id))
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

        // DELETE: api/ServiceCategory/5
        [HttpPost("DeleteServiceCategory")]
        public async Task<IActionResult> DeleteServiceCategory(int id)
        {
            try
            {
                var serviceCategory = await _context.ServiceCategories.FindAsync(id);
                if (serviceCategory == null)
                {
                    return NotFound();
                }

                _context.ServiceCategories.Remove(serviceCategory);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private bool ServiceCategoryExists(int id)
        {
            return _context.ServiceCategories.Any(e => e.CategoryId == id);
        }
    }
}
