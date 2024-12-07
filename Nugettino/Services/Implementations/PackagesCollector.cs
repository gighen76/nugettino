using Nugettino.Extensions;
using Nugettino.Models;
using System.Xml;

namespace Nugettino.Services.Implementations
{
    public class PackagesCollector: IPackagesCollector
    {

        private readonly IFileSystem _fileSystem;
        private readonly ILogger _logger;

        public PackagesCollector(IFileSystem fileSystem, ILogger<PackagesCollector> logger)
        {
            _fileSystem = fileSystem;
            _logger = logger;
        }


        public async Task<PackageInfo?> CollectPackageInfoAsync(string packagesPath, string idDirectory, string versionDirectory)
        {
            var versionDirectoryPath = Path.Combine(packagesPath, idDirectory, versionDirectory);

            _logger.LogInformation($"Collect from versionDirectoryPath [{versionDirectoryPath}]");

            var exactNupkgFileName = await _fileSystem.GetExistingExactFileNameAsync(versionDirectoryPath, $"{idDirectory}.{versionDirectory}.nupkg");
            if (exactNupkgFileName == null)
            {
                return null;
            }

            var id = exactNupkgFileName.Substring(idDirectory.Length);
            var version = exactNupkgFileName.Substring(idDirectory.Length + 1, versionDirectory.Length);
            var exactNuspecFileName = await _fileSystem.GetExistingExactFileNameAsync(versionDirectoryPath, $"{idDirectory}.nuspec");

            var packageInfo = new PackageInfo
            {
                Id = id,
                IdDirectory = idDirectory,
                Version = version,
                VersionDirectory = versionDirectory,
                ExactNupkgFileName = exactNupkgFileName,
                ExactNuspecFileName = exactNuspecFileName
            };

            var nuspecPath = packageInfo.GetNuspecPath(packagesPath);
            if (!string.IsNullOrEmpty(nuspecPath))
            {
                var xmlDocument = await _fileSystem.LoadXmlDocument(nuspecPath);
                await packageInfo.PopulateFromNuspecAsync(xmlDocument);
            }
            

            return packageInfo;


        }


        public async IAsyncEnumerable<PackageInfo> CollectPackageInfosAsync(string packagesPath, string idDirectory)
        {
            var idDirectoryPath = Path.Combine(packagesPath, idDirectory);

            _logger.LogInformation($"Collect from idDirectoryPath [{idDirectoryPath}]");

            await foreach (var versionDirectory in _fileSystem.EnumerateDirectoryNamesAsync(idDirectoryPath))
            {
                var packageVersion = await CollectPackageInfoAsync(packagesPath, idDirectory, versionDirectory);
                if (packageVersion == null)
                {
                    continue;
                }
                yield return packageVersion;
            }
        }


        public async IAsyncEnumerable<PackageInfo> CollectPackageInfosAsync(string packagesPath)
        {
            _logger.LogInformation($"Collect packages from [{packagesPath}]");

            await foreach (var idDirectory in _fileSystem.EnumerateDirectoryNamesAsync(packagesPath))
            {
                await foreach (var packageInfo in CollectPackageInfosAsync(packagesPath, idDirectory))
                {
                    yield return packageInfo;
                }
            }

        }


    }
}
