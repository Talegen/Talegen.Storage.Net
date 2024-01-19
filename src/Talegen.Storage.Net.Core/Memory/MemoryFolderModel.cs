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
    using System.Collections.Generic;

    /// <summary>
    /// This class contains properties related to supporting a virtual disk storage system for unit testing.
    /// </summary>
    public class MemoryFolderModel
    {
        /// <summary>
        /// Gets or sets a dictionary of folders.
        /// </summary>
        public List<string> Children { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets a dictionary of files.
        /// </summary>
        public Dictionary<string, byte[]> Files { get; set; } = new Dictionary<string, byte[]>();
    }
}