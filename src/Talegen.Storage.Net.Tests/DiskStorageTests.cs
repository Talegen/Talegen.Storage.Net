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

namespace Talegen.Storage.Net.Tests
{
    using System.IO;
    using Talegen.Storage.Net.Core.Disk;

    /// <summary>
    /// This class implements unit tests for local storage service.
    /// </summary>
    /// <seealso cref="Talegen.Storage.Net.Tests.StorageTestsBase" />
    public class DiskStorageTests : StorageTestsBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiskStorageTests" /> class.
        /// </summary>
        public DiskStorageTests()
            : base(new LocalStorageContext(Path.Combine(Path.GetTempPath(), nameof(DiskStorageTests)), true))
        {
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="DiskStorageTests" /> class.
        /// </summary>
        public override void CleanupTests()
        {
            // clean-up storage while destroying
            this.StorageService.DeleteDirectory(this.StorageService.RootPath);
        }
    }
}