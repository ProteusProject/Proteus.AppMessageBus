using System.Linq;
using System.Threading.Tasks;
using PCLStorage;
using Proteus.Infrastructure.Messaging.Portable.Abstractions;

namespace Proteus.Infrastructure.Messaging.Portable
{
    public class FileSystemProvider : IFileSystemProvider
    {
        public async Task<IFolder> GetFolderAsync(IFolder parentFolder, string folderName)
        {
            IFolder folder = null;
            var folders = await parentFolder.GetFoldersAsync();

            foreach (var candidate in folders.Where(candidate => candidate.Name == folderName))
            {
                folder = candidate;
            }

            return folder;
        }

        public async Task<IFile> GetFileAsync(IFolder parentFolder, string fileName)
        {
            IFile file = null;

            var files = await parentFolder.GetFilesAsync();

            foreach (var candidate in files.Where(candidate => candidate.Name == fileName))
            {
                file = candidate;
            }

            return file;
        }

        public async Task<IFolder> CreateFolderAsync(IFolder parentFolder, string folderName, CreationCollisionOption creationCollisionOption)
        {
            return await parentFolder.CreateFolderAsync(folderName, creationCollisionOption);
        }

        public async Task DeleteFolderAsync(IFolder parentFolder, string folderName)
        {
            var folders = await parentFolder.GetFoldersAsync();

            foreach (var candidate in folders.Where(candidate => candidate.Name == folderName))
            {
                await candidate.DeleteAsync();
            }
        }

        public async Task<IFile> CreateFileAsync(IFolder parentFolder, string filename, CreationCollisionOption creationCollisionOption)
        {
            return await parentFolder.CreateFileAsync(filename, creationCollisionOption);
        }

        public async Task DeleteFileAsync(IFolder parentFolder, string filename)
        {
            var files = await parentFolder.GetFilesAsync();
            foreach (var file in files.Where(file => file.Name == filename))
            {
                await file.DeleteAsync();
            }
        }

        public async Task<string> ReadAllTextAsync(IFile file)
        {
            return await file.ReadAllTextAsync();
        }

        public async Task WriteAllTextAsync(IFile file, string text)
        {
            await file.WriteAllTextAsync(text);
        }
    }
}