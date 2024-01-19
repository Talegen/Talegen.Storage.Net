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
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Azure;
    using Azure.Storage.Files.Shares;
    using Azure.Storage.Files.Shares.Models;
    using Newtonsoft.Json;
    using Talegen.Common.Core.Extensions;
    using Talegen.Storage.Net.AzureBlobs.Properties;
    using Talegen.Storage.Net.Core;

    /// <summary>
    /// This class implements the storage interface for implementing file IO with the Azure Blob File Shares.
    /// </summary>
    public class AzureBlobStorageService : IStorageService
    {
        /// <summary>
        /// Contains an exception message to throw.
        /// </summary>
        private string exceptionMessage = string.Empty;

        /// <summary>
        /// The BLOB container client.
        /// </summary>
        private ShareClient azureShareClient;

        /// <summary>
        /// The azure storage context.
        /// </summary>
        private AzureBlobStorageContext azureStorageContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobStorageService" /> class.
        /// </summary>
        /// <param name="blobEndpointUri">The BLOB endpoint URI.</param>
        /// <param name="accountName">Name of the account.</param>
        /// <param name="accountKey">The account key.</param>
        /// <param name="uniqueWorkspace">Contains a value indicating whether the storage service shall use a unique workspace sub-folder.</param>
        public AzureBlobStorageService(Uri blobEndpointUri, string accountName, string accountKey, bool uniqueWorkspace = false)
            : this(new AzureBlobStorageContext(blobEndpointUri, accountName, accountKey, uniqueWorkspace))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobStorageService" /> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="accountName">Name of the account.</param>
        /// <param name="uniqueWorkspace">Contains a value indicating whether the storage service shall use a unique workspace sub-folder.</param>
        public AzureBlobStorageService(string connectionString, string accountName, bool uniqueWorkspace = false)
            : this(new AzureBlobStorageContext(connectionString, accountName, uniqueWorkspace))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobStorageService" /> class.
        /// </summary>
        /// <param name="storageContext">Contains an implementation of the storage context.</param>
        public AzureBlobStorageService(AzureBlobStorageContext storageContext)
        {
            // create a storage Id of 10 random characters
            this.StorageId = CryptoExtensions.RandomAlphaString(10);

            this.Initialize(storageContext);
        }

        /// <summary>
        /// Gets or sets the storage identity for this service instance.
        /// </summary>
        public string StorageId { get; private set; }

        /// <summary>
        /// Gets or sets the root folder path for this service instance.
        /// </summary>
        public string RootPath { get; set; }

        /// <summary>
        /// This method is used to initialize a storage service with the specified settings provided within the <see cref="IStorageContext" /> object.
        /// </summary>
        /// <param name="storageContext">Contains the settings used to initialize the storage service.</param>
        public void Initialize(AzureBlobStorageContext storageContext)
        {
            this.azureStorageContext = storageContext;

            if (this.azureStorageContext == null)
            {
                throw new ArgumentNullException(nameof(storageContext));
            }

            this.azureShareClient = new ShareClient(this.azureStorageContext.ConnectionString, this.azureStorageContext.AccountName);
            this.azureShareClient.CreateIfNotExists();
        }

        /// <summary>
        /// This method is used to create a temporary directory within the Inspire application path.
        /// </summary>
        /// <param name="subFolderName">Contains a sub-folder that will be included in the working directory path.</param>
        /// <param name="silentExists">Contains a value indicating whether the method is silently return successfully if the folder path already exists.</param>
        /// <returns>Returns the name of the directory that was created.</returns>
        /// <exception cref="StorageException">This exception is thrown if the directory is unable to be created.</exception>
        public string CreateDirectory(string subFolderName, bool silentExists = false)
        {
            if (string.IsNullOrWhiteSpace(subFolderName))
            {
                throw new ArgumentNullException(nameof(subFolderName));
            }

            this.VerifyDirectoryNameRoot(subFolderName);

            try
            {
                // build the directory
                ShareDirectoryClient shareDirectoryClient = this.azureShareClient.GetDirectoryClient(subFolderName);
                var response = shareDirectoryClient.CreateIfNotExists();
            }
            catch (RequestFailedException azureEx)
            {
                this.exceptionMessage = string.Format(Resources.RequestFailedErrorText, subFolderName, azureEx.Message);
                throw new StorageException(this.exceptionMessage, azureEx);
            }
            catch (Exception ex)
            {
                this.exceptionMessage = string.Format(Resources.ExceptionErrorText, ex.Message);
                throw new StorageException(this.exceptionMessage, ex);
            }

            return subFolderName;
        }

        /// <summary>
        /// This method is used to delete a sub-folder directory and all of its files.
        /// </summary>
        /// <param name="subFolderName">Contains the sub-folder name of the directory off the root working folder that will be deleted.</param>
        /// <param name="recursive">Delete all contents within the folder.</param>
        /// <param name="silentNoExist">
        /// Contains a value indicating whether an exception is thrown if the target folder does not exist. Default is true; no exception is thrown if the
        /// folder does not exist.
        /// </param>
        /// <exception cref="StorageException">This exception is thrown if the directory is unable to be deleted.</exception>
        public void DeleteDirectory(string subFolderName, bool recursive = true, bool silentNoExist = true)
        {
            if (string.IsNullOrWhiteSpace(subFolderName))
            {
                throw new ArgumentNullException(nameof(subFolderName));
            }

            this.VerifyDirectoryNameRoot(subFolderName);

            try
            {
                ShareDirectoryClient shareDirectoryClient = this.azureShareClient.GetDirectoryClient(subFolderName);

                if (shareDirectoryClient.Exists())
                {
                    this.DeleteDirectoryInternal(shareDirectoryClient, recursive);
                }
                else if (!silentNoExist)
                {
                    throw new StorageException(string.Format(Resources.StorageDirectoryDoesNotExistErrorText, subFolderName));
                }
            }
            catch (RequestFailedException azureEx)
            {
                this.exceptionMessage = string.Format(Resources.RequestFailedErrorText, subFolderName, azureEx.Message);
                throw new StorageException(this.exceptionMessage, azureEx);
            }
            catch (Exception ex)
            {
                this.exceptionMessage = string.Format(Resources.StorageDirectoryCommandErrorText, subFolderName, ex.Message);
                throw new StorageException(this.exceptionMessage, ex);
            }
        }

        /// <summary>
        /// This method is used to delete all directories and files inside of a sub-folder within the specified workspace root path, however the folder is left
        /// in place empty.
        /// </summary>
        /// <param name="subFolderName">Contains the sub-folder within the within the specified workspace root path to empty.</param>
        public void EmptyDirectory(string subFolderName)
        {
            if (string.IsNullOrWhiteSpace(subFolderName))
            {
                throw new ArgumentNullException(nameof(subFolderName));
            }

            this.VerifyDirectoryNameRoot(subFolderName);

            // build the directory
            ShareDirectoryClient shareDirectoryClient = this.azureShareClient.GetDirectoryClient(subFolderName);

            if (shareDirectoryClient.Exists())
            {
                foreach (ShareFileItem item in shareDirectoryClient.GetFilesAndDirectories())
                {
                    if (item.IsDirectory)
                    {
                        var subClient = shareDirectoryClient.GetSubdirectoryClient(item.Name);
                        this.DeleteDirectoryInternal(subClient, true);
                    }
                    else
                    {
                        shareDirectoryClient.DeleteFile(item.Name);
                    }
                }
            }
            else
            {
                throw new StorageException(string.Format(Resources.StorageDirectoryDoesNotExistErrorText, subFolderName));
            }
        }

        /// <summary>
        /// This method is used to read all the bytes from a binary file.
        /// </summary>
        /// <param name="path">Contains the path to the file to load and return.</param>
        /// <returns>Returns a byte array containing the binary bytes of the target file.</returns>
        public byte[] ReadBinaryFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            this.VerifyDirectoryNameRoot(path);

            byte[] result = Array.Empty<byte>();
            string directoryName = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);

            var dirClient = this.azureShareClient.GetDirectoryClient(directoryName);
            var fileClient = dirClient.GetFileClient(fileName);

            if (fileClient.Exists())
            {
                ShareFileDownloadInfo download = fileClient.Download();
                using MemoryStream stream = new MemoryStream();
                download.Content.CopyTo(stream);
                result = new byte[stream.Length + 1];
                result = stream.ToArray();
            }
            else
            {
                throw new StorageException(string.Format(Resources.StorageNotFoundErrorText, path));
            }

            return result;
        }

        /// <summary>
        /// This method is used to read all the bytes from a binary file to a provided stream.
        /// </summary>
        /// <param name="path">Contains the path to the file to load into the stream.</param>
        /// <param name="outputStream">The stream to write the file to.</param>
        public void ReadBinaryFile(string path, Stream outputStream)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (outputStream == null)
            {
                throw new ArgumentNullException(nameof(outputStream));
            }

            if (!outputStream.CanWrite)
            {
                throw new IOException(Resources.StreamWriteErrorText);
            }

            this.VerifyDirectoryNameRoot(path);

            string directoryName = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);

            var dirClient = this.azureShareClient.GetDirectoryClient(directoryName);
            var fileClient = dirClient.GetFileClient(fileName);

            if (fileClient.Exists())
            {
                ShareFileDownloadInfo download = fileClient.Download();
                download.Content.CopyTo(outputStream);
            }
            else
            {
                throw new StorageException(string.Format(Resources.StorageNotFoundErrorText, path));
            }
        }

        /// <summary>
        /// This method is used to read all the bytes from a text file.
        /// </summary>
        /// <param name="path">Contains the path to the file to load and return.</param>
        /// <param name="encoding">Contains the text encoding type. If none is specified, Encoding.Default is used.</param>
        /// <returns>Returns a string containing the content of the target file.</returns>
        public string ReadTextFile(string path, System.Text.Encoding encoding = null)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            this.VerifyDirectoryNameRoot(path);

            Encoding encoder = encoding ?? Encoding.Default;
            return encoder.GetString(this.ReadBinaryFile(path));
        }

        /// <summary>
        /// This method is used to write content to the specified path.
        /// </summary>
        /// <param name="path">Contains the fully qualified path, including file name, to the location in which the binary content shall be written.</param>
        /// <param name="content">Contains a byte array of the content to be written.</param>
        /// <returns>Returns a value indicating whether the write was successful.</returns>
        public bool WriteBinaryFile(string path, byte[] content)
        {
            bool result = false;

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            this.VerifyDirectoryNameRoot(path);

            using (MemoryStream stream = new MemoryStream(content))
            {
                result = this.WriteBinaryFile(path, stream);
            }

            return result;
        }

        /// <summary>
        /// This method is used to write content to the specified path.
        /// </summary>
        /// <param name="path">Contains the fully qualified path, including file name, to the location in which the binary content shall be written.</param>
        /// <param name="inputStream">Contains a stream of the content to be written.</param>
        /// <returns>Returns a value indicating whether the write was successful.</returns>
        public bool WriteBinaryFile(string path, Stream inputStream)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (inputStream == null)
            {
                throw new ArgumentNullException(nameof(inputStream));
            }

            if (!inputStream.CanRead)
            {
                throw new IOException(Resources.StreamReadErrorText);
            }

            this.VerifyDirectoryNameRoot(path);

            string directoryName = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);

            var dirClient = this.azureShareClient.GetDirectoryClient(directoryName);
            var fileClient = dirClient.GetFileClient(fileName);
            bool result;

            if (fileClient.Exists())
            {
                // Max. 4MB (4194304 Bytes in binary) allowed
                const int UploadLimit = 4194304;
                fileClient.Create(inputStream.Length);

                if (inputStream.Length <= UploadLimit)
                {
                    fileClient.UploadRange(new HttpRange(0, inputStream.Length), inputStream);
                }
                else
                {
                    int bytesRead;
                    long index = 0;
                    byte[] buffer = new byte[UploadLimit];

                    // Stream is larger than the limit so we need to upload in chunks
                    while ((bytesRead = inputStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        // Create a memory stream for the buffer to upload
                        using MemoryStream ms = new MemoryStream(buffer, 0, bytesRead);
                        fileClient.UploadRange(new HttpRange(index, ms.Length), ms);
                        index += ms.Length; // increment the index to the account for bytes already written
                    }
                }

                result = fileClient.Exists();
            }
            else
            {
                throw new StorageException(string.Format(Resources.StorageNotFoundErrorText, path));
            }

            return result;
        }

        /// <summary>
        /// This method is used to write content to the specified path.
        /// </summary>
        /// <param name="path">Contains the fully qualified path, including file name, to the location in which the text content shall be written.</param>
        /// <param name="content">Contains a string of the content to be written.</param>
        /// <param name="encoding">Contains the text file encoding. If none specified, Encoding.Default is used.</param>
        /// <returns>Returns a value indicating whether the write was successful.</returns>
        public bool WriteTextFile(string path, string content, System.Text.Encoding encoding = null)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            Encoding encoder = encoding ?? Encoding.Default;
            return this.WriteBinaryFile(path, encoder.GetBytes(content));
        }

        /// <summary>
        /// Contains a value indicating whether the folder already exists.
        /// </summary>
        /// <param name="subFolderName">Contains the sub-folder name excluding the root folder path.</param>
        /// <returns>Returns a value indicating whether the directory exists.</returns>
        public bool DirectoryExists(string subFolderName)
        {
            if (string.IsNullOrWhiteSpace(subFolderName))
            {
                throw new ArgumentNullException(nameof(subFolderName));
            }

            this.VerifyDirectoryNameRoot(subFolderName);

            ShareDirectoryClient shareDirectoryClient = this.azureShareClient.GetDirectoryClient(subFolderName);
            return shareDirectoryClient.Exists();
        }

        /// <summary>
        /// Contains a value indicating whether the file exists.
        /// </summary>
        /// <param name="filePath">Contains the sub-folder path and file name excluding the root folder path to determine if exists.</param>
        /// <returns>Returns a value indicating whether the file exists.</returns>
        public bool FileExists(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            this.VerifyDirectoryNameRoot(filePath);

            string directoryName = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileName(filePath);
            ShareDirectoryClient shareDirectoryClient = this.azureShareClient.GetDirectoryClient(directoryName);
            bool result;

            if (shareDirectoryClient.Exists())
            {
                var shareFileClient = shareDirectoryClient.GetFileClient(fileName);
                result = shareFileClient.Exists();
            }
            else
            {
                throw new StorageException(string.Format(Resources.StorageDirectoryDoesNotExistErrorText, directoryName));
            }

            return result;
        }

        /// <summary>
        /// Deletes the files.
        /// </summary>
        /// <param name="filePathNames">The file path names.</param>
        /// <param name="deleteFolders">if set to <c>true</c> [delete folders].</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void DeleteFiles(List<string> filePathNames, bool deleteFolders = false)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method is used to delete a file.
        /// </summary>
        /// <param name="filePath">Contains a the path to the file that will be deleted.</param>
        /// <param name="deleteDirectory">
        /// Contains a value indicating whether the directory the file is within will be deleted. This will only occur if no other files remain in the directory
        /// after the list of files have been deleted.
        /// </param>
        public void DeleteFile(string filePath, bool deleteDirectory = false)
        {
            string directoryName = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileName(filePath);

            ShareDirectoryClient shareDirectoryClient = this.azureShareClient.GetDirectoryClient(directoryName);

            if (shareDirectoryClient.Exists())
            {
                shareDirectoryClient.DeleteFile(fileName);

                if (deleteDirectory && !shareDirectoryClient.GetFilesAndDirectories().Any())
                {
                    shareDirectoryClient.Delete();
                }
            }
            else
            {
                throw new StorageException(string.Format(Resources.StorageNotFoundErrorText, filePath));
            }
        }

        /// <summary>
        /// This method is used to copy a file.
        /// </summary>
        /// <param name="sourceFilePath">Contains a the path to the file that will be copied.</param>
        /// <param name="targetFilePath">Contains the path to the target where the file is to be copied.</param>
        /// <param name="overwrite">Contains a value indicating if the target should be overwritten if it already exists. Default is true.</param>
        public void CopyFile(string sourceFilePath, string targetFilePath, bool overwrite = true)
        {
            if (string.IsNullOrWhiteSpace(sourceFilePath))
            {
                throw new ArgumentNullException(nameof(sourceFilePath));
            }

            if (string.IsNullOrWhiteSpace(targetFilePath))
            {
                throw new ArgumentNullException(nameof(targetFilePath));
            }

            this.VerifyDirectoryNameRoot(sourceFilePath);
            this.VerifyDirectoryNameRoot(targetFilePath);

            this.LateralCopyInternal(sourceFilePath, targetFilePath, overwrite);
        }

        /// <summary>
        /// This method is used to move a file.
        /// </summary>
        /// <param name="sourceFilePath">Contains a the path to the file that will be moved.</param>
        /// <param name="targetFilePath">Contains the path to the target where the file is to be moved.</param>
        /// <param name="overwrite">Contains a value indicating if the target should be overwritten if it already exists. Default is true.</param>
        public void MoveFile(string sourceFilePath, string targetFilePath, bool overwrite = true)
        {
            if (string.IsNullOrWhiteSpace(sourceFilePath))
            {
                throw new ArgumentNullException(nameof(sourceFilePath));
            }

            if (string.IsNullOrWhiteSpace(targetFilePath))
            {
                throw new ArgumentNullException(nameof(targetFilePath));
            }

            if (this.LateralCopyInternal(sourceFilePath, targetFilePath, overwrite))
            {
                this.DeleteFile(sourceFilePath);
            }
        }

        /// <summary>
        /// This method is used to create a hash of the file contents.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <returns>Returns a hash of the file contents.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public string FileHash(string filePath)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method is used to get the files from a directory.
        /// </summary>
        /// <param name="subFolderName">Contains the directory name to get the files.</param>
        /// <param name="searchPattern">Contains an optional file name search pattern. If not specified, *.* is used.</param>
        /// <param name="searchOption">Contains search options. If not specified, all sub-folders are searched for the file pattern.</param>
        /// <returns>Returns a list of files in the directory path.</returns>
        public List<string> FindFiles(string subFolderName, string searchPattern = "*.*", SearchOption searchOption = SearchOption.AllDirectories)
        {
            if (string.IsNullOrWhiteSpace(subFolderName))
            {
                throw new ArgumentNullException(nameof(subFolderName));
            }

            this.VerifyDirectoryNameRoot(subFolderName);

            ShareDirectoryClient shareDirectoryClient = this.azureShareClient.GetDirectoryClient(subFolderName);
            return this.FindFilesInternal(shareDirectoryClient, searchPattern, searchOption);
        }

        /// <summary>
        /// Verifies the directory name root path.
        /// </summary>
        /// <param name="subFolderName">Name of the sub folder to verify.</param>
        /// <exception cref="StorageException">
        /// An exception is thrown if the root folder specified in <paramref name="subFolderName" /> is not rooted with workspace root folder path.
        /// </exception>
        private void VerifyDirectoryNameRoot(string subFolderName)
        {
            // if the folder name specified has an absolute path...
            if (Path.IsPathRooted(subFolderName))
            {
                // a root path was defined in subFolderName, ensure it is under this.RootPath
                if (!Path.GetFullPath(subFolderName).StartsWith(Path.GetFullPath(this.RootPath)))
                {
                    throw new StorageException(string.Format(Resources.StoragePathSpecifiedNotInRootErrorText, subFolderName, this.RootPath));
                }
            }
        }

        /// <summary>
        /// Laterals the copy internal.
        /// </summary>
        /// <param name="sourceFilePath">The source file path.</param>
        /// <param name="destFilePath">The dest file path.</param>
        /// <param name="overwrite">Contains a value indicating if the target should be overwritten if it already exists.</param>
        /// <returns>Returns a value indicating copy success.</returns>
        private bool LateralCopyInternal(string sourceFilePath, string destFilePath, bool overwrite)
        {
            string sourceDirectoryPath = Path.GetDirectoryName(sourceFilePath);
            string sourceFileName = Path.GetFileName(sourceFilePath);
            string destDirectoryPath = Path.GetDirectoryName(destFilePath);
            string destFileName = Path.GetFileName(destFilePath);

            bool result = false;

            var sourceDirClient = this.azureShareClient.GetDirectoryClient(sourceDirectoryPath);
            var sourceFileClient = sourceDirClient.GetFileClient(sourceFileName);

            // Ensure that the source file exists
            if (sourceFileClient.Exists())
            {
                var destDirClient = this.azureShareClient.GetDirectoryClient(destDirectoryPath);
                destDirClient.CreateIfNotExists();

                var destFileClient = destDirClient.GetFileClient(destFileName);

                if (overwrite)
                {
                    destFileClient.DeleteIfExists();
                }

                // Start the copy operation
                destFileClient.StartCopy(sourceFileClient.Uri);

                result = destFileClient.Exists();
            }

            return result;
        }

        /// <summary>
        /// Finds the files internal.
        /// </summary>
        /// <param name="directoryClient">The directory client.</param>
        /// <param name="searchPattern">The search pattern.</param>
        /// <param name="searchOption">The search option.</param>
        /// <returns>Returns a list of file paths found internally in recursive call.</returns>
        private List<string> FindFilesInternal(ShareDirectoryClient directoryClient, string searchPattern = "", SearchOption searchOption = SearchOption.AllDirectories)
        {
            List<string> results = new List<string>();

            foreach (ShareFileItem item in directoryClient.GetFilesAndDirectories(searchPattern))
            {
                if (item.IsDirectory && searchOption == SearchOption.AllDirectories)
                {
                    ShareDirectoryClient subDir = directoryClient.GetSubdirectoryClient(item.Name);
                    results.AddRange(this.FindFilesInternal(subDir, searchPattern, searchOption));
                }
                else
                {
                    results.Add(item.Name);
                }
            }

            return results;
        }

        /// <summary>
        /// Deletes the directory and contents.
        /// </summary>
        /// <param name="shareDirectoryClient">The share directory client.</param>
        /// <param name="recursive">if set to <c>true</c> [recursive].</param>
        private void DeleteDirectoryInternal(ShareDirectoryClient shareDirectoryClient, bool recursive = true)
        {
            foreach (ShareFileItem item in shareDirectoryClient.GetFilesAndDirectories())
            {
                if (item.IsDirectory)
                {
                    var subClient = shareDirectoryClient.GetSubdirectoryClient(item.Name);

                    if (recursive)
                    {
                        this.DeleteDirectoryInternal(subClient, recursive);
                    }
                    else
                    {
                        subClient.Delete();
                    }
                }
                else
                {
                    shareDirectoryClient.DeleteFile(item.Name);
                }
            }

            shareDirectoryClient.Delete();
        }
    }
}