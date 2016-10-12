#region License

/*
 * Copyright � 2013-2016 the original author or authors.
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

using System.Threading.Tasks;
using PCLStorage;

namespace Proteus.AppMessageBus.Portable.Abstractions
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