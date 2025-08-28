using AFFZ_API.Models;
using AFFZ_API.Models.Partial;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace AFFZ_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EligibilityController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly ILogger<EligibilityController> _logger;

        public EligibilityController(MyDbContext context, ILogger<EligibilityController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // POST: api/Eligibility/SubmitRequest
        [HttpPost("SubmitRequest")]
        public async Task<ActionResult<SResponse>> SubmitEligibilityRequest(EligibilityRequestDTO request)
        {
            _logger.LogInformation("SubmitEligibilityRequest initiated for CustomerId: {CustomerId}, ServiceId: {ServiceId}", request.CustomerID, request.ServiceID);

            try
            {
                var service = await _context.Services.Where(s => s.ServiceId == request.ServiceID).FirstOrDefaultAsync();
                if (service == null)
                {
                    return new SResponse
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = "Service not found."
                    };
                }
                if (service.RequiresEligibility != 1)
                {
                    return new SResponse
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Message = "Eligibility verification is not required for this service."
                    };
                }

                var eligibilityRequest = new EligibilityRequest
                {
                    CustomerID = request.CustomerID,
                    ServiceID = request.ServiceID,
                    MerchantID = service.MerchantID ?? 0,
                    StatusID = 21, // Pending status
                    RequestDetails = request.RequestDetails,
                    CreatedDate = DateTime.UtcNow.ToLocalTime(),
                    UpdatedDate = DateTime.UtcNow.ToLocalTime(),
                    Nationality = request.requestorNationality,
                    ReasonForRejection = ""
                    //Status = _context.RequestStatuses.Where(x => x.StatusID == 21).FirstOrDefault(),
                    //Service = service
                };

                _context.EligibilityRequests.Add(eligibilityRequest);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Eligibility request successfully created with RequestId: {RequestId}", eligibilityRequest.RequestID);

                return new SResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = "Eligibility request submitted successfully.",
                    Data = new { eligibilityRequest.RequestID }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while submitting eligibility request.");
                return new SResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        // GET: api/Eligibility/RequestStatus/{requestId}
        [HttpGet("RequestStatus/{requestId}")]
        public async Task<ActionResult<SResponse>> GetEligibilityRequestStatus(int requestId)
        {
            _logger.LogInformation("Fetching status for Eligibility RequestId: {RequestId}", requestId);

            try
            {
                var request = await _context.EligibilityRequests
                    .Include(r => r.Status)
                    .FirstOrDefaultAsync(r => r.RequestID == requestId);

                if (request == null)
                {
                    return new SResponse
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = "Eligibility request not found."
                    };
                }

                return new SResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Data = new
                    {
                        request.RequestID,
                        request.Status.StatusName,
                        request.ReasonForRejection,
                        request.CreatedDate,
                        request.UpdatedDate,
                        request.Nationality,
                        request.RequestDetails
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching eligibility request status.");
                return new SResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        // POST: api/Eligibility/UpdateRequest
        [HttpPost("UpdateRequest")]
        public async Task<ActionResult<SResponse>> UpdateEligibilityRequest(EligibilityUpdateDTO updateDTO)
        {
            _logger.LogInformation("UpdateEligibilityRequest initiated for RequestId: {RequestId}", updateDTO.RequestID);

            try
            {
                var request = await _context.EligibilityRequests.FirstOrDefaultAsync(r => r.RequestID == updateDTO.RequestID);

                if (request == null)
                {
                    return new SResponse
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = "Eligibility request not found."
                    };
                }

                request.StatusID = updateDTO.StatusID;
                request.ReasonForRejection = updateDTO.ReasonForRejection;
                request.RequestDetails = updateDTO.RequestDetails;
                request.UpdatedDate = DateTime.UtcNow.ToLocalTime();
                request.IsRequestRejected = updateDTO.IsRequestRejected;
                request.IsRespondedByMerchant = updateDTO.IsRespondedByMerchant;
                request.UpdatedDate = DateTime.UtcNow.ToLocalTime();
                _context.EligibilityRequests.Update(request);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Eligibility request updated successfully with RequestId: {RequestId}", updateDTO.RequestID);

                return new SResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = "Eligibility request updated successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating eligibility request.");
                return new SResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }
        //
        [HttpGet("GetPendingRequests")]
        public async Task<ActionResult<SResponse>> GetPendingRequests(int merchantId)
        {
            _logger.LogInformation("Fetching Get Pending Requests For Merchant: {merchantId}", merchantId);

            try
            {
                var request = await _context.EligibilityRequests
                    .Include(r => r.Status)
                    .Include(r => r.Customer)
                    .Include(r => r.Service)
                    .Where(r => r.MerchantID == merchantId).OrderByDescending(r => r.RequestID).ToListAsync();

                if (request == null)
                {
                    return new SResponse
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = "Eligibility request not found."
                    };
                }

                return new SResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Data = request
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching eligibility request status.");
                return new SResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }
        [HttpGet("GetUserPendingRequests")]
        public async Task<ActionResult<SResponse>> GetUserPendingRequests(int userId)
        {
            _logger.LogInformation("Fetching Get Pending Requests For Customer: {userId}", userId);

            try
            {
                var request = await _context.EligibilityRequests
                    .Include(r => r.Status)
                    .Include(r => r.Customer)
                    .Include(r => r.Service)
                    .Where(r => r.CustomerID == userId).OrderByDescending(r => r.RequestID).ToListAsync();

                if (request == null)
                {
                    return new SResponse
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = "Eligibility request not found."
                    };
                }

                return new SResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Data = request
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching eligibility request status.");
                return new SResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }
    }

    public class EligibilityRequestDTO
    {
        public int CustomerID { get; set; }
        public int ServiceID { get; set; }
        public string RequestDetails { get; set; } // JSON for eligibility details
        public string requestorNationality { get; set; } // JSON for eligibility details
        public bool IsRequestRejected { get; set; }
        public bool IsRespondedByMerchant { get; set; }
    }

    public class EligibilityUpdateDTO
    {
        public int RequestID { get; set; }
        public int StatusID { get; set; } // Approved, Rejected, Pending
        public string ReasonForRejection { get; set; }
        public bool IsRequestRejected { get; set; }
        public bool IsRespondedByMerchant { get; set; }
        public string? RequestDetails { get; set; }
    }
}
