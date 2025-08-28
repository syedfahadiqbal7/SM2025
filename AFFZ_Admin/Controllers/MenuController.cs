using AFFZ_Admin.Models;
using AFFZ_Admin.Utils;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace AFFZ_Admin.Controllers
{
    public class MenuController : Controller
    {
        private readonly HttpClient _httpClient;
        private string BaseUrl = string.Empty;
        private string PublicDomain = string.Empty;
        private string ApiHttpsPort = string.Empty;
        private string MerchantHttpsPort = string.Empty;
        private ILogger<MenuController> _logger;

        public MenuController(IHttpClientFactory httpClientFactory, ILogger<MenuController> logger, IAppSettingsService service)
        {
            _httpClient = httpClientFactory.CreateClient("Main");
            BaseUrl = service.GetBaseIpAddress();
            PublicDomain = service.GetPublicDomain();
            ApiHttpsPort = service.GetApiHttpsPort();
            MerchantHttpsPort = service.GetMerchantHttpsPort();
            _logger = logger;
        }

        // GET: Menu
        public async Task<IActionResult> GetAllMenus(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Menu/GetAllMenus?pageNumber={pageNumber}&pageSize={pageSize}");
                var responseString = await response.Content.ReadAsStringAsync();
                var menuList = JsonConvert.DeserializeObject<List<MenuViewModel>>(responseString);

                // Read total count from headers
                var totalCount = int.TryParse(response.Headers.GetValues("X-Total-Count")?.FirstOrDefault(), out var count) ? count : 0;

                ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                ViewBag.CurrentPage = pageNumber;
                return View(menuList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving menu list.");
                return View(new List<MenuViewModel>());
            }
        }


        // GET: Menu/Create
        public IActionResult MenusCreate()
        {
            return PartialView("MenuCreate");
        }

        // POST: Menu/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MenusCreate(MenuViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var menu = new Menu
                    {
                        MenuId = 0, // Assuming new menu will have 0 
                        MenuName = model.MenuName,
                        MenuUrl = model.MenuUrl,
                        MenuIcon = model.MenuIcon,
                        IsVisible = model.IsVisible ? 1 : 0,
                        // Map other fields as needed
                    };

                    var response = await _httpClient.PostAsJsonAsync("Menu/CreateMenu", menu);
                    if (response.IsSuccessStatusCode)
                    {
                        string res = await response.Content.ReadAsStringAsync();
                        TempData["SuccessMessage"] = "Menu created successfully.";
                        return RedirectToAction(nameof(GetAllMenus));
                    }

                    TempData["FailMessage"] = "Failed to create menu.";
                    string _res = await response.Content.ReadAsStringAsync();
                }
                return PartialView("MenuCreate", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating menu.");
                return View(model);
            }
        }

        // GET: Menu/Edit/5
        public async Task<IActionResult> MenusEdit(int id)
        {
            try
            {
                var menu = await _httpClient.GetFromJsonAsync<Menu>($"Menu/GetMenuById?id={id}");
                if (menu == null)
                    return NotFound();
                var _menu = new MenuViewModel
                {
                    MenuId = menu.MenuId, // Assuming new menu will have 0 
                    MenuName = menu.MenuName,
                    MenuUrl = menu.MenuUrl,
                    MenuIcon = menu.MenuIcon,
                    IsVisible = menu.IsVisible == 1 ? true : false,
                    // Map other fields as needed
                };
                return PartialView("MenuEdit", _menu);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading menu for editing.");
                return NotFound();
            }
        }

        // POST: Menu/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MenusEdit(int id, MenuViewModel model)
        {
            try
            {
                if (id != model.MenuId)
                    return BadRequest();

                if (ModelState.IsValid)
                {
                    var menu = new Menu
                    {
                        MenuId = model.MenuId, // Assuming new menu will have 0 
                        MenuName = model.MenuName,
                        MenuUrl = model.MenuUrl,
                        MenuIcon = model.MenuIcon,
                        IsVisible = model.IsVisible ? 1 : 0,
                        // Map other fields as needed
                    };
                    var response = await _httpClient.PostAsJsonAsync($"Menu/UpdateMenu?id={id}", menu);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Menu updated successfully.";
                        return RedirectToAction(nameof(GetAllMenus));
                    }

                    TempData["FailMessage"] = "Failed to update menu.";
                }

                return PartialView("MenuEdit", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating menu.");
                return View(model);
            }
        }

        // GET: Menu/Delete/5
        public async Task<IActionResult> MenusDelete(int id)
        {
            try
            {
                var menu = await _httpClient.GetFromJsonAsync<Menu>($"Menu/GetMenuById?id={id}");
                if (menu == null)
                    return NotFound();
                var _menu = new MenuViewModel
                {
                    MenuId = menu.MenuId, // Assuming new menu will have 0 
                    MenuName = menu.MenuName,
                    MenuUrl = menu.MenuUrl,
                    MenuIcon = menu.MenuIcon,
                    IsVisible = menu.IsVisible == 1 ? true : false,
                    // Map other fields as needed
                };

                return PartialView("MenuDelete", _menu);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading menu for deletion.");
                return RedirectToAction(nameof(GetAllMenus));
            }
        }

        // POST: Menu/DeleteConfirmed/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MenusDeleteConfirmed(int MenuId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"Menu/DeleteMenu?id={MenuId}");
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Menu deleted successfully.";
                    return RedirectToAction(nameof(GetAllMenus));
                }

                TempData["FailMessage"] = "Failed to delete menu.";
                return RedirectToAction(nameof(GetAllMenus));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting menu.");
                return RedirectToAction(nameof(GetAllMenus));
            }
        }
    }
    public class MenuViewModel
    {
        [Key]
        public int MenuId { get; set; }

        [Required(ErrorMessage = "Menu Name is required.")]
        [Display(Name = "Menu Name")]
        public string MenuName { get; set; }

        [Display(Name = "Icon (FontAwesome or Image)")]
        public string MenuIcon { get; set; }

        [Display(Name = "Link (URL or Action)")]
        public string MenuUrl { get; set; }

        [Display(Name = "Parent Menu")]
        public int? ParentId { get; set; }

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; }

        [Display(Name = "Is Active")]
        public bool IsVisible { get; set; }

        // Optional: to display parent menu name
        [Display(Name = "Parent Menu Name")]
        public string? ParentMenuName { get; set; }

        // Optional: for UI dropdown binding
        public List<MenuViewModel>? ParentMenuList { get; set; }
    }
}
