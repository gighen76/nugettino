using Castle.Core.Logging;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nugettino.Configurations;
using Nugettino.Services;
using Nugettino.Services.Implementations;

namespace Nugettino.Tests.Services
{
    public class PackagesManagerTests
    {

        private readonly IPackagesCollector _packagesCollector;

        private readonly NugettinoOptions _nugettinoOptions;

        private readonly PackagesManager _packagesManager;

        public PackagesManagerTests()
        {
            _packagesCollector = A.Fake<IPackagesCollector>();

            _nugettinoOptions = new NugettinoOptions
            {
                PackagesPath = "/packages"
            };

            var logger = A.Fake<ILogger<PackagesManager>>();

            _packagesManager = new PackagesManager(_packagesCollector, Options.Create(_nugettinoOptions), logger);

        }


        [Fact]
        public async Task Test()
        {

            await _packagesManager.RefreshAsync();

        }

    }
}
