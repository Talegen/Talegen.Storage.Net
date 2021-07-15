namespace Talegen.Storage.Net.Tests
{
    using System;
    using Talegen.Storage.Net.Core;
    using Talegen.Storage.Net.Core.Disk;
    using Talegen.Storage.Net.Core.Memory;

    /// <summary>
    /// This class contains base properties and methods useful for unit tests.
    /// </summary>
    public class UnitTestBase
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
        public UnitTestBase()
            : this(new MemoryStorageContext("/", true))
        {
        }

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