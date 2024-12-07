namespace Nugettino.Models
{
    public class PackageInfo
    {

        public string IdDirectory { get; set; } = null!;

        public string VersionDirectory { get; set; } = null!;

        public string ExactNupkgFileName { get; set; } = null!;

        public string? ExactNuspecFileName { get; set; }

        public string Id { get; set; } = null!;

        public string Version { get; set; } = null!;

        public string? Authors { get; set; }

        public string? Description { get; set; }

    }
}
