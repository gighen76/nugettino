using Microsoft.AspNetCore.Mvc;
using Nugettino.Configurations;
using Nugettino.Models;
using Nugettino.Services;
using Nugettino.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<NugettinoOptions>(builder.Configuration.GetSection("NuGet"));
builder.Services.AddSingleton<IPackagesManager, PackagesManager>();
builder.Services.AddSingleton<IPackagesCollector, PackagesCollector>();
builder.Services.AddSingleton<IFileSystem, FileSystem>();

var app = builder.Build();
await app.Services.GetRequiredService<IPackagesManager>().RefreshAsync();


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

app.MapGet("/v3/search", ([FromServices] IPackagesManager packagesManager, string? q, int skip = 0, int take = 20) =>
{
    var foundPackageInfos = packagesManager.PackageInfos?
        .Where(pi => string.IsNullOrEmpty(q) || pi.IdDirectory.Contains(q, StringComparison.OrdinalIgnoreCase));

    var grupedPackages = foundPackageInfos.GroupBy(pi => pi.IdDirectory)
    .Select(g => new { 
        LastPackageInfo = g.OrderByDescending(pi => pi.Version).First() 
    })
        .Skip(skip)
        .Take(take);

    return new
    {
        totalHits = foundPackageInfos.Count(),
        data = grupedPackages.Select(gp => new
        {
            id = gp.LastPackageInfo.Id,
            version = gp.LastPackageInfo.Version,
            description = gp.LastPackageInfo.Description,
            authors = gp.LastPackageInfo.Authors?.Split(',')
        })
    };
});

app.MapGet("/packages/{id}/index.json", ([FromServices] IPackagesManager packagesManager, string id) =>
{
    var foundPackageInfos = packagesManager.PackageInfos?.Where(pi => pi.IdDirectory.Contains(id, StringComparison.OrdinalIgnoreCase));

    return new
    {
        versions = foundPackageInfos?.Select(pi => pi.Version) ?? new List<string>()
    };
});

app.MapGet("/packages/{id}/{version}/{fileName}.nupkg", ([FromServices] IPackagesManager packagesManager, string id, string version, string fileName) =>
{

    var nupkgPath = packagesManager.GetNupkgPath(id, version);
    if (nupkgPath == null || nupkgPath.EndsWith(fileName, StringComparison.OrdinalIgnoreCase))
    {
        return Results.NotFound("nupkg not found");
    }
    var stream = new FileStream(nupkgPath, FileMode.Open, FileAccess.Read);
    return Results.File(stream, "application/octet-stream", $"{fileName}.nupkg");

});

app.MapGet("/packages/{id}/{version}/{fileName}.nuspec", ([FromServices] IPackagesManager packagesManager, string id, string version, string fileName) =>
{
    var nuspecPath = packagesManager.GetNuspecPath(id, version);
    if (nuspecPath == null || nuspecPath.EndsWith(fileName, StringComparison.OrdinalIgnoreCase))
    {
        return Results.NotFound("nupkg not found");
    }
    var stream = new FileStream(nuspecPath, FileMode.Open, FileAccess.Read);
    return Results.File(stream, "application/octet-stream", $"{fileName}.nupkg");
});

await app.RunAsync();
