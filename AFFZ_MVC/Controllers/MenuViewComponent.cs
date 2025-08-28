using AFFZ_Customer.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
public class MenuViewComponent : ViewComponent
{
    private readonly HttpClient _httpClient;

    public MenuViewComponent(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("Main");
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var response = await _httpClient.GetStringAsync("Menus/GetMenusByUserType?userType=Customer");
        var data = JsonConvert.DeserializeObject<List<MenuItems>>(response);

        MenuView view = new MenuView
        {
            UserName = "testing",
            MenuItems = data
        };
        return View(view);
    }
}
