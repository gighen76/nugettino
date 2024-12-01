
using Microsoft.Extensions.Options;
using Nugettino.Configurations;
using Nugettino.Extensions;
using Nugettino.Models;

namespace Nugettino.Services.Implementations
{
    public class PackagesCollector : IPackagesCollector
    {

        private readonly NugettinoOptions _options;
        private readonly ILogger _logger;

        private List<PackageInfo>? _packageInfos;


        public PackagesCollector(IOptions<NugettinoOptions> options, ILogger<PackagesCollector> logger) 
        {
            _options = options.Value;
            _logger = logger;
        }

        public List<PackageInfo>? PackageInfos => _packageInfos;

        public string PackagesPath => _options.PackagesPath;

        public async Task RefreshAsync()
        {
            if (!Directory.Exists(_options.PackagesPath))
            {
                throw new InvalidOperationException($"Directory '{_options.PackagesPath}' not found. Ensure it exists and contains .nupkg files.");
            }

            var packageInfos = new List<PackageInfo>();

            foreach (var filePath in Directory.EnumerateFiles(_options.PackagesPath, "*.nupkg", SearchOption.AllDirectories))
            {
                _logger.LogInformation("Adding package [{filePath}]", filePath);
                var packageInfo = filePath.ToPackageInfo();
                packageInfos.Add(packageInfo);
            }

            _packageInfos = packageInfos;

        }


        

    }
}
