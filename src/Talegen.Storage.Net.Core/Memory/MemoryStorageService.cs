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
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Talegen.Common.Core.Extensions;
    using Talegen.Storage.Net.Core.Properties;

    /// <summary>
    /// This class implements the storage interface for reading and writing to a virtual directory in memory.
    /// </summary>
    /// <remarks>This is useful for faking storage for unit tests within your application.</remarks>
    /// <seealso cref="Talegen.Storage.Net.Core.IStorageService" />
    public class MemoryStorageService : IStorageService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryStorageService" /> class.
        /// </summary>
        /// <param name="workspacePath">Contains the root path where the local storage folder and file structure begins.</param>
        /// <param name="uniqueWorkspace">Contains a value indicating whether the storage service shall use a unique workspace sub-folder.</param>
        public MemoryStorageService(string workspacePath, bool uniqueWorkspace = false)
            : this(new MemoryStorageContext(workspacePath, uniqueWorkspace))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryStorageService" /> class.
        /// </summary>
        /// <param name="storageContext">Contains an implementation of the storage context.</param>
        public MemoryStorageService(MemoryStorageContext storageContext)
        {
            // create a storage Id of 10 random characters
            this.StorageId = Guid.NewGuid().ToString();

            // initialize the storage settings
            this.Initialize(storageContext);
        }

        /// <summary>
        /// Gets the virtual disk accessor.
        /// </summary>
        public static ConcurrentDictionary<string, MemoryFolderModel> VirtualDisk { get; } = new ConcurrentDictionary<string, MemoryFolderModel>();

        /// <summary>
        /// Gets or sets the storage identity for this service instance.
        /// </summary>
        public string StorageId { get; private set; }

        /// <summary>
        /// Gets or sets the root folder path for this service instance.
        /// </summary>
        public string RootPath { get; set; }

        /// <summary>
        /// This method is used to initialize a storage service with the specified settings provided within the <see cref="StorageContext" /> object.
        /// </summary>
        /// <param name="storageContext">Contains the settings used to initialize the storage service.</param>
        public void Initialize(MemoryStorageContext storageContext)
        {
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

            // if the folder hasn't been specified
            VirtualDisk.TryAdd(this.RootPath, new MemoryFolderModel());
        }

        /// <summary>
        /// This method is used to copy a file.
        /// </summary>
        /// <param name="sourceFilePath">Contains a the path to the file that will be copied.</param>
        /// <param name="targetFilePath">Contains the path to the target where the file is to be copied.</param>
        /// <param name="overwrite">Contains a value indicating if the target should be overwritten if it already exists. Default is true.</param>
        /// <exception cref="ArgumentNullException">sourceFilePath or targetFilePath</exception>
        /// <exception cref="Talegen.Storage.Net.Core.StorageException"></exception>
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
            bool targetIsDirectory = string.IsNullOrWhiteSpace(Path.GetExtension(absoluteTargetPath));

            if (targetIsDirectory)
            {
                // ensure path suffix
                absoluteTargetPath = Path.EndsInDirectorySeparator(absoluteTargetPath) ? absoluteTargetPath : absoluteTargetPath + Path.DirectorySeparatorChar;
            }

            if (this.FileExists(absoluteSourcePath))
            {
                string directory = Path.GetDirectoryName(absoluteSourcePath);
                var sourceContents = VirtualDisk[directory].Files[absoluteSourcePath];
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

                VirtualDisk[targetFolder].Files.Add(absoluteTargetPath, sourceContents);
            }
            else
            {
                throw new StorageException(string.Format(Resources.StorageNotFoundErrorText, absoluteSourcePath));
            }
        }

        /// <summary>
        /// This method is used to create a temporary directory within the Inspire application path.
        /// </summary>
        /// <param name="subFolderName">Contains a sub-folder that will be included in the working directory path.</param>
        /// <param name="silentExists">Contains a value indicating whether the method is silently return successfully if the folder path already exists.</param>
        /// <returns>Returns the name of the directory that was created.</returns>
        /// <exception cref="ArgumentNullException">subFolderName</exception>
        /// <exception cref="Talegen.Storage.Net.Core.StorageException"></exception>
        public string CreateDirectory(string subFolderName, bool silentExists = false)
        {
            if (string.IsNullOrWhiteSpace(subFolderName))
            {
                throw new ArgumentNullException(nameof(subFolderName));
            }

            this.VerifyDirectoryNameRoot(subFolderName);

            string absolutePath = Path.IsPathRooted(subFolderName) ? subFolderName : Path.Combine(this.RootPath, subFolderName);

            // create the working directory
            if (!this.DirectoryExists(absolutePath))
            {
                VirtualDisk[this.RootPath].Children.Add(absolutePath);
                VirtualDisk.TryAdd(absolutePath, new MemoryFolderModel());
            }
            else
            {
                throw new StorageException(string.Format(Resources.StorageDirectoryExistsErrorText, absolutePath));
            }

            return absolutePath;
        }

        /// <summary>
        /// This method is used to delete a directory and all of its files.
        /// </summary>
        /// <param name="subFolderName">Contains the name of the directory that will be deleted.</param>
        /// <param name="recursive">Delete all contents within the folder.</param>
        /// <param name="silentNoExist">
        /// Contains a value indicating whether an exception is thrown if the target folder does not exist. Default is true; no exception is thrown if the
        /// folder does not exist.
        /// </param>
        /// <exception cref="ArgumentNullException">subFolderName</exception>
        /// <exception cref="Talegen.Storage.Net.Core.StorageException">thrown if the specified folder does not exist.</exception>
        public void DeleteDirectory(string subFolderName, bool recursive = true, bool silentNoExist = true)
        {
            if (string.IsNullOrWhiteSpace(subFolderName))
            {
                throw new ArgumentNullException(nameof(subFolderName));
            }

            this.VerifyDirectoryNameRoot(subFolderName);

            string absolutePath = Path.IsPathRooted(subFolderName) ? subFolderName : Path.Combine(this.RootPath, subFolderName);

            // delete the directory and its files
            if (this.DirectoryExists(absolutePath))
            {
                if (!recursive && VirtualDisk[this.RootPath].Children.Count > 1)
                {
                    throw new StorageException(string.Format(Resources.StorageCannotDeleteRecursiveErrorText, absolutePath));
                }
                else
                {
                    VirtualDisk[this.RootPath].Children.Remove(absolutePath);

                    // recursive delete
                    this.DeleteFolderWorker(absolutePath);
                }
            }
            else
            {
                throw new StorageException(string.Format(Resources.StorageDirectoryDoesNotExistErrorText, absolutePath));
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
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            this.DeleteFiles(new List<string>() { filePath }, deleteDirectory);
        }

        /// <summary>
        /// This method is used to delete a list of files.
        /// </summary>
        /// <param name="filePathNames">Contains a list of file names that will be deleted.</param>
        /// <param name="deleteDirectory">
        /// Contains a value indicating whether the directory or directories the files are within will be deleted. This will only occur if no other files remain
        /// in the directory after the list of files have been deleted.
        /// </param>
        /// <exception cref="ArgumentNullException">filePathNames</exception>
        /// <exception cref="Talegen.Storage.Net.Core.StorageException"></exception>
        public void DeleteFiles(List<string> filePathNames, bool deleteDirectory = false)
        {
            if (filePathNames == null)
            {
                throw new ArgumentNullException(nameof(filePathNames));
            }

            List<string> directoryNames = new List<string>();

            try
            {
                filePathNames.ForEach(filePath =>
                {
                    this.VerifyDirectoryNameRoot(filePath);

                    string absolutePath = Path.Combine(this.RootPath, filePath);
                    string folderPath = Path.GetDirectoryName(absolutePath);

                    if (VirtualDisk.ContainsKey(folderPath) && VirtualDisk[folderPath].Files.ContainsKey(absolutePath))
                    {
                        VirtualDisk[folderPath].Files.Remove(absolutePath);

                        if (deleteDirectory && !directoryNames.Contains(folderPath))
                        {
                            directoryNames.Add(folderPath);
                        }
                    }
                    else
                    {
                        throw new StorageException(string.Format(Resources.StorageNotFoundErrorText, filePath));
                    }
                });

                if (deleteDirectory)
                {
                    foreach (var folderPath in directoryNames.OrderByDescending(n => n.Length))
                    {
                        if (VirtualDisk.ContainsKey(folderPath))
                        {
                            if (!VirtualDisk[folderPath].Files.Any() && !VirtualDisk[folderPath].Children.Any())
                            {
                                VirtualDisk.TryRemove(folderPath, out _);
                            }
                            else
                            {
                                throw new StorageException(string.Format(Resources.StorageCannotDeleteRecursiveErrorText, folderPath));
                            }
                        }
                        else
                        {
                            throw new StorageException(string.Format(Resources.StorageNotFoundErrorText, folderPath));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new StorageException(string.Format(Resources.ExceptionErrorText, ex.Message), ex);
            }
        }

        /// <summary>
        /// Contains a value indicating whether the folder already exists.
        /// </summary>
        /// <param name="subFolderName">Contains the sub-folder name excluding the root folder path.</param>
        /// <returns>Returns a value indicating whether the directory exists.</returns>
        /// <exception cref="ArgumentNullException">subFolderName</exception>
        public bool DirectoryExists(string subFolderName)
        {
            if (string.IsNullOrWhiteSpace(subFolderName))
            {
                throw new ArgumentNullException(nameof(subFolderName));
            }

            this.VerifyDirectoryNameRoot(subFolderName);

            // if the folder name specified has an absolute path, otherwise, just a sub-folder name structure was specified, so prefix root path.
            string absolutePath = Path.IsPathRooted(subFolderName) ? subFolderName : Path.Combine(this.RootPath, subFolderName);

            return VirtualDisk.ContainsKey(absolutePath);
        }

        /// <summary>
        /// This method is used to delete all directories and files inside of a sub-folder within the Inspire application data directory.
        /// </summary>
        /// <param name="subFolderName">Contains the sub-folder within the Inspire application data directory.</param>
        /// <exception cref="ArgumentNullException">subFolderName</exception>
        /// <exception cref="Talegen.Storage.Net.Core.StorageException">thrown if the specified folder does not exist.</exception>
        public void EmptyDirectory(string subFolderName)
        {
            if (string.IsNullOrWhiteSpace(subFolderName))
            {
                throw new ArgumentNullException(nameof(subFolderName));
            }

            this.VerifyDirectoryNameRoot(subFolderName);

            string absolutePath = Path.IsPathRooted(subFolderName) ? subFolderName : Path.Combine(this.RootPath, subFolderName);

            // if the directory exists...
            if (this.DirectoryExists(absolutePath))
            {
                // clear the files...
                VirtualDisk[absolutePath].Files.Clear();

                // clear any sub-folders
                foreach (var childPath in VirtualDisk[absolutePath].Children)
                {
                    this.DeleteFolderWorker(childPath);
                }

                // clear children
                VirtualDisk[absolutePath].Children.Clear();
            }
            else
            {
                throw new StorageException(string.Format(Resources.StorageDirectoryDoesNotExistErrorText, absolutePath));
            }
        }

        /// <summary>
        /// Contains a value indicating whether the file exists.
        /// </summary>
        /// <param name="filePath">Contains the sub-folder path and file name excluding the root folder path to determine if exists.</param>
        /// <returns>Returns a value indicating whether the file exists.</returns>
        /// <exception cref="ArgumentNullException">filePath</exception>
        public bool FileExists(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            this.VerifyDirectoryNameRoot(filePath);

            // if the folder name specified has an absolute path, otherwise, just a sub-folder name structure was specified, so prefix root path.
            string absolutePath = Path.IsPathRooted(filePath) ? filePath : Path.Combine(this.RootPath, filePath);

            string folderPath = Path.GetDirectoryName(absolutePath);
            return VirtualDisk.ContainsKey(folderPath) && VirtualDisk[folderPath].Files.ContainsKey(absolutePath);
        }

        /// <summary>
        /// This method is used to create a hash of the file contents.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <returns>Returns a hash of the file contents.</returns>
        /// <exception cref="ArgumentNullException">filePath</exception>
        public string FileHash(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            byte[] content = this.ReadBinaryFile(filePath);
            return content?.ToHashString();
        }

        /// <summary>
        /// This method is used to get the files from a directory.
        /// </summary>
        /// <param name="subFolderName">Contains the sub-folder name to get the files.</param>
        /// <param name="searchPattern">Contains an optional file name search pattern. If not specified, *.* is used.</param>
        /// <param name="searchOption">Contains search options. If not specified, all sub-folders are searched for the file pattern.</param>
        /// <returns>Returns a list of files in the directory path.</returns>
        /// <exception cref="ArgumentNullException">subFolderName</exception>
        public List<string> FindFiles(string subFolderName, string searchPattern = "*.*", SearchOption searchOption = SearchOption.AllDirectories)
        {
            if (string.IsNullOrWhiteSpace(subFolderName))
            {
                throw new ArgumentNullException(nameof(subFolderName));
            }

            this.VerifyDirectoryNameRoot(subFolderName);

            // if the folder name specified has an absolute path, otherwise, just a sub-folder name structure was specified, so prefix root path.
            string absolutePath = Path.IsPathRooted(subFolderName) ? subFolderName : Path.Combine(this.RootPath, subFolderName);

            List<string> files = new List<string>();

            if (this.DirectoryExists(absolutePath))
            {
                Regex reSearchPattern = new Regex(Regex.Escape(searchPattern).Replace("\\*", ".*") + "$", RegexOptions.IgnoreCase);

                files = VirtualDisk[absolutePath].Files.Select(file => file.Key).Where(file => reSearchPattern.IsMatch(Path.GetExtension(file))).ToList();
            }
            else
            {
                throw new StorageException(string.Format(Resources.StorageDirectoryDoesNotExistErrorText, absolutePath));
            }

            return files;
        }

        /// <summary>
        /// This method is used to move a file.
        /// </summary>
        /// <param name="sourceFilePath">Contains a the path to the file that will be moved.</param>
        /// <param name="targetFilePath">Contains the path to the target where the file is to be moved.</param>
        /// <param name="overwrite">Contains a value indicating if the target should be overwritten if it already exists. Default is true.</param>
        /// <exception cref="ArgumentNullException">sourceFilePath or targetFilePath</exception>
        /// <exception cref="Talegen.Storage.Net.Core.StorageException"></exception>
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
                absoluteTargetPath = Path.EndsInDirectorySeparator(absoluteTargetPath) ? absoluteTargetPath : absoluteTargetPath + Path.DirectorySeparatorChar;
            }

            if (this.FileExists(absoluteSourcePath))
            {
                string directory = Path.GetDirectoryName(absoluteSourcePath);
                var sourceContents = VirtualDisk[directory].Files[absoluteSourcePath];
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

                VirtualDisk[targetFolder].Files.Add(absoluteTargetPath, sourceContents);
                this.DeleteFile(absoluteSourcePath);
            }
            else
            {
                throw new StorageException(string.Format(Resources.StorageNotFoundErrorText, absoluteSourcePath));
            }
        }

        /// <summary>
        /// This method is used to read all the bytes from a binary file.
        /// </summary>
        /// <param name="path">Contains the path to the file to load and return.</param>
        /// <returns>Returns a byte array containing the binary bytes of the target file.</returns>
        /// <exception cref="ArgumentNullException">path</exception>
        /// <exception cref="Talegen.Storage.Net.Core.StorageException"></exception>
        public byte[] ReadBinaryFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            this.VerifyDirectoryNameRoot(path);

            string absolutePath = Path.IsPathRooted(path) ? path : Path.Combine(this.RootPath, path);

            byte[] result;

            if (this.FileExists(absolutePath))
            {
                string folderPath = Path.GetDirectoryName(absolutePath);
                result = VirtualDisk[folderPath].Files[absolutePath];
            }
            else
            {
                throw new StorageException(string.Format(Resources.StorageNotFoundErrorText, absolutePath));
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

            outputStream.Write(this.ReadBinaryFile(path));
        }

        /// <summary>
        /// This method is used to read all the bytes from a text file.
        /// </summary>
        /// <param name="path">Contains the path to the file to load and return.</param>
        /// <param name="encoding">Contains the text encoding type. If none is specified, Encoding.Default is used.</param>
        /// <returns>Returns a string containing the content of the target file.</returns>
        public string ReadTextFile(string path, Encoding encoding = null)
        {
            Encoding encoder = encoding ?? Encoding.Default;
            return encoder.GetString(this.ReadBinaryFile(path) ?? Array.Empty<byte>());
        }

        /// <summary>
        /// This method is used to write content to the specified path.
        /// </summary>
        /// <param name="path">Contains the fully qualified path, including file name, to the location in which the binary content shall be written.</param>
        /// <param name="content">Contains a byte array of the content to be written.</param>
        /// <returns>Returns a value indicating whether the write was successful.</returns>
        /// <exception cref="ArgumentNullException">path</exception>
        /// <exception cref="Talegen.Storage.Net.Core.StorageException"></exception>
        public bool WriteBinaryFile(string path, byte[] content)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            this.VerifyDirectoryNameRoot(path);

            string absolutePath = Path.IsPathRooted(path) ? path : Path.Combine(this.RootPath, path);
            string folderPath = Path.GetDirectoryName(absolutePath);

            try
            {
                MemoryFolderModel folder = null;

                if (VirtualDisk.ContainsKey(folderPath))
                {
                    folder = VirtualDisk[folderPath];
                }
                else
                {
                    folder = new MemoryFolderModel();
                    VirtualDisk.TryAdd(folderPath, folder);
                    VirtualDisk[this.RootPath].Children.Add(folderPath);
                }

                // if the virtual file already exists...
                if (folder.Files.ContainsKey(absolutePath))
                {
                    // update contents
                    folder.Files[absolutePath] = content;
                }
                else
                {
                    // add contents if new virtual file.
                    folder.Files.Add(absolutePath, content);
                }
            }
            catch (Exception ex)
            {
                throw new StorageException(string.Format(Resources.ExceptionErrorText, ex.Message), ex);
            }

            return true;
        }

        /// <summary>
        /// This method is used to write content to the specified path.
        /// </summary>
        /// <param name="path">Contains the fully qualified path, including file name, to the location in which the binary content shall be written.</param>
        /// <param name="inputStream">Contains a stream of the content to be written.</param>
        /// <returns>Returns a value indicating whether the write was successful.</returns>
        /// <exception cref="ArgumentNullException">Thrown if argument is null.</exception>
        /// <exception cref="IOException">Thrown if stream cannot be read.</exception>
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

            return this.WriteBinaryFile(path, inputStream.ReadAllBytes());
        }

        /// <summary>
        /// This method is used to write content to the specified path.
        /// </summary>
        /// <param name="path">Contains the fully qualified path, including file name, to the location in which the text content shall be written.</param>
        /// <param name="content">Contains a string of the content to be written.</param>
        /// <param name="encoding">Contains the text file encoding. If none specified, Encoding.Default is used.</param>
        /// <returns>Returns a value indicating whether the write was successful.</returns>
        public bool WriteTextFile(string path, string content, Encoding encoding = null)
        {
            Encoding encoder = encoding ?? Encoding.Default;
            return this.WriteBinaryFile(path, encoder.GetBytes(content));
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
        /// A recursive worker function for deleting virtual folders.
        /// </summary>
        /// <param name="folderPath">Contains the virtual disk folder path to delete.</param>
        private void DeleteFolderWorker(string folderPath)
        {
            if (VirtualDisk.ContainsKey(folderPath))
            {
                foreach (var childPath in VirtualDisk[folderPath].Children)
                {
                    this.DeleteFolderWorker(childPath);
                }

                VirtualDisk.TryRemove(folderPath, out _);
            }
        }
    }
}