using AFFZ_API.Interfaces;
using AFFZ_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Net;

namespace AFFZ_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileUploadController : Controller
    {
        private readonly MyDbContext _context;
        private readonly ILogger<FileUploadController> _logger;
        private readonly IEmailService _emailService;
        public FileUploadController(MyDbContext context, ILogger<FileUploadController> logger, IEmailService emailService)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
        }
        [HttpGet("GetMerchantFile")]
        public async Task<ActionResult<IEnumerable<UploadedFile>>> GetMerchantFile(int merchantId, int RFDFU)
        {
            try
            {
                List<UploadedFile> FileList = await _context.UploadedFiles.Where(x => x.RFDFU == RFDFU && x.MerchantId == merchantId).ToListAsync();
                return Ok(FileList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting the file list.");
                return StatusCode(500, "Internal server error.");
            }
        }
        [HttpGet("GetFilesList")]
        public async Task<ActionResult<IEnumerable<UploadedFile>>> GetFilesList()
        {
            try
            {
                List<UploadedFile> FileList = await _context.UploadedFiles.ToListAsync();
                return Ok(FileList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting the file list.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost("UploadFiles")]
        public async Task<IActionResult> UploadFiles([FromBody] FileUploadModelAPI model)
        {
            _logger.LogInformation("UploadFiles method called with UserId: {UserId}", model.UserId);
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state.");
                    return BadRequest(ModelState);
                }

                foreach (var file in model.UploadedFiles)
                {
                    if (file.FileSize > 0)
                    {
                        var uploadedFile = new UploadedFile
                        {
                            FileName = file.FileName,
                            ContentType = file.ContentType,
                            FileSize = file.FileSize,
                            UserId = model.UserId,
                            MerchantId = file.MerchantId,
                            FolderName = "Documents_" + model.UserId,
                            Status = "Pending",
                            DocumentAddedDate = DateTime.Now,
                            DocumentModifiedDate = DateTime.Now,
                            RFDFU = file.RFDFU,
                            UploadedBy = model.UploadedBy
                        };
                        await _context.UploadedFiles.AddAsync(uploadedFile);
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Files uploaded successfully for UserId: {UserId}", model.UserId);
                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while uploading files for UserId: {UserId}", model.UserId);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("UsersWithDocuments")]
        public async Task<IActionResult> GetUsersWithDocuments(int? statusid = null, int? merchantId = null)
        {
            _logger.LogInformation("GetUsersWithDocuments method called.");

            try
            {
                var result = new List<UserDocumentsViewModel>();

                var merchantParam = new SqlParameter("@MerchantId", merchantId ?? (object)DBNull.Value);
                var statusParam = new SqlParameter("@StatusId", statusid ?? (object)DBNull.Value);

                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "EXEC GetUsersWithDocuments @MerchantId, @StatusId";
                    command.Parameters.Add(merchantParam);
                    command.Parameters.Add(statusParam);
                    command.CommandType = CommandType.Text;

                    _context.Database.OpenConnection();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(new UserDocumentsViewModel
                            {
                                DocumentCount = reader.GetInt32(0),
                                ServiceID = reader.GetInt32(1),
                                ServiceName = reader.GetString(2),
                                ServicePrice = reader.GetInt32(3),
                                ServiceAmountPaidToAdmin = reader.GetInt32(4),
                                SelectedDocumentIds = reader.GetString(5),
                                CategoryID = reader.GetInt32(6),
                                FINALPRICE = reader.GetInt32(7),
                                RFDFU = reader.GetInt32(8),
                                IsPaymentDone = reader.GetInt32(9),
                                IsMerchantSelected = reader.GetInt32(10),
                                MerchantName = reader.GetString(11),
                                CustomerName = reader.GetString(12),
                                UserId = reader.GetInt32(13),
                                MID = reader.GetInt32(14),
                                Quantity = reader.GetInt32(15),
                                ISPAYMENTSUCCESS = reader.GetInt32(16),
                                PAYMENTTYPE = reader.GetString(17),
                                SID = reader.GetInt32(18)
                            });
                        }
                    }
                }

                if (!result.Any())
                {
                    _logger.LogWarning("No users found with uploaded documents.");
                    return NotFound("No users found with uploaded documents.");
                }

                _logger.LogInformation("Users with documents retrieved successfully.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting users with documents.");
                return StatusCode(500, "Internal server error.");
            }
        }
        [HttpGet("ReviewDocuments/{userId}/{rfdfu}")]
        public async Task<IActionResult> ReviewDocuments(int userId, int rfdfu)
        {
            _logger.LogInformation("ReviewDocuments method called for UserId: {UserId}", userId);
            try
            {
                var documents = await _context.UploadedFiles.Where(d => d.UserId == userId && d.RFDFU == rfdfu).ToListAsync();
                if (!documents.Any())
                {
                    _logger.LogWarning("No documents found for UserId: {UserId}", userId);
                    return NotFound("No documents found for the user.");
                }

                _logger.LogInformation("Documents retrieved successfully for UserId: {UserId}", userId);
                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while reviewing documents for UserId: {UserId}", userId);
                return StatusCode(500, "Internal server error.");
            }
        }
        [HttpGet("AllFileVerifies")]
        public async Task<IActionResult> AllFileVerifies(int userId, int RFDFU)
        {
            _logger.LogInformation("ReviewDocuments method called for UserId: {UserId}", userId);
            try
            {
                var fileCounts = _context.UploadedFiles
                                .Where(u => u.UserId == userId && u.RFDFU == RFDFU && u.UploadedBy == "Customer")
                                .GroupBy(u => 1) // Grouping by a constant to get a single group
                                .Select(g => new
                                {
                                    TotalFiles = g.Count(),
                                    TotalVerified = g.Count(u => u.Status == "Verified"),
                                    TotalPending = g.Count(u => u.Status == "Pending")
                                })
                                .FirstOrDefault(); // Use FirstOrDefault to get the result as an object
                if (fileCounts == null)
                {
                    _logger.LogWarning("No documents found for UserId: {UserId}", userId);
                    return NotFound("No documents found for the user.");
                }

                _logger.LogInformation("Documents retrieved successfully for UserId: {UserId}", userId);
                return Ok(fileCounts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while reviewing documents for UserId: {UserId}", userId);
                return StatusCode(500, "Internal server error.");
            }
        }
        [HttpPost("VerifyDocument/{documentId}")]
        public async Task<IActionResult> VerifyDocument(int documentId)
        {
            _logger.LogInformation("VerifyDocument method called for DocumentId: {DocumentId}", documentId);
            try
            {
                var document = await _context.UploadedFiles.FindAsync(documentId);
                if (document == null)
                {
                    _logger.LogWarning("Document not found for DocumentId: {DocumentId}", documentId);
                    return NotFound("Document not found.");
                }

                document.Status = "Verified";
                document.DocumentModifiedDate = DateTime.Now;
                await _context.SaveChangesAsync();
                var notification = new Notification
                {
                    UserId = document.UserId.ToString(),
                    Message = "Merchant has verify some document you shared.",
                };
                _logger.LogInformation("Document verified successfully for DocumentId: {DocumentId}", documentId);
                //return Ok(new { Message = "Document verified successfully." });
                return Ok("Document verified successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while verifying document for DocumentId: {DocumentId}", documentId);
                return StatusCode(500, "Internal server error.");
            }
        }
        [HttpPost("UpdateDocumenttoPendingStatus/{documentId}")]
        public async Task<IActionResult> UpdateDocumenttoPendingStatus(int documentId)
        {
            _logger.LogInformation("DeleteDocument method called for DocumentId: {DocumentId}", documentId);
            try
            {
                var document = await _context.UploadedFiles.FindAsync(documentId);
                if (document == null)
                {
                    _logger.LogWarning("Document not found for DocumentId: {DocumentId}", documentId);
                    return NotFound("Document not found.");
                }
                document.Status = "Pending";
                document.DocumentModifiedDate = DateTime.Now;
                await _context.SaveChangesAsync();
                _logger.LogInformation("User Has Reshared the Document DocumentId: {DocumentId}", documentId);
                return Ok("Document Updated Successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while Deleting document for DocumentId: {DocumentId}", documentId);
                return StatusCode(500, "Internal server error.");
            }
        }
        [HttpPost("DeleteDocument/{documentId}")]
        public async Task<IActionResult> DeleteDocument(int documentId)
        {
            _logger.LogInformation("DeleteDocument method called for DocumentId: {DocumentId}", documentId);
            try
            {
                var document = await _context.UploadedFiles.FindAsync(documentId);
                if (document == null)
                {
                    _logger.LogWarning("Document not found for DocumentId: {DocumentId}", documentId);
                    return NotFound("Document not found.");
                }
                _context.Remove(document);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Document Deleted successfully for DocumentId: {DocumentId}", documentId);
                //return Ok(new { Message = "Document Deleteted successfully." });
                return Ok("Document Deleteted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while Deleting document for DocumentId: {DocumentId}", documentId);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost("ResendDocument/{documentId}")]
        public async Task<IActionResult> ResendDocument(int documentId)
        {
            _logger.LogInformation("ResendDocument method called for DocumentId: {DocumentId}", documentId);
            try
            {
                var document = await _context.UploadedFiles.FindAsync(documentId);
                if (document == null)
                {
                    _logger.LogWarning("Document not found for DocumentId: {DocumentId}", documentId);
                    return NotFound("Document not found.");
                }

                document.Status = "Resend";
                document.DocumentModifiedDate = DateTime.Now;
                await _context.SaveChangesAsync();
                var notification = new Notification
                {
                    UserId = document.UserId.ToString(),
                    Message = "Merchant has request some document you shared to upload again.",
                };
                _logger.LogInformation("Document marked for resending for DocumentId: {DocumentId}", documentId);
                //return Ok(new { Message = "Document marked for resending." });
                return Ok("Document marked for resending.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while resending document for DocumentId: {DocumentId}", documentId);
                return StatusCode(500, "Internal server error.");
            }
        }
        [HttpGet("GetServiceFileListByRFDFUId")]
        public async Task<IActionResult> GetServiceFileListByRFDFUId(int rfdfuId, string UploadedBy)
        {
            try
            {
                if (rfdfuId <= 0)
                {
                    return BadRequest("Invalid RFDFU ID.");
                }

                var rfdfuParam = new SqlParameter("@RFDFU_ID", rfdfuId);
                var uploadedByParam = new SqlParameter("@UploadedBy", UploadedBy ?? (object)DBNull.Value);

                var result = await _context.Set<DocumentInfo>()
                    .FromSqlRaw("EXEC GetServiceFileListByRFDFUId @RFDFU_ID, @UploadedBy", rfdfuParam, uploadedByParam)
                    .ToListAsync();

                if (result == null || !result.Any())
                {
                    return NotFound("No data found for the given RFDFU ID.");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching discount data by RFDFU ID.");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal server error occurred.");
            }
        }
        [HttpGet("GetQuantityOfService")]
        public async Task<string> GetQuantityOfService(int rfdfuId)
        {
            try
            {
                string result = string.Empty;
                if (rfdfuId <= 0)
                {
                    return "Invalid RFDFU ID.";
                }

                result = await (from r in _context.RequestForDisCountToUsers
                                join sm in _context.PaymentHistories on r.IsPaymentDone equals sm.ID
                                where r.RFDFU == rfdfuId
                                select sm.Quantity.ToString()).FirstOrDefaultAsync();

                if (result == null || !result.Any())
                {
                    return "No Quantity Details found for the given RFDFU ID.";
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching discount data by RFDFU ID.");
                return "An internal server error occurred.";
            }
        }
        [HttpGet("GetListOfDocuments")]
        public async Task<IActionResult> GetListOfDocuments(int userId, int RFDFU)
        {
            _logger.LogInformation("GetListOfDocuments method called for UserId: {UserId}", userId);
            try
            {
                var fileList = await _context.UploadedFiles
                                .Where(u => u.UserId == userId && u.RFDFU == RFDFU && u.UploadedBy == "Customer")
                                .ToListAsync(); // Use FirstOrDefault to get the result as an object
                if (fileList == null)
                {
                    _logger.LogWarning("No documents found for UserId: {UserId}", userId);
                    return NotFound("No documents found for the user.");
                }

                _logger.LogInformation("Documents retrieved successfully for UserId: {UserId}", userId);
                return Ok(fileList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while reviewing documents for UserId: {UserId}", userId);
                return StatusCode(500, "Internal server error.");
            }
        }
        [HttpPost("DeleteUploadedFilesByStatus")]
        public async Task<IActionResult> DeleteUploadedFilesByStatus(int rfdfu)
        {
            try
            {
                // Log the start of the operation
                _logger.LogInformation("Executing stored procedure: DeleteUploadedFilesByStatus");

                // Execute the stored procedure
                await _context.Database.ExecuteSqlRawAsync("EXEC [dbo].[DeleteUploadedFilesByStatus]");

                // Log the successful execution
                _logger.LogInformation("Stored procedure executed successfully.");

                // Return a success response
                return Ok(new { Message = "Files deleted successfully based on status." });
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError(ex, "An error occurred while deleting uploaded files by status.");

                // Return an error response
                return StatusCode(500, "Internal server error.");
            }
        }
        private void NotifyMerchant(int userId)
        {
            // Fetch merchant details based on userId or other logic
            var merchantEmail = "merchant@example.com"; // Replace with actual logic to get the merchant's email

            // Send email or notification
            SendEmail(merchantEmail, "Document Uploaded", "A new document has been uploaded. Please review it.");
        }
        private void SendEmail(string to, string subject, string body)
        {
            _emailService.SendEmail(to, subject, body, to.Split("@")[0]);
        }
        //[HttpPost("FileUpload/SaveFileInfo")]
        //public async Task<IActionResult> SaveFileInfo([FromBody] FileUploadInfo fileInfo)
        //{
        //    try
        //    {
        //        // Save file info to DB
        //        _context.UploadedFiles.Add(new UploadedFile
        //        {
        //            UserId = fileInfo.UserId,
        //            FileName = fileInfo.FileName,
        //            FilePath = fileInfo.FilePath,
        //            UploadedOn = fileInfo.UploadedOn
        //        });
        //        await _context.SaveChangesAsync();

        //        return Ok(new { Message = "File info saved successfully." });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error saving file info.");
        //        return StatusCode(500, "Internal server error.");
        //    }
        //}
    }


    public class FileUploadViewModel
    {
        [Required]
        public List<IFormFileCollection> Files { get; set; }
        public int UserId { get; set; }
        public List<UploadedFile>? UploadedFiles { get; set; }
    }

    public class FileUploadModelAPI
    {
        public int UserId { get; set; }
        public int MID { get; set; }
        public int RFDFU { get; set; }
        public string? UploadedBy { get; set; }

        public List<UploadedFile>? UploadedFiles { get; set; }
    }

    //public class UserDocumentsViewModel
    //{
    //    public int UserId { get; set; }
    //    public int MID { get; set; }
    //    public int DocumentCount { get; set; }
    //}
    public class UserDocumentsViewModel
    {

        public int DocumentCount { get; set; }
        public int ServiceID { get; set; }
        public string ServiceName { get; set; }
        public int ServicePrice { get; set; }
        public int ServiceAmountPaidToAdmin { get; set; }
        public string SelectedDocumentIds { get; set; }
        public int CategoryID { get; set; }
        public int FINALPRICE { get; set; }
        public int RFDFU { get; set; }
        public int IsPaymentDone { get; set; }
        public int IsMerchantSelected { get; set; }
        public string CompanyName { get; set; }
        public string CustomerName { get; set; }
        public int MID { get; set; }
        public int Quantity { get; set; }
        public int ISPAYMENTSUCCESS { get; set; }
        public string PAYMENTTYPE { get; set; }
        public int UserId { get; set; }//
        public int SID { get; set; }//
        public string MerchantName { get; set; }

    }

}
