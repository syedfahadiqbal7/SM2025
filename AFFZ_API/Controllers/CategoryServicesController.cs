using AFFZ_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AFFZ_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryServicesController : ControllerBase
    {
        private readonly MyDbContext _context;
        public CategoryServicesController(MyDbContext context)
        {
            _context = context;
        }

        [HttpGet("AllCategories")]
        public async Task<ActionResult<IEnumerable<Service>>> GetSubCategoriesList()
        {
            /*
			 * GroupBy: The query uses GroupBy on CategoryName to group the records by the distinct CategoryName.
			 * Select: The Select method is then used to project the grouped data into a new ServiceCategoryDto. The CategoryId is taken from the first record in each group, and the CategoryName is the grouping key (g.Key).
			 
			 */

            try
            {
                var categories = await _context.Services
                                .GroupBy(sc => new
                                {
                                    sc.SID,
                                    sc.ServicePrice,
                                    MerchantLocation = sc.Merchant.MerchantLocation,
                                    sc.CategoryID
                                })
                                .Select(g => new
                                {
                                    g.Key.SID,
                                    g.Key.ServicePrice,
                                    g.Key.MerchantLocation,
                                    g.Key.CategoryID
                                })
                                .Join(
                                    _context.ServicesLists, // Joining with ServicesList
                                    s => s.SID,             // Match SID from Services
                                    sl => sl.ServiceListID, // with ServiceListID from ServicesList
                                    (s, sl) => new SubCatPage
                                    {
                                        CatId = s.CategoryID,
                                        ServiceName = sl.ServiceName,  // Use ServiceName from ServicesList
                                        ServicePrice = s.ServicePrice,
                                        Location = s.MerchantLocation,
                                        ServiceImage = sl.ServiceImage // Include ServiceImage
                                    })
                                .ToListAsync();

                return Ok(categories);

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        [HttpGet("getServiceByName")]
        public async Task<ActionResult<Service>> GetServiceByName(string id)
        {

            int sid = Convert.ToInt32(id);
            var servicedata = await _context.Services.Where(x => x.ServiceId == sid).FirstOrDefaultAsync();

            return Ok(servicedata);
        }
        [HttpGet("getServiceNameFromServiceList")]
        public async Task<ActionResult<string>> getServiceNameFromServiceList(int id)
        {

            int sid = Convert.ToInt32(id);
            var servicedata = await _context.ServicesLists.Where(x => x.ServiceListID == sid).FirstOrDefaultAsync();

            return Ok(servicedata.ServiceName);
        }
    }
    //Defines the data structure returned by the search endpoint, including service name, company name, merchant location, price, and average rating.
    public class ServiceCategoryDto
    {
        public string CategoryName { get; set; }
    }
}
