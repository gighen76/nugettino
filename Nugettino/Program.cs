using Microsoft.Extensions.FileProviders;
using Nugettino.Extensions;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// Legge la directory dei pacchetti in lista di PackageInfos
var packagesPath = builder.Configuration.GetSection("NuGet:PackagesPath").Value;

if (!Directory.Exists(packagesPath))
{
    Console.WriteLine($"Directory '{packagesPath}' not found. Ensure it exists and contains .nupkg files.");
    return;
}

var packageInfos = Directory.EnumerateFiles(packagesPath, "*.nupkg", SearchOption.AllDirectories)
        .Select(fp => fp.ToPackageInfo()).ToList();


// Configura il middleware per servire i file statici
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(packagesPath),
    RequestPath = "/packages",
    ServeUnknownFileTypes = true,
    DefaultContentType = "application/octet-stream"
});

app.MapGet("/", () => "NuGet Feed Server is running. Use /packages to access packages.");

app.MapGet("/v3/index.json", () => new
{
    version = "3.0.0",
    resources = new[]
    {
        new Dictionary<string, string> { { "@type", "SearchQueryService" },  { "@id", $"{app.Urls.First()}/v3/search" } },
        new Dictionary<string, string> { { "@type", "SearchQueryService/3.0.0-rc" },  { "@id", $"{app.Urls.First()}/v3/search" } },
        new Dictionary<string, string> { { "@type", "SearchQueryService/3.0.0-beta" },  { "@id", $"{app.Urls.First()}/v3/search" } },
        new Dictionary<string, string> { { "@type", "SearchQueryService/3.5.0" },  { "@id", $"{app.Urls.First()}/v3/search" } },
        new Dictionary<string, string> { { "@type", "PackageBaseAddress/3.0.0" },  { "@id", $"{app.Urls.First()}/packages" } },
    }
});

// https://learn.microsoft.com/it-it/nuget/api/package-base-address-resource
// https://learn.microsoft.com/it-it/nuget/api/search-query-service-resource
//Directory.EnumerateFiles(packagesPath, "*.nupkg", SearchOption.AllDirectories).Select()

app.MapGet("/v3/search", (string? q, int skip = 0, int take = 20) =>
{
    var foundPackageInfos = packageInfos
        .Where(pi => string.IsNullOrEmpty(q) || pi.FileName.Contains(q, StringComparison.OrdinalIgnoreCase))
        .Skip(skip)
        .Take(take);

    return new
    {
        totalHits = foundPackageInfos.Count(),
        data = foundPackageInfos.Select(name => new
        {
            id = name.Id,
            version = name.Version,
            description = "Fake description",
            authors = new string[] {  }
        })
    };
});


app.Run();
