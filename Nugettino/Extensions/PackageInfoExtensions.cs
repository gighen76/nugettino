using Nugettino.Models;

namespace Nugettino.Extensions
{
    public static class PackageInfoExtensions
    {

        public static string ToNupkgPath(this PackageInfo packageInfo, string packagesPath)
        {
            return Path.Combine(packagesPath, packageInfo.Id, packageInfo.Version, $"{packageInfo.Id}.{packageInfo.Version}.nupkg");
        }

        public static string ToNuspecPath(this PackageInfo packageInfo, string packagesPath)
        {
            return Path.Combine(packagesPath, packageInfo.Id, packageInfo.Version, $"{packageInfo.Id}.nuspec");
        }

    }
}
