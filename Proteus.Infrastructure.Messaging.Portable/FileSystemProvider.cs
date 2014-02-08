using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PCLStorage;
using Proteus.Infrastructure.Messaging.Portable.Abstractions;

namespace Proteus.Infrastructure.Messaging.Portable
{
    public class FileSystemProvider : IFileSystemProviderAsync
    {
        private static readonly object MutexLock = new object();

        public async Task<IFolder> GetFolderAsync(IFolder parentFolder, string folderName)
        {
            Monitor.Enter(MutexLock);
            try
            {
                IFolder folder = null;
                var folders = await parentFolder.GetFoldersAsync();

                foreach (var candidate in folders.Where(candidate => candidate.Name == folderName))
                {
                    folder = candidate;
                }

                return folder;
            }
            finally
            {
                Monitor.Exit(MutexLock);
            }
        }

        public async Task<IFile> GetFileAsync(IFolder parentFolder, string fileName)
        {
            Monitor.Enter(MutexLock);
            try
            {
                IFile file = null;

                var files = await parentFolder.GetFilesAsync();

                foreach (var candidate in files.Where(candidate => candidate.Name == fileName))
                {
                    file = candidate;
                }

                return file;
            }
            finally
            {
                Monitor.Exit(MutexLock);
            }
        }

        public async Task<IFolder> CreateFolderAsync(IFolder parentFolder, string folderName, CreationCollisionOption creationCollisionOption)
        {
            Monitor.Enter(MutexLock);
            try
            {
                return await parentFolder.CreateFolderAsync(folderName, creationCollisionOption);
            }
            finally
            {
                Monitor.Exit(MutexLock);
            }
        }

        public async Task DeleteFolderAsync(IFolder parentFolder, string folderName)
        {
            Monitor.Enter(MutexLock);
            try
            {
                var folders = await parentFolder.GetFoldersAsync();

                foreach (var candidate in folders.Where(candidate => candidate.Name == folderName))
                {
                    await candidate.DeleteAsync();
                }
            }
            finally
            {
                Monitor.Exit(MutexLock);
            }
        }

        public async Task<IFile> CreateFileAsync(IFolder parentFolder, string filename, CreationCollisionOption creationCollisionOption)
        {
            Monitor.Enter(MutexLock);
            try
            {
                return await parentFolder.CreateFileAsync(filename, creationCollisionOption);
            }
            finally
            {
                Monitor.Exit(MutexLock);
            }
        }

        public async Task DeleteFileAsync(IFolder parentFolder, string filename)
        {
            Monitor.Enter(MutexLock);
            try
            {
                var files = await parentFolder.GetFilesAsync();
                foreach (var file in files.Where(file => file.Name == filename))
                {
                    await file.DeleteAsync();
                }
            }
            finally
            {
                Monitor.Exit(MutexLock);
            }
        }

        public async Task<string> ReadAllTextAsync(IFile file)
        {
            Monitor.Enter(MutexLock);
            try
            {
                return await file.ReadAllTextAsync();
            }
            finally
            {
                Monitor.Exit(MutexLock);
            }
        }

        public async Task WriteAllTextAsync(IFile file, string text)
        {
            Monitor.Enter(MutexLock);
            try
            {
                await file.WriteAllTextAsync(text);
            }
            finally
            {
                Monitor.Exit(MutexLock);
            }
        }
    }
}