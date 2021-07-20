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

namespace Talegen.Storage.Net.AzureBlobs
{
    using System;
    using Talegen.Storage.Net.Core;

    /// <summary>
    /// This class contains properties related to the storage service interfaces within the application.
    /// </summary>
    public class AzureBlobStorageContext : IStorageContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobStorageContext" /> class.
        /// </summary>
        /// <param name="blobEndpointUri">The BLOB endpoint URI.</param>
        /// <param name="accountName">Name of the account.</param>
        /// <param name="accountKey">The account key.</param>
        /// <param name="uniqueWorkspace">Contains a value indicating whether the storage service shall use a unique workspace sub-folder.</param>
        public AzureBlobStorageContext(Uri blobEndpointUri, string accountName, string accountKey, bool uniqueWorkspace = false)
        {
            /* Developer:
             * "UseDevelopmentStorage=true" converts to
             * "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;
             * AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;
             * BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;
             * QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;
             * TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;"
             */

            this.WorkspaceUri = blobEndpointUri;
            this.AccountName = accountName;
            this.AccountKey = accountKey;
            this.UniqueWorkspace = uniqueWorkspace;
            this.ConnectionString = $"AccountName={this.AccountName};AccountKey={this.AccountKey};BlobEndpoint={this.WorkspaceUri}";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobStorageContext" /> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="accountName">Name of the account.</param>
        /// <param name="uniqueWorkspace">Contains a value indicating whether the storage service shall use a unique workspace sub-folder.</param>
        public AzureBlobStorageContext(string connectionString, string accountName, bool uniqueWorkspace = false)
        {
            this.ConnectionString = connectionString;
            this.AccountName = accountName;
            this.UniqueWorkspace = uniqueWorkspace;
        }

        /// <summary>
        /// Gets the name of the account.
        /// </summary>
        /// <value>The name of the account.</value>
        public string AccountName { get; }

        /// <summary>
        /// Gets the account key.
        /// </summary>
        /// <value>The account key.</value>
        public string AccountKey { get; }

        /// <summary>
        /// Gets the connection string.
        /// </summary>
        /// <value>The connection string.</value>
        public string ConnectionString { get; private set; }

        /// <summary>
        /// Gets the storage type of the context.
        /// </summary>
        public string StorageType => "AzureBlob";

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
        public string RootWorkspaceLocalFolderPath => this.WorkspaceUri?.ToString();
    }
}