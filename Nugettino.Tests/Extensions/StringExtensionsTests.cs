using Nugettino.Extensions;

namespace Nugettino.Tests.Extensions
{
    public class StringExtensionsTests
    {
        [Fact]
        public void ToPackageInfo_Test()
        {

            var id = "id";
            var version = "version";

            var packageInfo = $"path/{id}/{version}/{id}.{version}.nupkg".ToPackageInfo();

            Assert.Equal(id, packageInfo.Id);
            Assert.Equal(version, packageInfo.Version);

        }
    }
}