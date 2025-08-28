using AFFZ_Provider.Models;
using AFFZ_Provider.Utils;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
public class MenuViewComponent : ViewComponent
{
    private readonly HttpClient _httpClient;
    private readonly IDataProtector _protector;
    public MenuViewComponent(IHttpClientFactory httpClientFactory, IDataProtectionProvider provider)
    {
        _httpClient = httpClientFactory.CreateClient("Main");
        _protector = provider.CreateProtector("Example.SessionProtection");
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        // Call API to get menus based on user type
        var response = await _httpClient.GetStringAsync($"Menus/GetMenusByUserType?userType=Merchant");
        var data = JsonConvert.DeserializeObject<List<MenuItems>>(response);
        string MerchantName = HttpContext.Session.GetEncryptedString("ProviderName", _protector);
        MenuView view = new MenuView
        {
            UserName = MerchantName,
            MenuItems = data
        };

        return View(view);
    }
}
