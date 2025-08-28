using AFFZ_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AFFZ_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestForDisCountToUserController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly ILogger<RequestForDisCountToUserController> _logger;
        public RequestForDisCountToUserController(MyDbContext context, ILogger<RequestForDisCountToUserController> logger)
        {
            _context = context;
            _logger = logger;
        }
        [HttpGet("GetDiscountedAmount")]
        public async Task<int> GetDiscountedAmount(int rfdfu)
        {
            try
            {
                int DiscountPrice = await _context.RequestForDisCountToUsers.Where(x => x.RFDFU == rfdfu).Select(x => x.FINALPRICE).FirstOrDefaultAsync();
                return DiscountPrice;
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpGet("GetServiceIdFromRfdfu")]
        public async Task<int> GetServiceIdFromRfdfu(int rfdfu)
        {
            try
            {
                int SID = await _context.RequestForDisCountToUsers.Where(x => x.RFDFU == rfdfu).Select(x => x.SID).FirstOrDefaultAsync();
                return SID;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }

}
