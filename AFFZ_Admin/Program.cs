using AFFZ_Admin.Models;
using AFFZ_Admin.Utils;
using Microsoft.AspNetCore.Mvc.Razor;
// using SCAPI.ServiceDefaults; // Removed for CI/CD compatibility
using System.Net;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddControllersWithViews();

// Load configuration
var sharedConfig = new ConfigurationBuilder()
    .AddJsonFile(builder.Configuration["SharedFileLocation"].ToString(), optional: false, reloadOnChange: true)
    .Build();

var basePath = sharedConfig["BasePath"];
var usePort = Convert.ToBoolean(sharedConfig["UsePort"]);
var publicDomain = sharedConfig["PublicDomain"];

// Function to construct URLs based on BasePath and UsePort setting
string ConstructUrl(string serviceName, string protocol)
{
    var port = sharedConfig[$"Ports:{serviceName}:{protocol}"];
    return usePort ? $"{basePath}:{port}" : $"{basePath}/{serviceName}";
}

// Construct service URLs
var apiHttpsUrl = ConstructUrl("AFFZ_API", "Https");
var apiHttpUrl = ConstructUrl("AFFZ_API", "Http");
var customerHttpsUrl = ConstructUrl("AFFZ_Customer", "Https");
var customerHttpUrl = ConstructUrl("AFFZ_Customer", "Http");
var adminHttpsUrl = ConstructUrl("AFFZ_Admin", "Https");
var adminHttpUrl = ConstructUrl("AFFZ_Admin", "Http");
var providerHttpsUrl = ConstructUrl("AFFZ_Provider", "Https");
var providerHttpUrl = ConstructUrl("AFFZ_Provider", "Http");
var webFrontHttpsUrl = ConstructUrl("SCAPI_WebFront", "Https");
var webFrontHttpUrl = ConstructUrl("SCAPI_WebFront", "Http");

// Load certificate details
var certificatePath = sharedConfig["Certificate:Path"];
var certificatePassword = sharedConfig["Certificate:Password"];
var useKestrel = Convert.ToInt32(sharedConfig["Certificate:UseKestrel"]);

// Add HTTP Client with dynamic API URL
builder.Services.AddHttpClient("Main", client =>
{
    client.BaseAddress = new Uri($"{apiHttpsUrl}/api/");
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    return new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
    };
});

// Define configuration programmatically
builder.Services.Configure<AppSettings>(options =>
{
    options.BaseIpAddress = basePath;
    options.PublicDomain = publicDomain;
    options.ApiHttpsPort = apiHttpsUrl;
    options.AdminHttpsPort = adminHttpsUrl;
    options.MerchantHttpsPort = providerHttpsUrl;
    options.CustomerHttpsPort = customerHttpsUrl;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(
            customerHttpsUrl,
            providerHttpsUrl,
            webFrontHttpsUrl,
            adminHttpsUrl
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials(); // Required to allow credentials
    });
});

// Configure Razor View Engine for custom view locations
builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    options.ViewLocationFormats.Clear();
    options.ViewLocationFormats.Add("/Views/Pages/{0}.cshtml"); // Custom view path
    options.ViewLocationFormats.Add("/Views/Shared/{0}.cshtml"); // Shared views path
});

// Certificate validation
var handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
};

if (useKestrel == 1)
{
    builder.WebHost.UseKestrel(options =>
    {
        options.Listen(IPAddress.Any, Convert.ToInt32(sharedConfig["Ports:AFFZ_Admin:Http"]));
        options.Listen(IPAddress.Any, Convert.ToInt32(sharedConfig["Ports:AFFZ_Admin:Https"]), listenOptions =>
        {
            listenOptions.UseHttps(certificatePath, certificatePassword);
        });
    });
}
else
{
    Console.WriteLine($"Using IIS");
}

builder.Services.AddSingleton<IAppSettingsService, AppSettingsService>();

var app = builder.Build();

// Enable logging
var loggerFactory = app.Services.GetService<ILoggerFactory>();
loggerFactory.AddFile(builder.Configuration["Logging:LogFilePath"].ToString());

app.UseCors("AllowSpecificOrigins");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();
