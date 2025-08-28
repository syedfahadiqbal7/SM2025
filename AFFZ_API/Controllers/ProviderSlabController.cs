using AFFZ_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AFFZ_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProviderSlabController : ControllerBase
    {
        private readonly MyDbContext _context;

        public ProviderSlabController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/ProviderSlab
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProviderSlab>>> GetProviderSlabs()
        {
            return await _context.ProviderSlab
                .Include(ps => ps.ProviderUser)
                .ToListAsync();
        }

        // GET: api/ProviderSlab/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProviderSlab>> GetProviderSlab(int id)
        {
            var providerSlab = await _context.ProviderSlab
                .Include(ps => ps.ProviderUser)
                .FirstOrDefaultAsync(ps => ps.ProviderSlabID == id);

            if (providerSlab == null)
            {
                return NotFound();
            }

            return providerSlab;
        }
        [HttpGet("GetSlabByProviderId")]
        public async Task<ActionResult<ProviderSlab>> GetSlabByProviderId(int id)
        {
            var providerSlab = await _context.ProviderSlab
                .Include(ps => ps.ProviderUser)
                .Where(x => x.ProviderID == id)
                .FirstOrDefaultAsync(ps => ps.ProviderSlabID == id);

            if (providerSlab == null)
            {
                return NotFound();
            }

            return providerSlab;
        }
        // POST: api/ProviderSlab
        [HttpPost]
        public async Task<ActionResult<ProviderSlab>> CreateProviderSlab(ProviderSlab providerSlab)
        {
            //Check Provider Details:
            try
            {
                var existingProviderProfile = await _context.ProviderUsers.FirstOrDefaultAsync(x => x.ProviderId == providerSlab.ProviderID);
                providerSlab.ProviderUser = existingProviderProfile;
                providerSlab.CreatedAt = DateTime.Now;
                providerSlab.UpdatedAt = DateTime.Now;
                _context.ProviderSlab.Add(providerSlab);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetProviderSlab), new { id = providerSlab.ProviderSlabID }, providerSlab);
            }
            catch (Exception)
            {

                throw;
            }
        }

        // PUT: api/ProviderSlab/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProviderSlab(int id, ProviderSlab providerSlab)
        {
            if (id != providerSlab.ProviderSlabID)
            {
                return BadRequest();
            }

            _context.Entry(providerSlab).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProviderSlabExists(id))
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

        // DELETE: api/ProviderSlab/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProviderSlab(int id)
        {
            var providerSlab = await _context.ProviderSlab.FindAsync(id);
            if (providerSlab == null)
            {
                return NotFound();
            }

            _context.ProviderSlab.Remove(providerSlab);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProviderSlabExists(int id)
        {
            return _context.ProviderSlab.Any(e => e.ProviderSlabID == id);
        }
    }
}
