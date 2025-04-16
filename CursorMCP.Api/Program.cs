using System.IO;
using System.Net.Http.Headers;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

using CursorMCP.Api.Services;

using ModelContextProtocol;

using Serilog;
using Serilog.Events;

var builder = Host.CreateEmptyApplicationBuilder(settings: null);
builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();


// Configure Serilog
var logPath = builder.Configuration.GetValue<string>("Logging:File:Path") ?? "logs/sonarqube-api.log";
var fileSizeLimitBytes = builder.Configuration.GetValue<long>("Logging:File:FileSizeLimitBytes", 10485760);
var maxRollingFiles = builder.Configuration.GetValue<int>("Logging:File:MaxRollingFiles", 5);

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
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddSingleton<ISonarQubeService, SonarQubeService>();


var app = builder.Build();


await app.RunAsync();
