using AFFZ_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AFFZ_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceDocumentListBindingsController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly ILogger<ServiceDocumentListBindingsController> _logger;

        public ServiceDocumentListBindingsController(MyDbContext context, ILogger<ServiceDocumentListBindingsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/ServiceDocumentListBindings
        [HttpGet("GetServiceDocumentListBindings")]
        public async Task<ActionResult<IEnumerable<object>>> GetServiceDocumentListBindings(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var paginatedServices = await _context.M_ServiceDocumentListBinding
                    .Select(binding => new
                    {
                        binding.Id,
                        CategoryID = _context.ServiceCategories
                            .Where(cats => cats.CategoryId == binding.CategoryID)
                            .Select(cats => cats.CategoryName)
                            .FirstOrDefault(),
                        ServiceDocumentListId = _context.ServicesLists
                            .Where(docs => docs.ServiceListID == binding.ServiceDocumentListId)
                            .Select(docs => docs.ServiceName)
                            .FirstOrDefault()
                    })
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var totalRecords = await _context.M_ServiceDocumentListBinding.CountAsync();

                // Return paginated list and total count in headers
                Response.Headers.Add("X-Total-Count", totalRecords.ToString());

                return Ok(paginatedServices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching the service document list bindings.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("GetBindingById")]
        public async Task<ActionResult<M_SericeDocumentListBinding>> GetBindingById(int id)
        {
            try
            {
                var databind = await _context.M_ServiceDocumentListBinding.Where(binding => binding.Id == id).FirstOrDefaultAsync();
                if (databind == null)
                {
                    return NotFound();
                }

                return databind;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching the service document list binding by ID.");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("GetBindingByServiceId")]
        public async Task<ActionResult<M_SericeDocumentListBinding>> GetBindingByServiceId(int id)
        {
            try
            {
                var databind = await _context.M_ServiceDocumentListBinding.Where(binding => binding.ServiceDocumentListId == id).FirstOrDefaultAsync();
                if (databind == null)
                {
                    return NotFound();
                }

                return databind;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching the service document list binding by ID.");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/ServiceDocumentListBindings/5
        [HttpGet("GetServiceDocumentListBindingByCategoryId")]
        public async Task<ActionResult<object>> GetServiceDocumentListBindingByCategoryId(int id)
        {
            try
            {
                //var databind = await _context.M_ServiceDocumentListBinding.FindAsync(id);
                var databind = await _context.M_ServiceDocumentListBinding
                                    .Where(binding => binding.Id == id) // Filter by ID
                                    .Select(binding => new
                                    {
                                        binding.Id,
                                        CategoryID = _context.ServiceCategories
                                            .Where(cats => cats.CategoryId == binding.CategoryID)
                                            .Select(cats => cats.CategoryName)
                                            .FirstOrDefault(),
                                        ServiceDocumentListId = _context.ServicesLists
                                            .Where(docs => docs.ServiceListID == binding.ServiceDocumentListId)
                                            .Select(docs => docs.ServiceName)
                                            .FirstOrDefault()
                                    })
                                    .FirstOrDefaultAsync();
                if (databind == null)
                {
                    return NotFound();
                }

                return databind;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching the service document list binding by ID.");
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT: api/ServiceDocumentListBindings/5
        [HttpPost("UpdateServiceDocumentListBinding")]
        public async Task<IActionResult> UpdateServiceDocumentListBinding(int Id, M_SericeDocumentListBinding binding)
        {
            if (Id != binding.Id)
            {
                return BadRequest();
            }

            _context.Entry(binding).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceDocumentListBindingExists(Id))
                {
                    return NotFound();
                }
                throw; // rethrow the exception for logging
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the service document list binding.");
                return StatusCode(500, "Internal server error");
            }

            return NoContent();
        }

        // POST: api/ServiceDocumentListBindings
        [HttpPost("CreateServiceDocumentListBinding")]
        public async Task<ActionResult<M_SericeDocumentListBinding>> CreateServiceDocumentListBinding(M_SericeDocumentListBinding binding)
        {
            try
            {
                if (!ServiceDocumentListCatandServBindingExists(binding.CategoryID, binding.ServiceDocumentListId))
                {
                    _context.M_ServiceDocumentListBinding.Add(binding);
                    await _context.SaveChangesAsync();

                    return CreatedAtAction(nameof(GetServiceDocumentListBindings), new { id = binding.Id }, binding);
                }
                else
                {
                    _logger.LogError("This service and category is already bind.");
                    return StatusCode(409, "This service and category is already bind");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the service document list binding.");
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE: api/ServiceDocumentListBindings/5
        [HttpGet("DeleteServiceDocumentListBinding")]
        public async Task<IActionResult> DeleteServiceDocumentListBinding(int id)
        {
            try
            {
                var binding = await _context.M_ServiceDocumentListBinding.FindAsync(id);
                if (binding == null)
                {
                    return NotFound();
                }

                _context.M_ServiceDocumentListBinding.Remove(binding);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the service document list binding.");
                return StatusCode(500, "Internal server error");
            }
        }
        // DELETE: api/ServiceDocumentListBindings/5
        [HttpGet("DeleteServiceandCategoryBinding")]
        public async Task<IActionResult> DeleteServiceandCategoryBinding(int id)
        {
            try
            {
                var binding = await _context.M_ServiceDocumentListBinding.Where(x => x.ServiceDocumentListId == id).FirstOrDefaultAsync();
                if (binding == null)
                {
                    return NotFound();
                }

                _context.M_ServiceDocumentListBinding.Remove(binding);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the service document list binding.");
                return StatusCode(500, "Internal server error");
            }
        }
        private bool ServiceDocumentListBindingCategoryExists(int id)
        {
            return _context.M_ServiceDocumentListBinding.Any(e => e.CategoryID == id);
        }
        private bool ServiceDocumentListBindingServiceExists(int id)
        {
            return _context.M_ServiceDocumentListBinding.Any(e => e.ServiceDocumentListId == id);
        }
        private bool ServiceDocumentListCatandServBindingExists(int? CategoryID, int? ServiceId)
        {
            return _context.M_ServiceDocumentListBinding.Any(e => e.ServiceDocumentListId == ServiceId && e.CategoryID == CategoryID);
        }
        private bool ServiceDocumentListBindingExists(int id)
        {
            return _context.M_ServiceDocumentListBinding.Any(e => e.Id == id);
        }
    }
}
