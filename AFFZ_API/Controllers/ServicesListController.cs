using AFFZ_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AFFZ_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesListController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly ILogger<ServicesListController> _logger;

        public ServicesListController(MyDbContext context, ILogger<ServicesListController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/ServicesList
        [HttpGet("GetServicesList")]
        public async Task<ActionResult<List<ServiceWithCategoryBinding>>> GetServicesList()
        {
            //return await _context.ServicesLists.ToListAsync();
            var servicesWithCategories = await (from service in _context.ServicesLists
                                                join binding in _context.M_ServiceDocumentListBinding
                                                    on service.ServiceListID equals binding.ServiceDocumentListId into bindingJoin
                                                from binding in bindingJoin.DefaultIfEmpty() // Left join to ensure we include all services
                                                join category in _context.ServiceCategories
                                                    on binding.CategoryID equals category.CategoryId into categoryJoin
                                                from category in categoryJoin.DefaultIfEmpty() // Left join to ensure we include all services
                                                select new ServiceWithCategoryBinding
                                                {
                                                    ServiceListID = service.ServiceListID,
                                                    ServiceName = service.ServiceName,
                                                    ServiceImage = service.ServiceImage,
                                                    CategoryName = category.CategoryName ?? "No Category associated with service", // Custom message if no category
                                                    CategoryID = (category.CategoryId == null ? 0 : category.CategoryId),
                                                    ServiceCategoryBindingId = (binding.Id == null ? 0 : binding.Id),
                                                    BindingStatus = binding == null ? "No binding found" : string.Empty, // Custom message if no binding
                                                    CategoryStatus = category == null ? "No Category associated with service" : string.Empty // Custom message if no category
                                                }).ToListAsync();
            return servicesWithCategories;
        }

        // GET: api/ServicesList/5
        [HttpGet("GetServicesListById")]
        public async Task<ActionResult<ServicesList>> GetServicesListById(int id)
        {
            var servicesList = await _context.ServicesLists.FindAsync(id);

            if (servicesList == null)
            {
                return NotFound();
            }

            return servicesList;
        }

        // PUT: api/ServicesList/5
        [HttpPost("UpdateServicesList")]
        public async Task<IActionResult> UpdateServicesList(int id, ServicesList servicesList)
        {
            if (id != servicesList.ServiceListID)
            {
                return BadRequest();
            }

            _context.Entry(servicesList).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServicesListExists(id))
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

        // POST: api/ServicesList
        [HttpPost("PostServicesList")]
        public async Task<ActionResult<ServicesList>> PostServicesList(ServicesList servicesList)
        {
            _context.ServicesLists.Add(servicesList);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetServicesList", new { id = servicesList.ServiceListID }, servicesList);
        }

        // DELETE: api/ServicesList/5
        [HttpDelete("DeleteServicesList")]
        public async Task<IActionResult> DeleteServicesList(int id)
        {
            var servicesList = await _context.ServicesLists.FindAsync(id);
            if (servicesList == null)
            {
                return NotFound();
            }

            _context.ServicesLists.Remove(servicesList);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpGet("GetServiceNameById")]
        public async Task<string> GetServiceNameById(int id)
        {
            var servicesList = await _context.ServicesLists.FindAsync(id);
            if (servicesList == null)
            {
                return "NotFound";
            }
            return servicesList.ServiceName;

        }
        [HttpGet("GetServiceImageId")]
        public async Task<string> GetServiceImageId(int id)
        {
            var servicesList = await _context.ServicesLists.FindAsync(id);
            if (servicesList == null)
            {
                return "NotFound";
            }
            return servicesList.ServiceImage;

        }
        private bool ServicesListExists(int id)
        {
            return _context.ServicesLists.Any(e => e.ServiceListID == id);
        }
    }
    public class ServiceWithCategoryBinding
    {
        public int ServiceListID { get; set; }
        public string? ServiceName { get; set; }
        public string? ServiceImage { get; set; }
        public string? CategoryName { get; set; }
        public int CategoryID { get; set; }
        public int ServiceCategoryBindingId { get; set; }
        public string? BindingStatus { get; set; } // To store the "No binding found" message
        public string? CategoryStatus { get; set; } // To store the "No category associated" message
    }
}
