using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CursorMCP.Api.Services;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using System.Net.Http.Headers;
using System.Text;


var builder = Host.CreateEmptyApplicationBuilder(settings: null);
var configuration = builder.Configuration;
var services = builder.Services;
builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

// Configure Serilog
var logPath = configuration.GetValue<string>("Logging:File:Path") ?? "logs/sonarqube-api.log";
var fileSizeLimitBytes = configuration.GetValue<long>("Logging:File:FileSizeLimitBytes", 10485760);
var maxRollingFiles = configuration.GetValue("Logging:File:MaxRollingFiles", 5);

// Ensure logs directory exists
var logsDir = Path.GetDirectoryName(logPath);
if (!string.IsNullOrEmpty(logsDir) && !Directory.Exists(logsDir))
{
    Directory.CreateDirectory(logsDir);
}

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File(
        logPath,
        fileSizeLimitBytes: fileSizeLimitBytes,
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: maxRollingFiles)
    .CreateLogger();

// Register services
services.AddMemoryCache();
services.AddHttpClient();
services.AddSingleton(_ =>
{
    var client = new HttpClient();
    var baseUrl = configuration["SonarQube:BaseUrl"];
    var token = configuration["SonarQube:Token"];

    if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(token))
    {
        throw new InvalidOperationException("SonarQube configuration is missing. Please check appsettings.json");
    }

    var authenticationHeader = new AuthenticationHeaderValue("Basic",
        Convert.ToBase64String(Encoding.ASCII.GetBytes($"{token}:")));
    
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Authorization = authenticationHeader;

    return client;
});

services.AddSingleton<ICacheService, CacheService>();
services.AddSingleton<ISonarQubeService, SonarQubeService>();


var app = builder.Build();

await app.RunAsync();
