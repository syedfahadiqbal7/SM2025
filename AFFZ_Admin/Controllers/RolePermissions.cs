using AFFZ_Admin.Models;
using AFFZ_Admin.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AFFZ_Admin.Controllers
{
    public class RolePermissions : Controller
    {
        private readonly HttpClient _httpClient;

        public RolePermissions(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("Main");
        }


        public async Task<IActionResult> Index(int roleId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Menus/GetMenusPermissionByRoleId?roleId={roleId}");


                var responseString = await response.Content.ReadAsStringAsync();

                List<Menu>? rolesList = JsonSerializer.Deserialize<List<Menu>>(responseString);
                TempData["RoleId"] = roleId;
                ViewBag.RoleName = await GetRoleName(roleId);
                return View("RolePermissions", rolesList);
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePermissions(List<Menu> model)
        {
            try
            {
                List<Permission> permissions = model.SelectMany(x => x.permissions).ToList();
                var temp = Customs.GetJsonContent(permissions);
                var response = await _httpClient.PostAsync("Permissions/UpdatePermissions", Customs.GetJsonContent(permissions));
                //response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();

                return RedirectToAction("Index", "Roles");
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        private async Task<string> GetRoleName(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync("Roles");
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                List<Role> rolesList = JsonSerializer.Deserialize<List<Role>>(responseString);
                string Rolename = rolesList.Where(x => x.RoleId == id).Select(x => x.RoleName).FirstOrDefault();
                return Rolename;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
