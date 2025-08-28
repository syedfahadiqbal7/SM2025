using AFFZ_API.Interfaces;
using AFFZ_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AFFZ_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryWithMerchant : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly ILogger<CategoryWithMerchant> _logger;
        private readonly IEmailService _emailService;
        public CategoryWithMerchant(MyDbContext context, ILogger<CategoryWithMerchant> logger, IEmailService emailService)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
        }
        [HttpGet("AllRequestMerchant")]
        public async Task<ActionResult<IEnumerable<object>>> AllRequestMerchant(string Mid)
        {
            int id = Convert.ToInt32(Mid);
            var query = from rm in _context.RequestForDisCountToMerchants
                        join ser in _context.Services
                        on new { MID = (int)rm.MID, SID = (int)rm.SID } equals new { MID = (int)ser.MerchantID, SID = (int)ser.ServiceId }
                        join sl in _context.ServicesLists
                        on ser.SID equals sl.ServiceListID
                        where rm.MID == id && rm.IsResponseSent == 0
                        select new RequestForDiscountViewModel
                        {
                            RFDTM = rm.RFDTM,
                            SID = (int)ser.ServiceId,
                            UID = (int)rm.UID,
                            MID = rm.MID,
                            ServiceName = sl.ServiceName, // Get ServiceName from ServicesList
                            ServicePrice = ser.ServicePrice,
                            RequestDatetime = rm.RequestDateTime
                        };

            var result = await query.ToListAsync();

            return Ok(result);
        }

        private bool IsRequestedService(int Mid, int sid)
        {
            return _context.RequestForDisCountToMerchants.Where(x => x.MID == Mid && x.SID == sid && x.IsResponseSent == 0).Any();
        }
        private bool IsRequestedServiceCheckAfterMerchantResponse(int Mid, int sid, int uid)
        {
            return _context.RequestForDisCountToUsers
    .Where(x => x.MID == Mid
                && x.SID == sid
                && x.IsPaymentDone > -1
                && x.UID == uid
                && x.ResponseDateTime.Value.Year == DateTime.Now.Year
                && x.ResponseDateTime.Value.Month == DateTime.Now.Month)
    .Any();
        }
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] SearchRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var query = _context.Services
            .Join(_context.Merchants,
                  service => service.MerchantID,
                  merchant => merchant.MerchantId,
                  (service, merchant) => new { service, merchant })
            .GroupJoin(_context.MerchantRatings,
                       combined => combined.merchant.MerchantId,
                       rating => rating.RatedMerchantId,
                       (combined, ratings) => new { combined.service, combined.merchant, ratings })
            .SelectMany(x => x.ratings.DefaultIfEmpty(),
                        (combined, rating) => new
                        {
                            combined.service,
                            combined.merchant,
                            Rating = rating != null ? rating.RatingValue : (int?)null
                        })
            .Join(_context.ServicesLists, // Join with ServicesList
                  x => x.service.SID,
                  sl => sl.ServiceListID,
                  (x, sl) => new
                  {
                      x.service,
                      x.merchant,
                      x.Rating,
                      ServiceName = sl.ServiceName // Get ServiceName from ServicesList
                  })
            .GroupBy(x => new
            {
                x.ServiceName,
                x.merchant.CompanyName,
                x.merchant.MerchantLocation,
                x.service.ServicePrice
            })
            .Select(g => new ServiceDto
            {
                ServiceName = g.Key.ServiceName,
                CompanyName = g.Key.CompanyName,
                MerchantLocation = g.Key.MerchantLocation,
                Price = (decimal)g.Key.ServicePrice,
                AverageRating = g.Average(x => x.Rating ?? 0)
            })
            .AsQueryable();
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(s => s.ServiceName.Contains(request.Keyword) ||
                                         s.CompanyName.Contains(request.Keyword) ||
                                         s.MerchantLocation.Contains(request.Keyword));
            }

            if (!string.IsNullOrEmpty(request.CategoryName))
            {
                query = query.Where(s => s.ServiceName == request.CategoryName);
            }

            if (!string.IsNullOrEmpty(request.SubCategoryName))
            {
                query = query.Where(s => s.ServiceName == request.SubCategoryName);
            }

            if (!string.IsNullOrEmpty(request.Location))
            {
                query = query.Where(s => s.MerchantLocation.Contains(request.Location));
            }

            if (request.MinPrice.HasValue)
            {
                query = query.Where(s => s.Price >= request.MinPrice.Value);
            }

            if (request.MaxPrice.HasValue)
            {
                query = query.Where(s => s.Price <= request.MaxPrice.Value);
            }

            if (request.MinRating.HasValue)
            {
                query = query.Where(s => s.AverageRating >= request.MinRating.Value);
            }

            if (!string.IsNullOrEmpty(request.MerchantName))
            {
                query = query.Where(s => s.CompanyName.Contains(request.MerchantName));
            }

            if (request.SortBy == "price_high_to_low")
            {
                query = query.OrderByDescending(s => s.Price);
            }
            else if (request.SortBy == "price_low_to_high")
            {
                query = query.OrderBy(s => s.Price);
            }

            var result = await query.ToListAsync();

            return Ok(new { count = result.Count, services = result });
        }

        [HttpPost("reset-filters")]
        public IActionResult ResetFilters()
        {
            // Reset logic can be applied on client side, server can return default data set
            return Ok(new { message = "Filters reset successfully." });
        }

        [HttpGet("AllCategories")]
        public async Task<ActionResult<IEnumerable<Service>>> GetSubCategoriesList()
        {
            /*
			 * GroupBy: The query uses GroupBy on CategoryName to group the records by the distinct CategoryName.
			 * Select: The Select method is then used to project the grouped data into a new ServiceCategoryDto. The CategoryId is taken from the first record in each group, and the CategoryName is the grouping key (g.Key).
			 
			 */
            var categories = await _context.Services
            .Join(_context.ServicesLists,
                  service => service.SID,
                  serviceList => serviceList.ServiceListID,
                  (service, serviceList) => new
                  {
                      service.SID,
                      service.ServicePrice,
                      serviceList.ServiceName
                  })
            .GroupBy(sc => new
            {
                sc.SID,
                sc.ServicePrice,
                sc.ServiceName
            })
            .Select(g => new SubCatPage
            {
                ServiceName = g.Key.ServiceName,
                ServicePrice = g.Key.ServicePrice
            })
            .ToListAsync();


            return Ok(categories);
        }

        [HttpGet("AllSubCategories")]
        public async Task<IActionResult> GetServiceCategoriesList()
        {
            var categories = _context.ServiceCategories.ToListAsync();

            return Ok(categories);
        }
        [HttpGet("GetServiceListByMerchant")]
        public async Task<IActionResult> GetServiceListByMerchant(string CatName, int uid)
        {
            try
            {
                var query = from merchant in _context.Merchants
                            join service in _context.Services on merchant.MerchantId equals service.MerchantID
                            join serviceList in _context.ServicesLists on service.SID equals serviceList.ServiceListID
                            where string.IsNullOrEmpty(CatName) || serviceList.ServiceName == CatName
                            select new ServiceMerchantDTO
                            {
                                MID = merchant.MerchantId,
                                SID = service.ServiceId,
                                MERCHANTNAME = merchant.CompanyName,
                                SERVICENAME = serviceList.ServiceName,
                                PRICE = service.ServicePrice ?? 0,
                                MERCHANTLOCATION = merchant.MerchantLocation,
                                ServiceImage = serviceList.ServiceImage,
                                RequiresEligibility = service.RequiresEligibility,
                                SERVICERATING = 0,
                                IsRequestedEligibility = false,
                                IsWaitingEligibilityResponseFromUser = false,
                                IsRequestedAlready = false
                            };

                var result = await query.ToListAsync();

                var serviceIds = result.Select(x => x.SID).ToList();
                var merchantIds = result.Select(x => x.MID).ToList();

                var twentyFourHoursAgo = DateTime.UtcNow.ToLocalTime().AddHours(-24);

                // Fetch eligibility details in one go
                var eligibilityDetails = await _context.EligibilityRequests
                    .Where(x => serviceIds.Contains(x.ServiceID)
                                && merchantIds.Contains(x.MerchantID)
                                && x.CustomerID == uid
                                && x.CreatedDate >= twentyFourHoursAgo)
                    .ToListAsync();


                // Fetch discount details in one go
                var discountToMerchantList = await _context.RequestForDisCountToMerchants
                    .Where(x => serviceIds.Contains(x.SID) && merchantIds.Contains(x.MID) && x.UID == uid)
                    .ToListAsync();

                var discountToUserList = await _context.RequestForDisCountToUsers
                    .Where(x => serviceIds.Contains(x.SID) && merchantIds.Contains(x.MID) && x.UID == uid &&
                                x.ResponseDateTime.HasValue &&
                                x.ResponseDateTime.Value.Month == DateTime.Now.Month &&
                                x.ResponseDateTime.Value.Year == DateTime.Now.Year &&
                                x.IsPaymentDone > -1 && x.IsMerchantSelected > -1)
                    .ToListAsync();

                var currentServiceStatusList = await _context.CurrentServiceStatus
                    .Where(x => discountToUserList.Select(d => d.RFDFU).Contains(x.RFDFU))
                    .ToListAsync();

                var statusNames = await _context.RequestStatuses
                    .ToDictionaryAsync(x => x.StatusID, x => x.StatusName);

                // Fetch ratings in one go
                var serviceRatings = await _context.Review
                    .Where(x => serviceIds.Contains(x.ServiceId) && merchantIds.Contains(x.merchantId) && x.CustomerId == uid)
                    .ToDictionaryAsync(x => new ServiceMerchantKey { SID = x.ServiceId, MID = x.merchantId }, x => x.Rating);


                foreach (var item in result)
                {
                    var eligibility = eligibilityDetails.FirstOrDefault(x => x.ServiceID == item.SID && x.MerchantID == item.MID);
                    item.IsRequestedEligibility = eligibility != null;
                    item.IsEligibilityApproved = eligibility != null && !eligibility.IsRequestRejected && eligibility.IsRespondedByMerchant && statusNames.TryGetValue(eligibility.StatusID, out string _statusName)
    && _statusName == "Approved"; ;
                    item.IsWaitingEligibilityResponseFromUser = eligibility != null && !eligibility.IsRespondedByMerchant;

                    var discountToMerchant = discountToMerchantList.FirstOrDefault(x => x.SID == item.SID && x.MID == item.MID);
                    if (discountToMerchant == null)
                    {
                        item.IsRequestedAlready = false;
                    }
                    else if (discountToMerchant.IsResponseSent == 0)
                    {
                        item.IsRequestedAlready = true;
                    }
                    else
                    {
                        var discountToUser = discountToUserList.FirstOrDefault(x => x.SID == item.SID && x.MID == item.MID);
                        if (discountToUser == null)
                        {
                            item.IsRequestedAlready = false;
                        }
                        else
                        {
                            var currentStatus = currentServiceStatusList.FirstOrDefault(x => x.RFDFU == discountToUser.RFDFU);
                            if (currentStatus != null && statusNames.TryGetValue(Convert.ToInt32(currentStatus.CurrentStatus), out string statusName))
                            {
                                item.IsRequestedAlready = statusName == "InProgress" || statusName == "Pending";
                            }
                            else
                            {
                                item.IsRequestedAlready = false;
                            }
                        }
                    }

                    item.SERVICERATING = serviceRatings.GetValueOrDefault(new ServiceMerchantKey { SID = item.SID, MID = item.MID }, 0);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetServiceListByMerchant for UID: {UID}", uid);
                return StatusCode(500, new { message = "An error occurred while fetching services.", error = ex.Message });
            }
        }

        [HttpPost("sendRequestForDiscount")]
        public async Task<IActionResult> sendRequestForDiscount(DiscountRequestClass data)
        {
            if (data == null || !ModelState.IsValid)
            {
                return BadRequest("Invalid request data.");
            }

            try
            {
                int mid = Convert.ToInt32(data.MerchantId);
                var requestForDisCountToMerchant = new RequestForDisCountToMerchant
                {
                    SID = data.ServiceId,
                    MID = data.MerchantId,
                    UID = data.UserId,
                    IsResponseSent = 0,
                    RequestDateTime = DateTime.Now,
                };
                _context.Add(requestForDisCountToMerchant);
                await _context.SaveChangesAsync();
                /*  await _context.Database.ExecuteSqlRawAsync("sp_InsertRequestForDisCountToMerchant @p0, @p1, @p2, @p3, @p4",
             parameters: new object[] { data.ServiceId, data.MerchantId, data.UserId, 0, DateTime.Now });*/

                //string MerchantEmail = _context.ProviderUsers.Where(x => x.ProviderId == mid).Select(x => x.Email).FirstOrDefault();
                //string MerchantName = _context.ProviderUsers.Where(x => x.ProviderId == mid).Select(x => x.ProviderName).FirstOrDefault();
                //string ServiceName = _context.Services.Where(x => x.ServiceId == data.ServiceId).Join(_context.ServicesLists, service => service.SID, serviceList => serviceList.ServiceListID, (service, serviceList) => serviceList.ServiceName).FirstOrDefault();
                //bool res = await _emailService.SendEmail(MerchantEmail, "Service Inquiry-SmartCenter", "Dear " + MerchantName + ", You Have recieved a request from a user. Inquiry type:" + ServiceName, MerchantName);
                return Ok("Request Sent To Merchant");
            }
            catch (Exception ex)
            {
                // Log the exception details
                // Use your preferred logging framework here. For example:
                _logger.LogError(ex, "An error occurred while processing the discount request.");

                return StatusCode(500, "An internal server error occurred. Please try again later.");
            }
        }
        //AllResponsesFromMerchant
        [HttpGet("AllResponsesFromMerchant")]
        public async Task<IActionResult> AllResponsesFromMerchant(string Uid)
        {
            try
            {
                var query = from service in _context.Services
                            join serviceList in _context.ServicesLists
                                on service.SID equals serviceList.ServiceListID // Join with ServicesList to get ServiceName
                            join request in _context.RequestForDisCountToUsers
                                on new { TSID = service.ServiceId, TMID = service.MerchantID ?? 0 }
                                equals new { TSID = request.SID, TMID = request.MID }
                            join status in _context.CurrentServiceStatus
                                on new { UID = request.UID, MID = service.MerchantID ?? 0, RFDFU = request.RFDFU }
                                equals new { UID = status.UId, MID = status.MId, RFDFU = status.RFDFU }
                                into statusJoin
                            from status in statusJoin.DefaultIfEmpty() // Left join to handle missing status records
                            join requestStatus in _context.RequestStatuses
                                on Convert.ToInt32(status.CurrentStatus) equals requestStatus.StatusID
                                into requestStatusJoin
                            from requestStatus in requestStatusJoin.DefaultIfEmpty() // Left join to handle missing status records
                            where service.MerchantID.HasValue
                            select new RequestForDisCountToUserViewModel
                            {
                                SID = request.SID,
                                ServicePrice = service.ServicePrice ?? 0,
                                ServiceName = serviceList.ServiceName, // Fetch ServiceName from ServicesList
                                MerchantID = service.MerchantID ?? 0,
                                FINALPRICE = request.FINALPRICE,
                                UID = request.UID,
                                RFDFU = request.RFDFU,
                                IsMerchantSelected = request.IsMerchantSelected,
                                ResponseDateTime = request.ResponseDateTime,
                                IsPaymentDone = request.IsPaymentDone,
                                CurrentStatus = requestStatus != null ? requestStatus.StatusDescription : "Unknown",  // Use StatusDescription or default to "Unknown"
                                IsRequestCompleted = requestStatus != null && (requestStatus.StatusName == "Completed" || requestStatus.StatusName == "Failed") // Check for both statuses // Check if StatusName is "Completed"
                            };



                var sql = query.ToQueryString();
                _logger.LogInformation(sql);
                if (!string.IsNullOrEmpty(Uid))
                {
                    if (Uid != "Merchant")
                    {
                        int userid = Convert.ToInt32(Uid);
                        query = query.Where(x => x.UID == userid && x.IsMerchantSelected > -1);
                    }
                }

                var result = await query.ToListAsync();
                foreach (var item in result)
                {

                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }
        [HttpPost("SaveMerchantResponseForDiscount")]
        public async Task<IActionResult> SaveMerchantResponseForDiscount(SubmitResponseByMerchant data)
        {
            if (data == null)
            {
                return BadRequest("Invalid request data.");
            }

            try
            {
                var requestForDisCountToMerchant = new RequestForDisCountToMerchant
                {
                    SID = Convert.ToInt32(data.SID),
                    MID = Convert.ToInt32(data.MID),
                    UID = Convert.ToInt32(data.UID),
                    IsResponseSent = 1,
                    RFDTM = Convert.ToInt32(data.RFDTM),
                    RequestDateTime = DateTime.Now,
                };

                _context.Update(requestForDisCountToMerchant);
                await _context.SaveChangesAsync();

                var requestForDisCountToUser = new RequestForDisCountToUser
                {
                    SID = Convert.ToInt32(data.SID),
                    MID = Convert.ToInt32(data.MID),
                    UID = Convert.ToInt32(data.UID),
                    FINALPRICE = Convert.ToInt32(data.DiscountPrice),
                    ResponseDateTime = DateTime.Now,
                    IsMerchantSelected = 0,
                    IsPaymentDone = 0
                };

                _context.Add(requestForDisCountToUser);
                await _context.SaveChangesAsync();
                // Trigger notification
                var notification = new Notification
                {
                    UserId = data.UID.ToString(),
                    Message = "Your discount request has an update.",
                };
                return Ok("Response Saved and sent to user With RFDFU ID-" + requestForDisCountToUser.RFDFU);
            }
            catch (Exception ex)
            {
                // Log the exception details
                // Use your preferred logging framework here. For example:
                _logger.LogError(ex, "An error occurred while processing the discount request.");

                return StatusCode(500, "An internal server error occurred. Please try again later.");
            }
        }
        [HttpPost("SelectFinalMerchant")]
        public async Task<IActionResult> SelectFinalMerchant(RequestForDisCountToUser data)
        {
            if (data == null)
            {
                return BadRequest("Invalid request data.");
            }

            var serData = _context.RequestForDisCountToUsers.AsNoTracking().Where(x => x.RFDFU == data.RFDFU).FirstOrDefault();
            if (serData == null)
            {
                return NotFound("Request for discount not found.");
            }


            try
            {
                // Create a new instance of RequestForDisCountToUser and set its properties
                var requestForDisCount = new RequestForDisCountToUser
                {
                    RFDFU = serData.RFDFU, // Make sure to use the same key
                    MID = serData.MID,
                    UID = serData.UID,
                    IsMerchantSelected = 1,
                    SID = serData.SID,
                    FINALPRICE = serData.FINALPRICE,
                    ResponseDateTime = DateTime.Now,
                    IsPaymentDone = 0
                };

                // Attach the new entity and mark it as modified
                _context.Attach(requestForDisCount);
                _context.Entry(requestForDisCount).State = EntityState.Modified;

                await _context.SaveChangesAsync();
                // Trigger the notification
                return Ok("Congratulation. You have select your Merchant. Please navigate to upload your files to process your service.");
            }
            catch (Exception ex)
            {
                // Log the exception details
                _logger.LogError(ex, "An error occurred while processing the discount request.");

                return StatusCode(500, "An internal server error occurred. Please try again later.");
            }
        }
        [HttpPost("DeSelectFinalMerchant")]
        public async Task<IActionResult> DeSelectFinalMerchant(RequestForDisCountToUser data)
        {
            if (data == null)
            {
                return BadRequest("Invalid request data.");
            }

            var serData = _context.RequestForDisCountToUsers.AsNoTracking().Where(x => x.RFDFU == data.RFDFU).FirstOrDefault();
            if (serData == null)
            {
                return NotFound("Request for discount not found.");
            }

            try
            {
                // Create a new instance of RequestForDisCountToUser and set its properties
                var requestForDisCount = new RequestForDisCountToUser
                {
                    RFDFU = serData.RFDFU, // Make sure to use the same key
                    MID = serData.MID,
                    UID = serData.UID,
                    IsMerchantSelected = -1,
                    SID = serData.SID,
                    FINALPRICE = serData.FINALPRICE,
                    ResponseDateTime = DateTime.Now,
                    IsPaymentDone = serData.IsPaymentDone
                };

                // Attach the new entity and mark it as modified
                _context.Attach(requestForDisCount);
                _context.Entry(requestForDisCount).State = EntityState.Modified;

                await _context.SaveChangesAsync();
                return Ok("You have cancel to proced with the selected merchant.");
            }
            catch (Exception ex)
            {
                // Log the exception details
                _logger.LogError(ex, "An error occurred while processing the discount request.");

                return StatusCode(500, "An internal server error occurred. Please try again later.");
            }
        }
        // New API to update status
        [HttpPost("UpdateServiceStatus")]
        public async Task<IActionResult> UpdateServiceStatus([FromBody] CurrentServiceStatus statusUpdate)
        {
            try
            {
                if (statusUpdate == null || string.IsNullOrWhiteSpace(statusUpdate.CurrentStatus))
                {
                    return BadRequest("Invalid status update data.");
                }

                // Check if the record exists for the given UId, MId, and SID; if it does, update the status.
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
                return Ok("Service status updated successfully.");
            }
            catch (Exception ex)
            {

                throw;
            }



        }
        [HttpGet("GetAllCurrentServiceStatus")]
        public async Task<ActionResult<IEnumerable<CurrentServiceStatus>>> GetAllCurrentServiceStatus()
        {
            try
            {
                // Skip and take based on the pageNumber and pageSize
                var CurrentServicesStatus = await _context.CurrentServiceStatus.ToListAsync();

                return Ok(CurrentServicesStatus);
            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpGet("GetCurrentServiceStatusById")]
        public async Task<RequestStatuses> GetCurrentServiceStatusById(int customerId, int RFDFU, int mid)
        {
            try
            {
                // Skip and take based on the pageNumber and pageSize
                var CurrentServicesStatus = await _context.CurrentServiceStatus.Where(x => x.UId == customerId && x.MId == mid && x.RFDFU == RFDFU).FirstOrDefaultAsync();
                var stringStatus = await _context.RequestStatuses.Where(x => x.StatusID == Convert.ToInt32(CurrentServicesStatus.CurrentStatus)).FirstOrDefaultAsync();
                return stringStatus;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
    //Defines the data structure returned by the search endpoint, including service name, company name, merchant location, price, and average rating.
    public class ServiceMerchantDTO
    {
        public int MID { get; set; }
        public int SID { get; set; }
        public string MERCHANTNAME { get; set; }
        public string SERVICENAME { get; set; }
        public string? ServiceImage { get; set; }
        public int? PRICE { get; set; }
        public int SERVICERATING { get; set; }
        public string MERCHANTLOCATION { get; set; }
        public bool IsRequestedAlready { get; set; }
        public bool IsRequestedEligibility { get; set; }
        public bool IsEligibilityApproved { get; set; }
        public bool IsWaitingEligibilityResponseFromUser { get; set; }
        public int RequiresEligibility { get; set; }

    }
    public class CatWithMerchant
    {
        public string? MID { get; set; }
        public string? SID { get; set; }
        public string? MERCHANTNAME { get; set; }
        public string? SERVICENAME { get; set; }
        public string? PRICE { get; set; }
        public string? MERCHANTLOCATION { get; set; }
    }
    public class RequestForDiscountViewModel
    {
        public int RFDTM { get; set; }
        public int SID { get; set; }
        public int UID { get; set; }
        public int MID { get; set; }
        public string ServiceName { get; set; }
        public int? ServicePrice { get; set; }
        public DateTime? RequestDatetime { get; set; }
    }
    public class SubmitResponseByMerchant
    {
        public string RFDTM { get; set; }
        public string DiscountPrice { get; set; }
        public string MID { get; set; }
        public string UID { get; set; }
        public string SID { get; set; }
    }
    public class RequestForDisCountToUserViewModel
    {
        public int SID { get; set; }
        public int ServicePrice { get; set; }
        public string ServiceName { get; set; }
        public int MerchantID { get; set; }
        public decimal FINALPRICE { get; set; }
        public int UID { get; set; }
        public int RFDFU { get; set; }
        public int? IsMerchantSelected { get; set; }
        public int? IsPaymentDone { get; set; }
        public DateTime? ResponseDateTime { get; set; }
        public string? CurrentStatus { get; set; }
        public bool IsRequestCompleted { get; set; }
    }
    public class SearchRequest
    {
        [MaxLength(100)]
        public string Keyword { get; set; }

        [MaxLength(100)]
        public string CategoryName { get; set; }

        [MaxLength(100)]
        public string SubCategoryName { get; set; }

        [MaxLength(100)]
        public string Location { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? MinPrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? MaxPrice { get; set; }

        [Range(0, 5)]
        public int? MinRating { get; set; }

        [MaxLength(100)]
        public string MerchantName { get; set; }

        [RegularExpression("price_high_to_low|price_low_to_high")]
        public string SortBy { get; set; }
    }
    public class ServiceDto
    {
        public string? ServiceName { get; set; }
        public string? CompanyName { get; set; }
        public string? MerchantLocation { get; set; }
        public decimal Price { get; set; }
        public double AverageRating { get; set; }
    }
    public class ServiceMerchantKey
    {
        public int SID { get; set; }
        public int MID { get; set; }

        public override bool Equals(object obj)
        {
            return obj is ServiceMerchantKey key &&
                   SID == key.SID &&
                   MID == key.MID;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(SID, MID);
        }
    }
}
