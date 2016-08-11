using System.Threading.Tasks;
using PCLStorage;

namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public interface IFileSystemProviderAsync
    {
        Task<IFolder> GetFolderAsync(IFolder parentFolder, string folderName);
        Task<IFile> GetFileAsync(IFolder parentFolder, string fileName);
        Task<IFolder> CreateFolderAsync(IFolder parentFolder, string folderName, CreationCollisionOption creationCollisionOption);
        Task DeleteFolderAsync(IFolder parentFolder, string folderName);
        Task<IFile> CreateFileAsync(IFolder parentFolder, string filename, CreationCollisionOption creationCollisionOption);
        Task DeleteFileAsync(IFolder parentFolder, string filename);
        Task<string> ReadAllTextAsync(IFile file);
        Task WriteAllTextAsync(IFile file, string text);
    }
}