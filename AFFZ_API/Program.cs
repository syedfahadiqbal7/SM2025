using AFFZ_API;
using AFFZ_API.Interfaces;
using AFFZ_API.Middleware;
using AFFZ_API.Models;
using AFFZ_API.NotificationsHubs;
using AFFZ_API.Services;
using AFFZ_API.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SCAPI.ServiceDefaults;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults
builder.AddServiceDefaults();

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

// Bind AppSettings
builder.Services.Configure<AppSettings>(options =>
{
    options.BaseIpAddress = basePath;
    options.PublicDomain = publicDomain;
    options.ApiHttpsPort = apiHttpsUrl;
    options.MerchantHttpsPort = providerHttpsUrl;
    options.CustomerHttpsPort = customerHttpsUrl;
});

// Bind security settings
builder.Services.Configure<SecuritySettings>(builder.Configuration.GetSection("Security"));
builder.Services.Configure<CorsSettings>(builder.Configuration.GetSection("Cors"));

// Configure CORS dynamically with security restrictions
var corsSettings = builder.Configuration.GetSection("Cors").Get<CorsSettings>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(corsSettings?.AllowedOrigins ?? new[] { customerHttpsUrl, providerHttpsUrl, adminHttpsUrl })
        .WithMethods(corsSettings?.AllowedMethods ?? new[] { "GET", "POST", "PUT", "DELETE", "OPTIONS" })
        .WithHeaders(corsSettings?.AllowedHeaders ?? new[] { "Authorization", "Content-Type", "Accept", "X-Requested-With", "x-signalr-user-agent" })
        .AllowCredentials()
        .SetPreflightMaxAge(TimeSpan.FromMinutes(corsSettings?.PreflightMaxAgeMinutes ?? 10));
    });
});

// Configure services
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
builder.Services.AddSignalR();
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DBCS"))
);

// Configure JWT authentication dynamically
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Get JWT secret from configuration
    var jwtSecret = builder.Configuration["Security:JwtSecretKey"]
        ?? throw new InvalidOperationException("JWT secret key not configured in configuration");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = apiHttpsUrl,
        ValidAudience = apiHttpsUrl,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };
});

// Enable Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure email service
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddTransient<IEmailService, EmailNotifications>();

// Configure Kestrel if required
if (useKestrel == 1)
{
    builder.WebHost.UseKestrel(options =>
    {
        options.Listen(IPAddress.Any, Convert.ToInt32(sharedConfig["Ports:AFFZ_API:Http"]));
        options.Listen(IPAddress.Any, Convert.ToInt32(sharedConfig["Ports:AFFZ_API:Https"]), listenOptions =>
        {
            listenOptions.UseHttps(certificatePath, certificatePassword);
        });
    });
}
else
{
    Console.WriteLine($"Using IIS");
}

// Dependency Injection
builder.Services.AddSingleton<IAppSettingsService, AppSettingsService>();

// Register payment service
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Register Google auth service
builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();

// Register auth service
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// Enable logging
var loggerFactory = app.Services.GetService<ILoggerFactory>();
if (loggerFactory != null)
{
    loggerFactory.AddFile(builder.Configuration["Logging:LogFilePath"]?.ToString() ?? "Logs/app.log");
}

// Enable Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigins");
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");

// Add error handling middleware
app.UseErrorHandling();

app.Run();

// Expose Program for WebApplicationFactory in integration tests
public partial class Program { }