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
    using System;
    using Talegen.Storage.Net.Core;
    using Talegen.Storage.Net.Core.Disk;
    using Talegen.Storage.Net.Core.Memory;

    /// <summary>
    /// This class contains base properties and methods useful for unit tests.
    /// </summary>
    public abstract class UnitTestBase
    {
        /// <summary>
        /// Contains the storage context.
        /// </summary>
        private readonly IStorageContext storageContext;

        /// <summary>
        /// Contains the storage service.
        /// </summary>
        private readonly IStorageService storageService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitTestBase" /> class.
        /// </summary>
        /// <param name="storageService">The storage service.</param>
        public UnitTestBase(IStorageService storageService)
        {
            this.storageService = storageService ?? new MemoryStorageService("/", uniqueWorkspace: true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitTestBase" /> class.
        /// </summary>
        /// <param name="storageContext">The storage context.</param>
        public UnitTestBase(IStorageContext storageContext)
        {
            this.storageContext = storageContext ?? new MemoryStorageContext("/", uniqueWorkspace: true);

            switch (this.storageContext.StorageType)
            {
                case "memory":
                    this.storageService = new MemoryStorageService(this.storageContext as MemoryStorageContext);
                    break;

                case "disk":
                    this.storageService = new LocalStorageService(this.storageContext as LocalStorageContext);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets the storage context.
        /// </summary>
        /// <value>The storage context.</value>
        public IStorageContext StorageContext => this.storageContext;

        /// <summary>
        /// Gets the storage service.
        /// </summary>
        /// <value>The storage service.</value>
        public IStorageService StorageService => this.storageService;
    }
}