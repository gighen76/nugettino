using Nugettino.Models;

namespace Nugettino.Services
{
    public interface IPackagesManager
    {

        public Task RefreshAsync();

        public List<PackageInfo>? PackageInfos { get; }

        public string? GetNupkgPath(string id, string version);

        public string? GetNuspecPath(string id, string version);

    }
}
