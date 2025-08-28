using AFFZ_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AFFZ_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceDocumenListController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly ILogger<ServiceDocumenListController> _logger;

        public ServiceDocumenListController(MyDbContext context, ILogger<ServiceDocumenListController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/ServiceDocumenList/GetAllDocuments
        [HttpGet("GetAllDocuments")]
        public async Task<ActionResult<IEnumerable<ServiceDocumenList>>> GetAllDocuments()
        {
            try
            {
                var documents = await _context.ServiceDocumentList.ToListAsync();
                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching the documents.");
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        // GET: api/ServiceDocumenList/GetDocumentById/{id}
        [HttpGet("GetDocumentById/{id}")]
        public async Task<ActionResult<ServiceDocumenList>> GetDocumentById(int id)
        {
            var document = await _context.ServiceDocumentList.FindAsync(id);

            if (document == null)
            {
                return NotFound();
            }

            return Ok(document);
        }

        // POST: api/ServiceDocumenList/CreateDocument
        [HttpPost("CreateDocument")]
        public async Task<IActionResult> CreateDocument(ServiceDocumenList document)
        {
            try
            {
                if (document == null || !ModelState.IsValid)
                {
                    return BadRequest("Invalid request data.");
                }

                _context.ServiceDocumentList.Add(document);
                await _context.SaveChangesAsync();
                return Ok("Document Created Successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the document.");
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        // PUT: api/ServiceDocumenList/UpdateDocument/{id}
        [HttpPost("UpdateDocument/{id}")]
        public async Task<IActionResult> UpdateDocument(int id, ServiceDocumenList document)
        {
            if (id != document.ServiceDocumenListtId)
            {
                return BadRequest("Document ID mismatch.");
            }

            _context.Entry(document).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return Ok("Document Updated Successfully");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!DocumentExists(id))
                {
                    return NotFound("Document not found.");
                }
                else
                {
                    _logger.LogError(ex, "An error occurred while updating the document.");
                    return StatusCode(500, "An internal server error occurred.");
                }
            }
        }

        // DELETE: api/ServiceDocumenList/DeleteDocument/{id}
        [HttpPost("DeleteDocument/{id}")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            var document = await _context.ServiceDocumentList.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            _context.ServiceDocumentList.Remove(document);
            await _context.SaveChangesAsync();
            return Ok("Document Deleted Successfully");
        }

        private bool DocumentExists(int id)
        {
            return _context.ServiceDocumentList.Any(e => e.ServiceDocumenListtId == id);
        }
    }
}
