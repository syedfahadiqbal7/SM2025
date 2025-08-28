using AFFZ_API.Interfaces;
using AFFZ_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AFFZ_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestStatusController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly ILogger<RequestStatusController> _logger;
        private readonly IEmailService _emailService;

        public RequestStatusController(MyDbContext context, ILogger<RequestStatusController> logger, IEmailService emailService)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
        }
        [HttpGet("RequestStatusList")]
        public async Task<ActionResult<IEnumerable<RequestStatuses>>> RequestStatusList()
        {
            return await _context.RequestStatuses.ToListAsync();
        }
        // GET: api/RequestStatus
        [HttpGet("GetAllStatuses")]
        public async Task<ActionResult<IEnumerable<RequestStatuses>>> GetAllStatuses(string UserType)
        {
            return await _context.RequestStatuses.Where(x => x.Usertype == UserType).ToListAsync();
        }
        [HttpGet("GetAllStatusesToUpdateMerchant")]
        public async Task<ActionResult<IEnumerable<RequestStatuses>>> GetAllStatusesToUpdateMerchant(string UserType, int merchantId, int CustomerID, int ServiceId)
        {
            return await _context.RequestStatuses.Where(x => x.Usertype == UserType && x.StatusID > 15).ToListAsync();
        }

        // GET: api/RequestStatus/5
        [HttpGet("GetStatusById")]
        public async Task<ActionResult<RequestStatuses>> GetStatusById(int id)
        {
            var status = await _context.RequestStatuses.FindAsync(id);

            if (status == null)
            {
                return NotFound();
            }

            return status;
        }

        // POST: api/RequestStatus
        [HttpPost("CreateStatus")]
        public async Task<ActionResult<RequestStatuses>> CreateStatus(RequestStatuses status)
        {
            _context.RequestStatuses.Add(status);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStatusById), new { id = status.StatusID }, status);
        }

        // PUT: api/RequestStatus/5
        [HttpPost("UpdateStatus")]
        public async Task<IActionResult> UpdateStatus(int id, RequestStatuses status)
        {
            if (id != status.StatusID)
            {
                return BadRequest();
            }

            _context.Entry(status).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StatusExists(id))
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

        // DELETE: api/RequestStatus/5
        [HttpGet("DeleteStatus")]
        public async Task<IActionResult> DeleteStatus(int id)
        {
            var status = await _context.RequestStatuses.FindAsync(id);
            if (status == null)
            {
                return NotFound();
            }

            _context.RequestStatuses.Remove(status);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpGet("GetRequestStatusIdByName")]
        public async Task<int> GetRequestStatusIdByName(string StatusName)
        {
            var status = await _context.RequestStatuses.Where(x => x.StatusName == StatusName).Select(x => x.StatusID).FirstOrDefaultAsync();
            if (status == null)
            {
                return -1;
            }
            return status;

        }
        private bool StatusExists(int id)
        {
            return _context.RequestStatuses.Any(e => e.StatusID == id);
        }
    }
}
