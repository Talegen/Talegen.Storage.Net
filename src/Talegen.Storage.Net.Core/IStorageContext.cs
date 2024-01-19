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
namespace Talegen.Storage.Net.Core
{
    using System;

    /// <summary>
    /// This class contains properties related to the storage service interfaces within the application.
    /// </summary>
    public interface IStorageContext
    {
        /// <summary>
        /// Gets the type of the storage.
        /// </summary>
        string StorageType { get; }

        /// <summary>
        /// Gets a value indicating whether the storage service shall use a unique workspace sub-folder.
        /// </summary>
        bool UniqueWorkspace { get; }

        /// <summary>
        /// Gets the root storage workspace URI path.
        /// </summary>
        Uri WorkspaceUri { get; }

        /// <summary>
        /// Gets the local folder path representation of the root workspace folder Uri.
        /// </summary>
        string RootWorkspaceLocalFolderPath { get; }
    }
}