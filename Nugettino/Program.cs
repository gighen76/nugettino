using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Nugettino.Configurations;
using Nugettino.Services;
using Nugettino.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<NugettinoOptions>(builder.Configuration.GetSection("NuGet"));
builder.Services.AddSingleton<IPackagesCollector, PackagesCollector>();

var app = builder.Build();
await app.Services.GetRequiredService<IPackagesCollector>().RefreshAsync();


// Configura il middleware per servire i file statici
var packagesPath = builder.Configuration.GetSection("NuGet:PackagesPath").Value;
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(packagesPath),
    RequestPath = "/packages",
    ServeUnknownFileTypes = true,
    DefaultContentType = "application/octet-stream"
});

app.MapGet("/", () => "NuGet Feed Server is running. Use /packages to access packages.");

app.MapGet("/v3/index.json", (HttpContext httpContext) => 
{
    var scheme = httpContext.Request.Scheme;
    var hostName = httpContext.Request.Host.ToString();
    var appUrl = $"{scheme}://{hostName}";
    return new
    {
        version = "3.0.0",
        resources = new[]
        {
            new Dictionary<string, string> { { "@type", "SearchQueryService" },  { "@id", $"{appUrl}/v3/search" } },
            new Dictionary<string, string> { { "@type", "SearchQueryService/3.0.0-rc" },  { "@id", $"{appUrl}/v3/search" } },
            new Dictionary<string, string> { { "@type", "SearchQueryService/3.0.0-beta" },  { "@id", $"{appUrl}/v3/search" } },
            new Dictionary<string, string> { { "@type", "SearchQueryService/3.5.0" },  { "@id", $"{appUrl}/v3/search" } },
            new Dictionary<string, string> { { "@type", "PackageBaseAddress/3.0.0" },  { "@id", $"{appUrl}/packages" } },
        }
    };
});

// https://learn.microsoft.com/it-it/nuget/api/package-base-address-resource
// https://learn.microsoft.com/it-it/nuget/api/search-query-service-resource
//Directory.EnumerateFiles(packagesPath, "*.nupkg", SearchOption.AllDirectories).Select()

app.MapGet("/v3/search", ([FromServices] IPackagesCollector packagesCollector, string? q, int skip = 0, int take = 20) =>
{
    var foundPackageInfos = packagesCollector.PackageInfos
        .Where(pi => string.IsNullOrEmpty(q) || pi.FileName.Contains(q, StringComparison.OrdinalIgnoreCase))
        .Skip(skip)
        .Take(take);

    return new
    {
        totalHits = foundPackageInfos.Count(),
        data = foundPackageInfos.Select(pi => new
        {
            id = pi.Id,
            version = pi.Version,
            description = "Fake description",
            authors = new string[] {  }
        })
    };
});


await app.RunAsync();
