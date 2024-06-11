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
namespace Talegen.Storage.Net.Core.Disk
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Talegen.Common.Core.Extensions;
    using Talegen.Common.Core.Storage;
    using Talegen.Common.Core.Threading;
    using Talegen.Storage.Net.Core.Properties;

    /// <summary>
    /// This class implements the storage interface for implementing file IO with the local server disk storage.
    /// </summary>
    public class LocalStorageService : IStorageService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocalStorageService" /> class.
        /// </summary>
        /// <param name="workspacePath">Contains the root path where the local storage folder and file structure begins.</param>
        /// <param name="uniqueWorkspace">Contains a value indicating whether the storage service shall use a unique workspace sub-folder.</param>
        public LocalStorageService(string workspacePath, bool uniqueWorkspace = false)
            : this(new LocalStorageContext(workspacePath, uniqueWorkspace))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalStorageService" /> class.
        /// </summary>
        /// <param name="storageContext">Contains an implementation of the storage context.</param>
        public LocalStorageService(LocalStorageContext storageContext)
        {
            // create a storage Id of 10 random characters
            this.StorageId = CryptoExtensions.RandomAlphaString(10);

            // initialize the storage settings
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
        /// Gets the temporary path for the current operating system.
        /// </summary>
        public string TempPath => Path.Combine(this.RootPath, "Temp");

        /// <summary>
        /// This method is used to initialize a storage service with the specified settings provided within the <see cref="IStorageContext" /> object.
        /// </summary>
        /// <param name="storageContext">Contains the settings used to initialize the storage service.</param>
        public void Initialize(LocalStorageContext storageContext)
        {
            if (storageContext == null || storageContext.WorkspaceUri == null)
            {
                throw new ArgumentNullException(nameof(storageContext));
            }

            // if the path specified isn't a URL
            if (storageContext.WorkspaceUri.IsFile || storageContext.WorkspaceUri.IsUnc)
            {
                if (storageContext.UniqueWorkspace)
                {
                    // get the root worker local folder path now concatenate the storage session identifier as a sub-folder name to make this a unique path.
                    this.RootPath = Path.Combine(storageContext.RootWorkspaceLocalFolderPath, this.StorageId);
                }
                else
                {
                    // get the root worker local folder path now concatenate the storage session identifier as a sub-folder name to make this a unique path.
                    this.RootPath = storageContext.RootWorkspaceLocalFolderPath;
                }
            }
            else
            {
                // TODO: throw error that local folder must be specified.
                throw new ArgumentOutOfRangeException(nameof(storageContext.RootWorkspaceLocalFolderPath));
            }
        }

        /// <summary>
        /// This method is used to generate a temporary file name.
        /// </summary>
        /// <param name="extension">Contains an optional extension.</param>
        /// <param name="includePath">Contains a value indicating whether a temporary path should be generated and included in the result.</param>
        /// <returns>Returns the generated file name.</returns>
        public string GenerateTempFileName(string extension = null, bool includePath = false)
        {
            return includePath ? Path.Combine(this.GenerateTempDirectory(), Path.GetRandomFileName() + extension) : Path.GetRandomFileName() + extension;
        }

        /// <summary>
        /// This method is used to generate a temporary directory.
        /// </summary>
        /// <param name="useGuidNames">Contains a value indicating whether the random sub-folder should be a GUID.</param>
        /// <returns>Returns a temporary directory created inside the root path.</returns>
        public string GenerateTempDirectory(bool useGuidNames = false)
        {
            return Path.Combine(this.TempPath, useGuidNames ? Guid.NewGuid().ToString() : Path.GetRandomFileName());
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

            // if the folder name specified has an absolute path, otherwise, just a sub-folder name structure was specified, so prefix root path.
            string workingPath = Path.IsPathRooted(subFolderName) ? subFolderName : Path.Combine(this.RootPath, subFolderName);

            try
            {
                // so long as the directory does not exist already...
                if (!Directory.Exists(workingPath))
                {
                    // create the directory
                    Directory.CreateDirectory(workingPath);
                }
                else if (!silentExists)
                {
                    throw new StorageException(string.Format(Resources.StorageDirectoryExistsErrorText, workingPath));
                }
            }
            catch (UnauthorizedAccessException accessEx)
            {
                throw new StorageException(string.Format(Resources.StorageAccessDeniedErrorText, workingPath, accessEx.Message), accessEx);
            }
            catch (Exception ex)
            {
                throw new StorageException(string.Format(Resources.StorageDirectoryCommandErrorText, workingPath, ex.Message), ex);
            }

            return workingPath;
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

            string workingPath = Path.IsPathRooted(subFolderName) ? subFolderName : Path.Combine(this.RootPath, subFolderName);

            try
            {
                if (!recursive && Directory.EnumerateFileSystemEntries(workingPath).Any())
                {
                    throw new StorageException(string.Format(Resources.StorageCannotDeleteRecursiveErrorText, workingPath));
                }
                else if (Directory.Exists(workingPath))
                {
                    // delete the directory and any sub-folder/files
                    Directory.Delete(workingPath, recursive);
                }
                else if (!silentNoExist)
                {
                    throw new StorageException(string.Format(Resources.StorageDirectoryDoesNotExistErrorText, workingPath));
                }
            }
            catch (UnauthorizedAccessException accessEx)
            {
                throw new StorageException(string.Format(Resources.StorageAccessDeniedErrorText, workingPath, accessEx.Message), accessEx);
            }
            catch (Exception ex)
            {
                throw new StorageException(string.Format(Resources.StorageDirectoryCommandErrorText, workingPath, ex.Message), ex);
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

            string workingPath = Path.IsPathRooted(subFolderName) ? subFolderName : Path.Combine(this.RootPath, subFolderName);

            try
            {
                // delete the directory and its files
                if (Directory.Exists(workingPath))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(workingPath);
                    ParallelOptions parallelOptions = ThreadHelper.CreateOptions();

                    Parallel.ForEach(
                        directoryInfo.GetFiles(),
                        parallelOptions,
                        file =>
                        {
                            file.Delete();
                        });

                    Parallel.ForEach(
                        directoryInfo.GetDirectories(),
                        parallelOptions,
                        directory =>
                        {
                            directory.Delete(true);
                        });
                }
                else
                {
                    throw new StorageException(string.Format(Resources.StorageDirectoryDoesNotExistErrorText, workingPath));
                }
            }
            catch (UnauthorizedAccessException accessEx)
            {
                throw new StorageException(string.Format(Resources.StorageAccessDeniedErrorText, workingPath, accessEx.Message), accessEx);
            }
            catch (Exception ex)
            {
                throw new StorageException(string.Format(Resources.StorageDirectoryCommandErrorText, workingPath, ex.Message), ex);
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

            string workingPath = Path.IsPathRooted(path) ? path : Path.Combine(this.RootPath, path);

            byte[] result;

            try
            {
                // write all bytes to the extracted file path
                result = File.ReadAllBytes(workingPath);
            }
            catch (UnauthorizedAccessException accessEx)
            {
                throw new StorageException(string.Format(Resources.StorageAccessDeniedErrorText, workingPath, accessEx.Message), accessEx);
            }
            catch (Exception ex)
            {
                throw new StorageException(string.Format(Resources.ExceptionErrorText, ex.Message), ex);
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

            string workingPath = Path.IsPathRooted(path) ? path : Path.Combine(this.RootPath, path);

            try
            {
                using FileStream fileStream = File.OpenRead(workingPath);
                fileStream.CopyTo(outputStream);
            }
            catch (UnauthorizedAccessException accessEx)
            {
                throw new StorageException(string.Format(Resources.StorageAccessDeniedErrorText, workingPath, accessEx.Message), accessEx);
            }
            catch (Exception ex)
            {
                throw new StorageException(string.Format(Resources.ExceptionErrorText, ex.Message), ex);
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

            string workingPath = Path.IsPathRooted(path) ? path : Path.Combine(this.RootPath, path);
            string result;

            try
            {
                // write all bytes to the extracted file path
                result = File.ReadAllText(workingPath, encoding ?? System.Text.Encoding.Default);
            }
            catch (UnauthorizedAccessException accessEx)
            {
                throw new StorageException(string.Format(Resources.StorageAccessDeniedErrorText, workingPath, accessEx.Message), accessEx);
            }
            catch (Exception ex)
            {
                throw new StorageException(string.Format(Resources.ExceptionErrorText, ex.Message), ex);
            }

            return result;
        }

        /// <summary>
        /// This method is used to write content to the specified path.
        /// </summary>
        /// <param name="path">Contains the fully qualified path, including file name, to the location in which the binary content shall be written.</param>
        /// <param name="content">Contains a byte array of the content to be written.</param>
        /// <returns>Returns a value indicating whether the write was successful.</returns>
        public bool WriteBinaryFile(string path, byte[] content)
        {
            bool result = true;

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            this.VerifyDirectoryNameRoot(path);

            string workingPath = Path.IsPathRooted(path) ? path : Path.Combine(this.RootPath, path);
            string directoryPath = Path.GetDirectoryName(workingPath);

            try
            {
                directoryPath = this.CreateDirectory(directoryPath, true);

                // write all bytes to the extracted file path
                File.WriteAllBytes(workingPath, content);
            }
            catch (UnauthorizedAccessException accessEx)
            {
                throw new StorageException(string.Format(Resources.StorageAccessDeniedErrorText, workingPath, accessEx.Message), accessEx);
            }
            catch (Exception ex)
            {
                throw new StorageException(string.Format(Resources.ExceptionErrorText, ex.Message), ex);
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
            bool result = true;

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

            string workingPath = Path.IsPathRooted(path) ? path : Path.Combine(this.RootPath, path);
            string directoryPath = Path.GetDirectoryName(workingPath);

            try
            {
                directoryPath = this.CreateDirectory(directoryPath, true);
                using FileStream fileStream = new FileStream(workingPath, FileMode.OpenOrCreate);
                inputStream.CopyTo(fileStream);
            }
            catch (UnauthorizedAccessException accessEx)
            {
                throw new StorageException(string.Format(Resources.StorageAccessDeniedErrorText, workingPath, accessEx.Message), accessEx);
            }
            catch (Exception ex)
            {
                throw new StorageException(string.Format(Resources.ExceptionErrorText, ex.Message), ex);
            }

            return result;
        }

        /// <summary>
        /// This method is used to write content to the specified file stream.
        /// </summary>
        /// <param name="fileStream">Contains the file stream to which the binary content shall be written.</param>
        /// <param name="content">Contains a byte array of the content to be written.</param>
        /// <returns>Returns a value indicating whether the write was successful.</returns>
        public bool WriteBinaryFile(FileStream fileStream, byte[] content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            if (fileStream == null)
            {
                throw new ArgumentNullException(nameof(fileStream));
            }

            if (!fileStream.CanWrite)
            {
                throw new IOException(Resources.StreamWriteErrorText);
            }

            bool result = true;

            try
            {
                // write all bytes to the extracted file path
                fileStream.Write(content, 0, content.Length);
            }
            catch (Exception ex)
            {
                throw new StorageException(string.Format(Resources.ExceptionErrorText, ex.Message), ex);
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

            this.VerifyDirectoryNameRoot(path);

            string workingPath = Path.IsPathRooted(path) ? path : Path.Combine(this.RootPath, path);
            bool result;

            try
            {
                // write all bytes to the extracted file path
                File.WriteAllText(workingPath, content, encoding ?? System.Text.Encoding.Default);
                result = true;
            }
            catch (UnauthorizedAccessException accessEx)
            {
                throw new StorageException(string.Format(Resources.StorageAccessDeniedErrorText, workingPath, accessEx.Message), accessEx);
            }
            catch (Exception ex)
            {
                throw new StorageException(string.Format(Resources.ExceptionErrorText, ex.Message), ex);
            }

            return result;
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

            // if the folder name specified has an absolute path, otherwise, just a sub-folder name structure was specified, so prefix root path.
            string folderPath = Path.IsPathRooted(subFolderName) ? subFolderName : Path.Combine(this.RootPath, subFolderName);

            // does the directory path exist
            return Directory.Exists(folderPath);
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

            // if the folder name specified has an absolute path, otherwise, just a sub-folder name structure was specified, so prefix root path.
            string workingPath = Path.IsPathRooted(filePath) ? filePath : Path.Combine(this.RootPath, filePath);

            // does the file path exist
            return File.Exists(workingPath);
        }

        /// <summary>
        /// This method is used to delete a a list of files.
        /// </summary>
        /// <param name="filePathNames">Contains a list of file names that will be deleted.</param>
        /// <param name="deleteFolders">
        /// Contains a value indicating whether the directory or directories the files are within will be deleted. This will only occur if no other files remain
        /// in the directory after the list of files have been deleted.
        /// </param>
        public void DeleteFiles(List<string> filePathNames, bool deleteFolders = false)
        {
            if (filePathNames == null)
            {
                throw new ArgumentNullException(nameof(filePathNames));
            }

            List<string> localAbsoluteFilePaths = new List<string>();
            List<string> localAbsoluteDirectoryPaths = new List<string>();

            filePathNames.ForEach(name =>
            {
                this.VerifyDirectoryNameRoot(name);

                // if the folder name specified has an absolute path, otherwise, just a sub-folder name structure was specified, so prefix root path.
                string absolutePath = Path.IsPathRooted(name) ? name : Path.Combine(this.RootPath, name);

                // if is a directory
                if (Paths.EndsInDirectorySeparator(absolutePath) || this.IsDirectory(absolutePath))
                {
                    localAbsoluteDirectoryPaths.Add(absolutePath);
                }
                else
                {
                    localAbsoluteFilePaths.Add(absolutePath);

                    string folderPath = Path.GetDirectoryName(absolutePath);
                    if (deleteFolders && !localAbsoluteDirectoryPaths.Contains(folderPath))
                    {
                        localAbsoluteDirectoryPaths.Add(folderPath);
                    }
                }
            });

            try
            {
                ParallelOptions parallelOptions = ThreadHelper.CreateOptions();

                Parallel.ForEach(
                    localAbsoluteFilePaths,
                    parallelOptions,
                    filePath =>
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                    else
                    {
                        throw new StorageException(string.Format(Resources.StorageNotFoundErrorText, filePath));
                    }
                });

                if (deleteFolders)
                {
                    Parallel.ForEach(
                        localAbsoluteDirectoryPaths,
                        parallelOptions,
                        directoryPath =>
                    {
                        if (this.DirectoryExists(directoryPath))
                        {
                            if (!Directory.EnumerateFileSystemEntries(directoryPath).Any())
                            {
                                this.DeleteDirectory(directoryPath, true);
                            }
                            else
                            {
                                throw new StorageException(string.Format(Resources.StorageCannotDeleteRecursiveErrorText, directoryPath));
                            }
                        }
                        else
                        {
                            throw new StorageException(string.Format(Resources.StorageNotFoundErrorText, directoryPath));
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                throw new StorageException(string.Format(Resources.ExceptionErrorText, ex.Message), ex);
            }
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
            this.DeleteFiles(new List<string>() { filePath }, deleteDirectory);
        }

        /// <summary>
        /// This method is used to copy a file.
        /// </summary>
        /// <param name="sourceFilePath">Contains a the path to the file that will be copied.</param>
        /// <param name="targetFilePath">Contains the path to the target where the file is to be copied.</param>
        /// <param name="overwrite">Contains a value indicating if the target should be overwritten if it already exists. Default is true.</param>
        /// <exception cref="System.ArgumentNullException">sourceFilePath or targetFilePath</exception>
        /// <exception cref="StorageException"></exception>
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

            string absoluteSourcePath = Path.IsPathRooted(sourceFilePath) ? sourceFilePath : Path.Combine(this.RootPath, sourceFilePath);
            string absoluteTargetPath = Path.IsPathRooted(targetFilePath) ? targetFilePath : Path.Combine(this.RootPath, targetFilePath);
            bool targetIsDirectory = this.IsDirectory(absoluteTargetPath);

            if (targetIsDirectory)
            {
                // ensure path suffix
                absoluteTargetPath = Paths.EndsInDirectorySeparator(absoluteTargetPath) ? absoluteTargetPath : absoluteTargetPath + Path.DirectorySeparatorChar;
            }

            string targetFolder = Path.GetDirectoryName(absoluteTargetPath);

            if (!this.DirectoryExists(targetFolder))
            {
                targetFolder = this.CreateDirectory(targetFolder, true);
            }

            // is this a directory...
            if (targetIsDirectory && this.DirectoryExists(targetFolder))
            {
                // reuse the source file name for target path
                absoluteTargetPath = Path.Combine(absoluteTargetPath, Path.GetFileName(absoluteSourcePath));
            }

            try
            {
                File.Copy(absoluteSourcePath, absoluteTargetPath, overwrite);
            }
            catch (UnauthorizedAccessException accessEx)
            {
                throw new StorageException(string.Format(Resources.StorageAccessDeniedErrorText, absoluteSourcePath, accessEx.Message), accessEx);
            }
            catch (Exception ex)
            {
                throw new StorageException(string.Format(Resources.ExceptionErrorText, ex.Message), ex);
            }
        }

        /// <summary>
        /// This method is used to move a file.
        /// </summary>
        /// <param name="sourceFilePath">Contains a the path to the file that will be moved.</param>
        /// <param name="targetFilePath">Contains the path to the target where the file is to be moved.</param>
        /// <param name="overwrite">Contains a value indicating if the target should be overwritten if it already exists. Default is true.</param>
        /// <exception cref="System.ArgumentNullException">sourceFilePath or targetFilePath</exception>
        /// <exception cref="StorageException"></exception>
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

            this.VerifyDirectoryNameRoot(sourceFilePath);
            this.VerifyDirectoryNameRoot(targetFilePath);

            string absoluteSourcePath = Path.IsPathRooted(sourceFilePath) ? sourceFilePath : Path.Combine(this.RootPath, sourceFilePath);
            string absoluteTargetPath = Path.IsPathRooted(targetFilePath) ? targetFilePath : Path.Combine(this.RootPath, targetFilePath);

            bool targetIsDirectory = string.IsNullOrWhiteSpace(Path.GetExtension(absoluteTargetPath));

            if (targetIsDirectory)
            {
                // ensure path suffix
                absoluteTargetPath = Paths.EndsInDirectorySeparator(absoluteTargetPath) ? absoluteTargetPath : absoluteTargetPath + Path.DirectorySeparatorChar;
            }

            if (this.FileExists(absoluteSourcePath))
            {
                string targetFolder = Path.GetDirectoryName(absoluteTargetPath);

                if (!this.DirectoryExists(targetFolder))
                {
                    targetFolder = this.CreateDirectory(targetFolder, true);
                }

                // is this a directory...
                if (targetIsDirectory && this.DirectoryExists(targetFolder))
                {
                    // reuse the source file name for target path
                    absoluteTargetPath = Path.Combine(absoluteTargetPath, Path.GetFileName(absoluteSourcePath));
                }

                if (this.FileExists(absoluteTargetPath))
                {
                    if (overwrite)
                    {
                        this.DeleteFile(absoluteTargetPath);
                    }
                    else
                    {
                        throw new StorageException(string.Format(Resources.StorageTargetExistsErrorText, absoluteTargetPath));
                    }
                }

                try
                {
                    File.Move(absoluteSourcePath, absoluteTargetPath);
                }
                catch (UnauthorizedAccessException accessEx)
                {
                    throw new StorageException(string.Format(Resources.StorageAccessDeniedErrorText, absoluteSourcePath, accessEx.Message), accessEx);
                }
                catch (Exception ex)
                {
                    throw new StorageException(string.Format(Resources.ExceptionErrorText, ex.Message), ex);
                }
            }
            else
            {
                throw new StorageException(string.Format(Resources.StorageNotFoundErrorText, absoluteSourcePath));
            }
        }

        /// <summary>
        /// This method is used to create a hash of the file contents.
        /// </summary>
        /// <param name="filePath">Contains the path to the file to hash.</param>
        /// <returns>Returns a hash of the file contents.</returns>
        public string FileHash(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            this.VerifyDirectoryNameRoot(filePath);

            // if the folder name specified has an absolute path, otherwise, just a sub-folder name structure was specified, so prefix root path.
            string workingPath = Path.IsPathRooted(filePath) ? filePath : Path.Combine(this.RootPath, filePath);

            byte[] content = this.ReadBinaryFile(workingPath);
            string hash = content?.ToHashString();
            return hash;
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

            // if the folder name specified has an absolute path, otherwise, just a sub-folder name structure was specified, so prefix root path.
            string workingPath = Path.IsPathRooted(subFolderName) ? subFolderName : Path.Combine(this.RootPath, subFolderName);
            List<string> files;

            if (this.DirectoryExists(workingPath))
            {
                try
                {
                    files = Directory.GetFiles(workingPath, searchPattern, searchOption).ToList();
                }
                catch (UnauthorizedAccessException accessEx)
                {
                    throw new StorageException(string.Format(Resources.StorageAccessDeniedErrorText, workingPath, accessEx.Message), accessEx);
                }
                catch (Exception ex)
                {
                    throw new StorageException(string.Format(Resources.ExceptionErrorText, ex.Message), ex);
                }
            }
            else
            {
                throw new StorageException(string.Format(Resources.StorageDirectoryDoesNotExistErrorText, workingPath));
            }

            return files;
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
        /// Determines whether the specified path is a directory.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns><c>true</c> if the specified path is a directory; otherwise, <c>false</c>.</returns>
        private bool IsDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            bool result;

            try
            {
                if (File.Exists(path) || Directory.Exists(path))
                {
                    FileAttributes fileAttributes = File.GetAttributes(path);
                    result = fileAttributes.HasFlag(FileAttributes.Directory);
                }
                else
                {
                    result = string.IsNullOrWhiteSpace(Path.GetExtension(path));
                }
            }
            catch (Exception ex)
            {
                throw new StorageException(string.Format(Resources.ExceptionErrorText, ex.Message), ex);
            }

            return result;
        }
    }
}