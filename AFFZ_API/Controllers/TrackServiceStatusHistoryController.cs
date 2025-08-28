using AFFZ_API.Interfaces;
using AFFZ_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AFFZ_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrackServiceStatusHistoryController : ControllerBase
    {

        private readonly MyDbContext _context;
        private readonly ILogger<TrackServiceStatusHistoryController> _logger;
        private readonly IEmailService _emailService;

        public TrackServiceStatusHistoryController(MyDbContext context, ILogger<TrackServiceStatusHistoryController> logger, IEmailService emailService)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
        }

        // GET: api/RequestStatus
        [HttpGet("GetAllStatuses")]
        public async Task<ActionResult<IEnumerable<TrackServiceStatusHistory>>> GetAllStatuses(string UserType, int userId)
        {
            return await _context.TrackServiceStatusHistory.Where(x => x.ChangedByID == userId && x.ChangedByUserType == UserType).ToListAsync();
        }

        // GET: api/RequestStatus/5
        [HttpGet("GetStatusById")]
        public async Task<ActionResult<TrackServiceStatusHistory>> GetStatusById(int id)
        {
            var status = await _context.TrackServiceStatusHistory.FindAsync(id);

            if (status == null)
            {
                return NotFound();
            }

            return status;
        }

        // POST: api/RequestStatus
        [HttpPost("CreateStatus")]
        public async Task<ActionResult<TrackServiceStatusHistory>> CreateStatus(TrackServiceStatusHistory status)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                _context.TrackServiceStatusHistory.Add(status);
                await _context.SaveChangesAsync();
                await CreateorUpdateCurrentStatus(status);
                return CreatedAtAction(nameof(GetStatusById), new { id = status.HistoryID }, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                return StatusCode(500, ex.Message);
            }
        }

        // PUT: api/RequestStatus/5
        [HttpPost("UpdateStatus")]
        public async Task<IActionResult> UpdateStatus(int id, TrackServiceStatusHistory status)
        {
            if (id != status.HistoryID)
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
            var status = await _context.TrackServiceStatusHistory.FindAsync(id);
            if (status == null)
            {
                return NotFound();
            }

            _context.TrackServiceStatusHistory.Remove(status);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        private async Task CreateorUpdateCurrentStatus(TrackServiceStatusHistory status)
        {
            try
            {
                //get uid AND MID FROM RFDFU iD
                var requestDiscount = await _context.RequestForDisCountToUsers.FindAsync(status.RFDFU);
                if (requestDiscount != null)
                {
                    var statusUpdate = new CurrentServiceStatus
                    {
                        UId = requestDiscount.UID,
                        MId = requestDiscount.MID,
                        RFDFU = status.RFDFU,
                        CurrentStatus = status.StatusID.ToString()
                    };

                    // Send the request to the AFFZ_API
                    try
                    {
                        if (statusUpdate != null || !string.IsNullOrWhiteSpace(statusUpdate.CurrentStatus))
                        {
                            // Check if the record exists for the given UId, MId, and RFDFU; if it does, update the status.
                            var existingStatus = await _context.CurrentServiceStatus
                                .FirstOrDefaultAsync(cs => cs.UId == statusUpdate.UId && cs.MId == statusUpdate.MId && cs.RFDFU == statusUpdate.RFDFU);

                            if (existingStatus != null)
                            {
                                existingStatus.CurrentStatus = statusUpdate.CurrentStatus;
                            }
                            else
                            {
                                // If no record exists, create a new one
                                _context.CurrentServiceStatus.Add(statusUpdate);
                            }

                            await _context.SaveChangesAsync();
                        }
                    }
                    catch (Exception ex)
                    {

                        throw;
                    }
                }

            }
            catch (Exception)
            {

                throw;
            }


        }
        private bool StatusExists(int id)
        {
            return _context.TrackServiceStatusHistory.Any(e => e.HistoryID == id);
        }
    }
}
