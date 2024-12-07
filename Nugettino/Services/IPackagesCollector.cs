using Nugettino.Models;

namespace Nugettino.Services
{
    public interface IPackagesCollector
    {

        Task<PackageInfo?> CollectPackageInfoAsync(string packagesPath, string idDirectory, string versionDirectory);

        IAsyncEnumerable<PackageInfo> CollectPackageInfosAsync(string packagesPath, string idDirectory);

        IAsyncEnumerable<PackageInfo> CollectPackageInfosAsync(string packagesPath);

    }
}
