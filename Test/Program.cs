using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http.Headers;
using System.Text;
using CursorMCP.Services;
using Microsoft.Extensions.Configuration;

var builder = Host.CreateEmptyApplicationBuilder(settings: null);
var configuration = builder.Configuration;
var services = builder.Services;

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

// Register services
services.AddMemoryCache();
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
services.AddSingleton<ISimpleLogService, SimpleLogService>();
services.AddSingleton<ICacheService, CacheService>();
services.AddSingleton<ISonarQubeService, SonarQubeService>();

// Get an instance of ISonarQubeService
var sonarQubeService = services.BuildServiceProvider().GetRequiredService<ISonarQubeService>();

var app = builder.Build();

var result = await sonarQubeService.GetIssueDetailsAsync("AZW4H4_yQ2RKoZVJ3375");
await app.RunAsync();