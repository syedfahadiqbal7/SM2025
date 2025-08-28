using AFFZ_Customer.Models;
using AFFZ_Customer.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Mvc.Razor;
// using SCAPI.ServiceDefaults; // Removed for CI/CD compatibility
using System.Net;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddServiceDefaults();
builder.Services.AddControllersWithViews();

// Configure custom view locations
builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    options.ViewLocationFormats.Clear();
    options.ViewLocationFormats.Add("/Views/Pages/{0}.cshtml");
    options.ViewLocationFormats.Add("/Views/Shared/{0}.cshtml");
    options.ViewLocationFormats.Add("/Views/{0}.cshtml");
});

// Load shared configuration
var sharedConfig = new ConfigurationBuilder()
    .AddJsonFile(builder.Configuration["SharedFileLocation"].ToString(), optional: false, reloadOnChange: true)
    .Build();

// Get BasePath and UsePort setting
var basePath = sharedConfig["BasePath"] ?? throw new Exception("BasePath is missing in shared configuration.");
var usePort = Convert.ToBoolean(sharedConfig["UsePort"]);
var publicDomain = sharedConfig["PublicDomain"];

// Function to construct URLs dynamically based on BasePath and UsePort setting
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

// Define configuration programmatically
builder.Services.Configure<AppSettings>(options =>
{
    options.BaseIpAddress = basePath;
    options.PublicDomain = publicDomain;
    options.ApiHttpsPort = apiHttpsUrl;
    options.AdminHttpsPort = adminHttpsUrl;
    options.MerchantHttpsPort = providerHttpsUrl;
    options.CustomerHttpsPort = customerHttpsUrl;
    options.StripeSecretKey = builder.Configuration["Security:StripeSecretKey"];
    options.StripePublishableKey = builder.Configuration["Security:StripePublishableKey"];
});

// Configure CORS dynamically with security restrictions
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(
            customerHttpsUrl,
            providerHttpsUrl,
            webFrontHttpsUrl,
            adminHttpsUrl,
            apiHttpsUrl
        )
        .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
        .WithHeaders("Authorization", "Content-Type", "Accept")
        .AllowCredentials()
        .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
});

// Add Data Protection
builder.Services.AddDataProtection();

// Configure cookies
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.None;
    options.HttpOnly = HttpOnlyPolicy.None;
    options.Secure = CookieSecurePolicy.Always;
});

// Add session services
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "SmartCenter";
});

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

// Add services
builder.Services.AddHttpClient<NotificationService>();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// Certificate validation
var handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
};

// Configure Kestrel if required
if (useKestrel == 1)
{
    builder.WebHost.UseKestrel(options =>
    {
        options.Listen(IPAddress.Any, Convert.ToInt32(sharedConfig["Ports:AFFZ_Customer:Http"]));
        options.Listen(IPAddress.Any, Convert.ToInt32(sharedConfig["Ports:AFFZ_Customer:Https"]), listenOptions =>
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
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle("Google", options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    options.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "given_name");
    options.ClaimActions.MapJsonKey(ClaimTypes.Surname, "family_name");
});


var app = builder.Build();

// Enable CORS
app.UseCors("AllowSpecificOrigins");

// Enable logging
var loggerFactory = app.Services.GetService<ILoggerFactory>();
if (loggerFactory != null)
{
    loggerFactory.AddFile(builder.Configuration["Logging:LogFilePath"]?.ToString() ?? "Logs/app.log");
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseAuthentication();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
//app.UseSessionExpiryMiddleware();

app.UseRouting();
app.UseAuthorization();

// Configure endpoints
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}");//Homepage
//pattern: "{controller=Login}/{action=Index}/{id?}");--Customer

app.Use(async (context, next) =>
{
    context.Items["BaseIP"] = basePath;
    await next();
});

app.Run();