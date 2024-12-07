using System.Xml;

namespace Nugettino.Services
{
    public interface IFileSystem
    {

        IAsyncEnumerable<string> EnumerateDirectoryNamesAsync(string directoryPath);

        Task<string?> GetExistingExactFileNameAsync(string directoryPath, string fileName);

        Task<XmlDocument> LoadXmlDocument(string xmlPath);

    }
}
