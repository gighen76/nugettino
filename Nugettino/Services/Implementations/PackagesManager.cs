
using Microsoft.Extensions.Options;
using Nugettino.Configurations;
using Nugettino.Extensions;
using Nugettino.Models;

namespace Nugettino.Services.Implementations
{
    public class PackagesManager : IPackagesManager
    {

        private readonly IPackagesCollector _packagesCollector;
        private readonly NugettinoOptions _options;
        private readonly ILogger _logger;

        private List<PackageInfo>? _packageInfos;


        public PackagesManager(IPackagesCollector packagesCollector, IOptions<NugettinoOptions> options, ILogger<PackagesManager> logger) 
        {
            _packagesCollector = packagesCollector;
            _options = options.Value;
            _logger = logger;
        }

        public List<PackageInfo>? PackageInfos => _packageInfos;

        public async Task RefreshAsync()
        {
            _packageInfos = await _packagesCollector.CollectPackageInfosAsync(_options.PackagesPath).ToListAsync();
        }

        public string? GetNupkgPath(string id, string version)
        {
            return _packageInfos?.SingleOrDefault(pi => pi.Match(id, version))?.GetNupkgPath(_options.PackagesPath);
        }

        public string? GetNuspecPath(string id, string version)
        {
            return _packageInfos?.SingleOrDefault(pi => pi.Match(id, version))?.GetNuspecPath(_options.PackagesPath);
        }
    }
}
