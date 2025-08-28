using AFFZ_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AFFZ_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MServiceDefaultDocumentController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly ILogger<MServiceDefaultDocumentController> _logger;

        public MServiceDefaultDocumentController(MyDbContext context, ILogger<MServiceDefaultDocumentController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("GetServiceDocuments")]
        public async Task<ActionResult<IEnumerable<ServiceDocument>>> GetServiceDocuments(int serviceID)
        {
            try
            {
                var documents = await _context.m_SericeDefaultDocumentList.Where(x => x.ServiceID == serviceID).ToListAsync();
                if (documents == null)
                {
                    _logger.LogWarning("No service documents found.");
                    return NotFound();
                }
                _logger.LogInformation("Fetched service documents successfully.");
                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching service documents: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
