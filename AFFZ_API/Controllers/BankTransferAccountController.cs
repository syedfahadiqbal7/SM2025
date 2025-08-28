using AFFZ_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AFFZ_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BankTransferAccountController : ControllerBase
    {
        private readonly MyDbContext _context;
        private ILogger<BankTransferAccountController> _logger;

        public BankTransferAccountController(MyDbContext context, ILogger<BankTransferAccountController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BankTransferAccount>>> GetAccounts()
        {
            try
            {
                var AccList = await _context.BankTransferAccount.ToListAsync();
                if (AccList.Count == 0)
                    return new List<BankTransferAccount>();
                return AccList;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        [HttpGet("AllMerchants")]
        public async Task<ActionResult<IEnumerable<Merchant>>> AllMerchantsAccounts()
        {
            try
            {
                var merchants = await _context.Merchants.ToListAsync();
                if (merchants.Count == 0)
                    return new List<Merchant>();
                return merchants;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        [HttpGet("GetMerchantById")]
        public async Task<ActionResult<Merchant>> GetMerchantById(int id)
        {
            try
            {
                var merchants = await _context.Merchants.FindAsync(id);
                if (merchants == null)
                    return NotFound();
                return merchants;
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<BankTransferAccount>> GetAccount(int id)
        {
            try
            {
                var account = await _context.BankTransferAccount.FindAsync(id);
                if (account == null)
                    return NotFound();
                return account;
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpPost]
        public async Task<ActionResult<BankTransferAccount>> CreateAccount(BankTransferAccount account)
        {
            try
            {
                _context.BankTransferAccount.Add(account);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, account);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> UpdateAccount(int id, BankTransferAccount account)
        {
            try
            {
                if (id != account.Id)
                    return BadRequest();

                _context.Entry(account).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        [HttpGet("DeleteAccount")]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            try
            {
                var account = await _context.BankTransferAccount.FindAsync(id);
                if (account == null)
                    return NotFound();

                _context.BankTransferAccount.Remove(account);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        [HttpGet("GetMerchants")]
        public async Task<ActionResult<IEnumerable<Merchant>>> GetMerchants()
        {
            return await _context.Merchants.ToListAsync();
        }
    }
}
