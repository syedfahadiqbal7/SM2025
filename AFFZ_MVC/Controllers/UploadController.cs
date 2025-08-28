using Microsoft.AspNetCore.Mvc;
using static AFFZ_Customer.Models.FileModel;


namespace AFFZ_Customer.Controllers
{
    public class UploadController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(UploadModel model)
        {
            var result = new ResultModel(model.File.FileName, model.File.Length, model.File.ContentType, MalwareStatus.Unknown, []);
            return View("Result", result);
        }
    }
}
