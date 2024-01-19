/*
 *
 * (c) Copyright Talegen, LLC.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/
namespace Talegen.Storage.Net.Core.Memory
{
    using System;
    using System.IO;

    /// <summary>
    /// This class contains properties related to the storage service interfaces within the application.
    /// </summary>
    public class MemoryStorageContext : IStorageContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryStorageContext" /> class.
        /// </summary>
        /// <param name="workspacePath">Contains an the root workspace folder Uri path string.</param>
        /// <param name="uniqueWorkspace">Contains a value indicating whether the storage service shall use a unique workspace sub-folder.</param>
        public MemoryStorageContext(string workspacePath = "", bool uniqueWorkspace = true)
            : this(new Uri(Path.GetFullPath(workspacePath)), uniqueWorkspace)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryStorageContext" /> class.
        /// </summary>
        /// <param name="workspaceUri">Contains a root workspace folder Uri.</param>
        /// <param name="uniqueWorkspace">Contains a value indicating whether the storage service shall use a unique workspace sub-folder.</param>
        public MemoryStorageContext(Uri workspaceUri, bool uniqueWorkspace = true)
        {
            this.WorkspaceUri = workspaceUri;
            this.UniqueWorkspace = uniqueWorkspace;
        }

        /// <summary>
        /// Gets the storage type of the context.
        /// </summary>
        public string StorageType => "memory";

        /// <summary>
        /// Gets a value indicating whether the storage service shall use a unique workspace sub-folder.
        /// </summary>
        public bool UniqueWorkspace { get; }

        /// <summary>
        /// Gets the root storage workspace URI path.
        /// </summary>
        public Uri WorkspaceUri { get; }

        /// <summary>
        /// Gets the local folder path representation of the root workspace folder Uri.
        /// </summary>
        public string RootWorkspaceLocalFolderPath => this.WorkspaceUri?.LocalPath;
    }
}