using AFFZ_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AFFZ_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceDocumentMappingController : ControllerBase
    {
        private readonly MyDbContext _context;

        public ServiceDocumentMappingController(MyDbContext context)
        {
            _context = context;
        }

        [HttpPost("CreateMapping")]
        public async Task<IActionResult> CreateMapping(List<ServiceDocumentMapping> mappings)
        {
            try
            {
                if (mappings == null || mappings.Count == 0)
                {
                    return BadRequest("Invalid data.");
                }

                // Extract unique ServiceIDs from the incoming mappings
                var serviceIds = mappings.Select(m => m.ServiceID).Distinct().ToList();

                // Log received mappings
                foreach (var mapping in mappings)
                {
                    Console.WriteLine($"Received Mapping: ServiceID: {mapping.ServiceID}, ServiceDocumentListId: {mapping.ServiceDocumentListId}");
                }

                // Delete existing mappings for those service IDs
                var existingMappings = _context.ServiceDocumentMapping
                    .Where(m => serviceIds.Contains(m.ServiceID));
                _context.ServiceDocumentMapping.RemoveRange(existingMappings);

                // Add new mappings
                await _context.ServiceDocumentMapping.AddRangeAsync(mappings);

                await _context.SaveChangesAsync();

                return Ok("Mappings created successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("GetMappingsByService")]
        public async Task<ActionResult<IEnumerable<ServiceDocumentMapping>>> GetMappingsByService(int Id)
        {
            var mappings = await _context.ServiceDocumentMapping
                                         .Where(m => m.ServiceID == Id)
                                         .ToListAsync();

            if (mappings == null || mappings.Count == 0)
            {
                return NotFound();
            }

            return Ok(mappings);
        }

        [HttpGet("DeleteMappingsByServiceId")]
        public IActionResult DeleteMappingsByServiceId(int Id)
        {
            List<ServiceDocumentMapping> service = _context.ServiceDocumentMapping.Where(x => x.ServiceID == Id).ToList();
            _context.ServiceDocumentMapping.RemoveRange(service);
            _context.SaveChanges();

            return Ok("Mappings deleted successfully.");
        }

    }
}
