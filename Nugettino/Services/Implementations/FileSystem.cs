
using System.Xml;

namespace Nugettino.Services.Implementations
{
    public class FileSystem : IFileSystem
    {

        public async IAsyncEnumerable<string> EnumerateDirectoryNamesAsync(string directoryPath)
        {
            var directoryInfo = new DirectoryInfo(directoryPath);
            if (!directoryInfo.Exists)
            {
                throw new ArgumentException($"Directory [{directoryPath}] does not exists", nameof(directoryPath));
            }

            foreach (var subDirectoryInfo in directoryInfo.EnumerateDirectories())
            {
                yield return subDirectoryInfo.Name;
            }
        }

        public async Task<string?> GetExistingExactFileNameAsync(string directoryPath, string fileName)
        {
            var directoryInfo = new DirectoryInfo(directoryPath);
            if (!directoryInfo.Exists)
            {
                throw new ArgumentException($"Directory [{directoryPath}] does not exists", nameof(directoryPath));
            }

            foreach (var fileInfo in directoryInfo.EnumerateFiles())
            {
                if (fileInfo.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                {
                    return fileInfo.Name;
                }
            }
            return null;
        }

        public async Task<XmlDocument> LoadXmlDocument(string xmlPath)
        {
            var document = new XmlDocument();
            document.Load(xmlPath);
            return document;
        }
    }
}
