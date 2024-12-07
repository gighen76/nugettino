using FakeItEasy;
using Microsoft.Extensions.Logging;
using Nugettino.Services;
using Nugettino.Services.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nugettino.Tests.Services
{
    public class PackagesCollectorTests
    {

        private readonly IFileSystem _fileSystem;

        private readonly PackagesCollector _packagesCollector;

        public PackagesCollectorTests()
        {

            _fileSystem = A.Fake<IFileSystem>();
            var logger = A.Fake<ILogger<PackagesCollector>>();

            _packagesCollector = new PackagesCollector(_fileSystem, logger);
        }

        [Fact]
        public async Task CollectPackageVersionAsync_WithEmptyDir_Test()
        {

            var packagesPath = "/packages";
            var packageId = "package-id";
            var packageVersion = "0.0.0";
            var expectedVersionDirectoryPath = Path.Combine(packagesPath, packageId, packageVersion);

            A.CallTo(() => _fileSystem.GetExistingExactFileNameAsync(expectedVersionDirectoryPath, $"{packageId}.{packageVersion}.nupkg"))
                .Returns<string?>(null);
            A.CallTo(() => _fileSystem.GetExistingExactFileNameAsync(expectedVersionDirectoryPath, $"{packageId}.nuspec"))
                .Returns<string?>(null);

            var packageVersionInfo = await _packagesCollector.CollectPackageInfoAsync(packagesPath, packageId, packageVersion);
            Assert.Null(packageVersionInfo);

        }

        [Fact]
        public async Task CollectPackageVersionAsync_WithNupkgOnly_Test()
        {

            var packagesPath = "/packages";
            var packageId = "package-id";
            var packageVersion = "0.0.0";
            var expectedVersionDirectoryPath = Path.Combine(packagesPath, packageId, packageVersion);
            var exactNupkgFileName = "Package-Id.0.0.0.nupkg";

            A.CallTo(() => _fileSystem.GetExistingExactFileNameAsync(expectedVersionDirectoryPath, $"{packageId}.{packageVersion}.nupkg"))
                .Returns(exactNupkgFileName);
            A.CallTo(() => _fileSystem.GetExistingExactFileNameAsync(expectedVersionDirectoryPath, $"{packageId}.nuspec"))
                .Returns<string?>(null);

            var packageVersionInfo = await _packagesCollector.CollectPackageInfoAsync(packagesPath, packageId, packageVersion);
            Assert.NotNull(packageVersionInfo);
            Assert.Equal(exactNupkgFileName, packageVersionInfo.ExactNupkgFileName);
            Assert.Null(packageVersionInfo.ExactNuspecFileName);

        }


        [Fact]
        public async Task CollectPackageVersionAsync_WithNupkgAndNuspec_Test()
        {

            var packagesPath = "/packages";
            var idDirectory = "package-id";
            var versionDirectory = "0.0.0-alfa";
            var id = "Package-Id";
            var version = "0.0.0-Alfa";
            var expectedVersionDirectoryPath = Path.Combine(packagesPath, idDirectory, versionDirectory);
            var exactNupkgFileName = $"{id}.{version}.nupkg";
            var exactNuspecFileName = $"{id}.nuspec";

            A.CallTo(() => _fileSystem.GetExistingExactFileNameAsync(expectedVersionDirectoryPath, $"{idDirectory}.{versionDirectory}.nupkg"))
                .Returns(exactNupkgFileName);
            A.CallTo(() => _fileSystem.GetExistingExactFileNameAsync(expectedVersionDirectoryPath, $"{idDirectory}.nuspec"))
                .Returns(exactNuspecFileName);

            var packageVersionInfo = await _packagesCollector.CollectPackageInfoAsync(packagesPath, idDirectory, versionDirectory);
            Assert.NotNull(packageVersionInfo);
            Assert.Equal(version, packageVersionInfo.Version);
            Assert.Equal(exactNupkgFileName, packageVersionInfo.ExactNupkgFileName);
            Assert.Equal(exactNuspecFileName, packageVersionInfo.ExactNuspecFileName);

        }

    }
}
