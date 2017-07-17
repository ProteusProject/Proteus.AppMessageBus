#region License

/*
 * Copyright © 2013-2016 the original author or authors.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Plugin.NetStandardStorage.Abstractions.Interfaces;
using Plugin.NetStandardStorage.Abstractions.Types;
using Proteus.AppMessageBus.Abstractions;

namespace Proteus.AppMessageBus
{
    public class FileSystemProvider : IFileSystemProviderAsync
    {
        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);

        public async Task<IFolder> GetFolderAsync(IFolder parentFolder, string folderName)
        {
            await Semaphore.WaitAsync();
            try
            {
                IFolder folder = null;
                var folders = parentFolder.GetFolders();

                foreach (var candidate in folders.Where(candidate => candidate.Name == folderName))
                {
                    folder = candidate;
                }

                return folder;
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public async Task<IFile> GetFileAsync(IFolder parentFolder, string fileName)
        {
            await Semaphore.WaitAsync();
            try
            {
                IFile file = null;

                var files = parentFolder.GetFiles();

                foreach (var candidate in files.Where(candidate => candidate.Name == fileName))
                {
                    file = candidate;
                }

                return file;
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public async Task<IFolder> CreateFolderAsync(IFolder parentFolder, string folderName, CreationCollisionOption creationCollisionOption)
        {
            await Semaphore.WaitAsync();
            try
            {
                return parentFolder.CreateFolder(folderName, creationCollisionOption);
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public async Task DeleteFolderAsync(IFolder parentFolder, string folderName)
        {
            await Semaphore.WaitAsync();
            try
            {
                var folders = parentFolder.GetFolders();

                foreach (var candidate in folders.Where(candidate => candidate.Name == folderName))
                {
                    candidate.Delete();
                }
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public async Task<IFile> CreateFileAsync(IFolder parentFolder, string filename, CreationCollisionOption creationCollisionOption)
        {
            await Semaphore.WaitAsync();
            try
            {
                return parentFolder.CreateFile(filename, creationCollisionOption);
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public async Task DeleteFileAsync(IFolder parentFolder, string filename)
        {
            await Semaphore.WaitAsync();
            try
            {
                var files = parentFolder.GetFiles();
                foreach (var file in files.Where(file => file.Name == filename))
                {
                    file.Delete();
                }
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public async Task<string> ReadAllTextAsync(IFile file)
        {
            await Semaphore.WaitAsync();
            try
            {
                using (var stream = file.Open(FileAccess.Read))
                {
                    return await new StreamReader(stream).ReadToEndAsync();
                }
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public async Task WriteAllTextAsync(IFile file, string text)
        {
            await Semaphore.WaitAsync();
            try
            {
                file.WriteAllText(text);
            }
            finally
            {
                Semaphore.Release();
            }
        }
    }
}