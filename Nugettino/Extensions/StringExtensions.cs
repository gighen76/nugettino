using Nugettino.Models;

namespace Nugettino.Extensions
{
    public static class StringExtensions
    {

        public static PackageInfo ToPackageInfo(this string filePath)
        {
            // atteso .../{id_lower}/{version_lower}/file_name, devono esserci almeno 3 parti
            var pathParts = filePath.Split('\\', '/');
            if (pathParts.Length < 3)
            {
                throw new ArgumentException($"Invalid filePath [{filePath}]: it must be in the in the form .../id_lower/version_lower/file_name");
            }
            return new PackageInfo
            {
                Id = pathParts[^3],
                Version = pathParts[^2],
                FileName = Path.GetFileNameWithoutExtension(filePath),
            };
        }

    }
}
