using Nugettino.Models;

namespace Nugettino.Services
{
    public interface IPackagesCollector
    {

        public Task RefreshAsync();


        public List<PackageInfo>? PackageInfos { get; }

    }
}
