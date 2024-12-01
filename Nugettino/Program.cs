using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Nugettino.Configurations;
using Nugettino.Extensions;
using Nugettino.Models;
using Nugettino.Services;
using Nugettino.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<NugettinoOptions>(builder.Configuration.GetSection("NuGet"));
builder.Services.AddSingleton<IPackagesCollector, PackagesCollector>();

var app = builder.Build();
await app.Services.GetRequiredService<IPackagesCollector>().RefreshAsync();


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
    var foundPackageInfos = packagesCollector.PackageInfos?
        .Where(pi => string.IsNullOrEmpty(q) || pi.Id.Contains(q, StringComparison.OrdinalIgnoreCase))
        .Skip(skip)
        .Take(take) ?? new List<PackageInfo>();

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

app.MapGet("/packages/{id}/index.json", ([FromServices] IPackagesCollector packagesCollector, string id) =>
{
    var foundPackageInfos = packagesCollector.PackageInfos?
        .Where(pi => pi.Id.Equals(id, StringComparison.OrdinalIgnoreCase)) ?? new List<PackageInfo>();
    return new
    {
        versions = foundPackageInfos.Select(pi => pi.Version)
    };
});

app.MapGet("/packages/{id}/{version}/{fileName}.nupkg", ([FromServices] IPackagesCollector packagesCollector, string id, string version, string fileName) =>
{

    if (!fileName.Equals($"{id}.{version}"))
    {
        return Results.NotFound("Invalid file name");
    }

    var foundPackageInfo = packagesCollector.PackageInfos?
        .SingleOrDefault(pi => pi.Id.Equals(id, StringComparison.OrdinalIgnoreCase) && pi.Version.Equals(version, StringComparison.OrdinalIgnoreCase));

    if (foundPackageInfo == null)
    {
        return Results.NotFound("Package not found");
    }

    var stream = new FileStream(foundPackageInfo.ToNupkgPath(packagesCollector.PackagesPath), FileMode.Open, FileAccess.Read);
    return Results.File(stream, "application/octet-stream", $"{fileName}.nupkg");

});

app.MapGet("/packages/{id}/{version}/{fileName}.nuspec", ([FromServices] IPackagesCollector packagesCollector, string id, string version, string fileName) =>
{

    var foundPackageInfo = packagesCollector.PackageInfos?
        .SingleOrDefault(pi => pi.Id.Equals(id, StringComparison.OrdinalIgnoreCase) && pi.Version.Equals(version, StringComparison.OrdinalIgnoreCase));

    if (foundPackageInfo == null)
    {
        return Results.NotFound("Package not found");
    }

    var stream = new FileStream(foundPackageInfo.ToNuspecPath(packagesCollector.PackagesPath), FileMode.Open, FileAccess.Read);
    return Results.File(stream, "application/xml", $"{fileName}.nuspec");
});

await app.RunAsync();
