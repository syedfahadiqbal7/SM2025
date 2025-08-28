using AFFZ_Customer.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

namespace AFFZ_Customer.Controllers
{
    public class FileUploadController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private ILogger<FileUploadController> _logger;

        public FileUploadController(IWebHostEnvironment environment, ILogger<FileUploadController> logger)
        {
            _environment = environment;
            _logger = logger;
        }
        public async Task<IActionResult> Index()
        {
            var jsonResponse = await WebApiHelper.GetData("/api/FileUpload/GetFilesList?id=1");
            if (!string.IsNullOrEmpty(jsonResponse))
            {
                var DocListSerialize = JsonConvert.DeserializeObject<string>(jsonResponse);
                List<UploadedFile> DocumentList = JsonConvert.DeserializeObject<List<UploadedFile>>(DocListSerialize);
                // Check TempData for the response message
                FileUploadViewModel model = new FileUploadViewModel();
                model.UploadedFiles = DocumentList;
                if (TempData.ContainsKey("SaveResponse"))
                {
                    if (TempData["SavedResponseDocuments"] != null)
                    {
                        ViewBag.SaveResponse = TempData["SavedResponseDocuments"];
                    }
                    else
                    {
                        ViewBag.SaveResponse = model;
                    }
                }

            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> UploadDocuments(FileUploadViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Get the current user's username
                var username = User.FindFirstValue(ClaimTypes.Name);

                // Create a folder for the user
                var folderPath = Path.Combine(_environment.WebRootPath, "uploads", username);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Save the files to the user's folder
                model.UploadedFiles = new List<UploadedFile>();
                foreach (var file in model.Files)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        var filePath = Path.Combine(folderPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        // Add file information to the database
                        var uploadedFile = new UploadedFile
                        {
                            FileName = fileName,
                            ContentType = file.ContentType,
                            FileSize = file.Length,
                            FolderName = username,
                            Status = "Pending", // Initial status
                            UserId = 1
                        };
                        model.UploadedFiles.Add(uploadedFile);
                        string responseMessage = await WebApiHelper.PostData("/api/FileUpload/UploadFiles", uploadedFile);


                    }
                }
                // Use TempData to pass the response message to the Index view
                TempData["SaveResponse"] = model;

                // Redirect to the Index action
                return RedirectToAction("Index");
            }

            return View(model);
        }

        // Action to download a file
        [HttpGet]
        public IActionResult DownloadFile(string fileName, string folderName)
        {
            var filePath = Path.Combine(_environment.WebRootPath, "uploads", folderName, fileName);
            if (System.IO.File.Exists(filePath))
            {
                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, "application/octet-stream", fileName);
            }
            else
            {
                return NotFound();
            }
        }

        // Actions for Provider/Merchant to approve/reject files and notify the customer
        // ... (Implement these actions in your controller)
    }
    //public class FileUploadViewModel
    //{
    //    [Required]
    //    public IFormFileCollection Files { get; set; }
    //    public int UserId { get; set; }
    //    public List<UploadedFile> UploadedFiles { get; set; }
    //}
}
