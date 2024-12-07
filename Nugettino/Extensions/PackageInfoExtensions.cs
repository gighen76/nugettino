using Nugettino.Models;
using System.Xml;

namespace Nugettino.Extensions
{
    public static class PackageInfoExtensions
    {

        public static bool Match(this PackageInfo packageInfo, string lowerId)
        {
            return packageInfo.Id.Equals(lowerId, StringComparison.OrdinalIgnoreCase);
        }

        public static bool Match(this PackageInfo packageInfo, string lowerId, string lowerVersion)
        {
            return packageInfo.Match(lowerId) && packageInfo.Version.Equals(lowerVersion, StringComparison.OrdinalIgnoreCase);
        }

        public static string GetNupkgPath(this PackageInfo packageInfo, string packagesPath)
        {
            return Path.Combine(packagesPath, packageInfo.IdDirectory, packageInfo.VersionDirectory, packageInfo.ExactNupkgFileName);
        }

        public static string? GetNuspecPath(this PackageInfo packageInfo, string packagesPath)
        {
            if (packageInfo.ExactNuspecFileName == null)
            {
                return null;
            }
            return Path.Combine(packagesPath, packageInfo.IdDirectory, packageInfo.VersionDirectory, packageInfo.ExactNuspecFileName);
        }


        public static void PopulateFromNuspec(this PackageInfo packageInfo, XmlDocument xmlDocument)
        {

            var idElements = xmlDocument.GetElementsByTagName("id");
            packageInfo.Id = idElements.Count > 0 ? idElements[0]!.InnerText : packageInfo.Id;
            var versionElements = xmlDocument.GetElementsByTagName("version");
            packageInfo.Version = versionElements.Count > 0 ? versionElements[0]!.InnerText : packageInfo.Version;
            var authorElements = xmlDocument.GetElementsByTagName("authors");
            packageInfo.Authors = authorElements.Count > 0 ? authorElements[0]!.InnerText : packageInfo.Authors;
            var descriptionElements = xmlDocument.GetElementsByTagName("description");
            packageInfo.Description = authorElements.Count > 0 ? descriptionElements[0]!.InnerText : packageInfo.Description;

        }

    }
}
