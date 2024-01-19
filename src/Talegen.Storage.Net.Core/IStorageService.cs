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
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Defines a minimum implementation of a storage service.
    /// </summary>
    public interface IStorageService
    {
        /// <summary>
        /// Gets or sets the unique identity for the instance of the storage service.
        /// </summary>
        string StorageId { get; }

        /// <summary>
        /// Gets or sets the storage path for the instance of the storage service.
        /// </summary>
        string RootPath { get; }

        /// <summary>
        /// This method is used to create a temporary directory within the Inspire application path.
        /// </summary>
        /// <param name="subFolderName">Contains a sub-folder that will be included in the working directory path.</param>
        /// <param name="silentExists">Contains a value indicating whether the method is silently return successfully if the folder path already exists.</param>
        /// <returns>Returns the name of the directory that was created.</returns>
        string CreateDirectory(string subFolderName, bool silentExists = false);

        /// <summary>
        /// This method is used to delete a directory and all of its files.
        /// </summary>
        /// <param name="subFolderName">Contains the name of the directory that will be deleted.</param>
        /// <param name="recursive">Delete all contents within the folder.</param>
        /// <param name="silentNoExist">
        /// Contains a value indicating whether an exception is thrown if the target folder does not exist. Default is true; no exception is thrown if the
        /// folder does not exist.
        /// </param>
        void DeleteDirectory(string subFolderName, bool recursive = true, bool silentNoExist = true);

        /// <summary>
        /// This method is used to delete all directories and files inside of a sub-folder within the Inspire application data directory.
        /// </summary>
        /// <param name="subFolderName">Contains the sub-folder within the Inspire application data directory.</param>
        void EmptyDirectory(string subFolderName);

        /// <summary>
        /// This method is used to read all the bytes from a text file.
        /// </summary>
        /// <param name="path">Contains the path to the file to load and return.</param>
        /// <param name="encoding">Contains the text encoding type. If none is specified, Encoding.Default is used.</param>
        /// <returns>Returns a string containing the content of the target file.</returns>
        string ReadTextFile(string path, System.Text.Encoding encoding = null);

        /// <summary>
        /// This method is used to read all the bytes from a binary file.
        /// </summary>
        /// <param name="path">Contains the path to the file to load and return.</param>
        /// <returns>Returns a byte array containing the binary bytes of the target file.</returns>
        byte[] ReadBinaryFile(string path);

        /// <summary>
        /// This method is used to read all the bytes from a binary file to a provided stream.
        /// </summary>
        /// <param name="path">Contains the path to the file to load into the stream.</param>
        /// <param name="outputStream">The stream to write the file to.</param>
        void ReadBinaryFile(string path, Stream outputStream);

        /// <summary>
        /// This method is used to write content to the specified path.
        /// </summary>
        /// <param name="path">Contains the fully qualified path, including file name, to the location in which the binary content shall be written.</param>
        /// <param name="content">Contains a byte array of the content to be written.</param>
        /// <returns>Returns a value indicating whether the write was successful.</returns>
        bool WriteBinaryFile(string path, byte[] content);

        /// <summary>
        /// This method is used to write content to the specified path.
        /// </summary>
        /// <param name="path">Contains the fully qualified path, including file name, to the location in which the binary content shall be written.</param>
        /// <param name="inputStream">Contains a stream of the content to be written.</param>
        /// <returns>Returns a value indicating whether the write was successful.</returns>
        bool WriteBinaryFile(string path, Stream inputStream);

        /// <summary>
        /// This method is used to write content to the specified path.
        /// </summary>
        /// <param name="path">Contains the fully qualified path, including file name, to the location in which the text content shall be written.</param>
        /// <param name="content">Contains a string of the content to be written.</param>
        /// <param name="encoding">Contains the text file encoding. If none specified, Encoding.Default is used.</param>
        /// <returns>Returns a value indicating whether the write was successful.</returns>
        bool WriteTextFile(string path, string content, System.Text.Encoding encoding = null);

        /// <summary>
        /// This method is used to delete a list of files.
        /// </summary>
        /// <param name="filePathNames">Contains a list of file names that will be deleted.</param>
        /// <param name="deleteDirectory">
        /// Contains a value indicating whether the directory or directories the files are within will be deleted. This will only occur if no other files remain
        /// in the directory after the list of files have been deleted.
        /// </param>
        void DeleteFiles(List<string> filePathNames, bool deleteDirectory = false);

        /// <summary>
        /// This method is used to delete a file.
        /// </summary>
        /// <param name="filePath">Contains a the path to the file that will be deleted.</param>
        /// <param name="deleteDirectory">
        /// Contains a value indicating whether the directory the file is within will be deleted. This will only occur if no other files remain in the directory
        /// after the list of files have been deleted.
        /// </param>
        void DeleteFile(string filePath, bool deleteDirectory = false);

        /// <summary>
        /// This method is used to copy a file.
        /// </summary>
        /// <param name="sourceFilePath">Contains a the path to the file that will be copied.</param>
        /// <param name="targetFilePath">Contains the path to the target where the file is to be copied.</param>
        /// <param name="overwrite">Contains a value indicating if the target should be overwritten if it already exists. Default is true.</param>
        void CopyFile(string sourceFilePath, string targetFilePath, bool overwrite = true);

        /// <summary>
        /// This method is used to move a file.
        /// </summary>
        /// <param name="sourceFilePath">Contains a the path to the file that will be moved.</param>
        /// <param name="targetFilePath">Contains the path to the target where the file is to be moved.</param>
        /// <param name="overwrite">Contains a value indicating if the target should be overwritten if it already exists. Default is true.</param>
        void MoveFile(string sourceFilePath, string targetFilePath, bool overwrite = true);

        /// <summary>
        /// Contains a value indicating whether the folder already exists.
        /// </summary>
        /// <param name="subFolderName">Contains the sub-folder name excluding the root folder path.</param>
        /// <returns>Returns a value indicating whether the directory exists.</returns>
        bool DirectoryExists(string subFolderName);

        /// <summary>
        /// Contains a value indicating whether the file exists.
        /// </summary>
        /// <param name="filePath">Contains the sub-folder path and file name excluding the root folder path to determine if exists.</param>
        /// <returns>Returns a value indicating whether the file exists.</returns>
        bool FileExists(string filePath);

        /// <summary>
        /// This method is used to create a hash of the file contents.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <returns>Returns a hash of the file contents.</returns>
        string FileHash(string filePath);

        /// <summary>
        /// This method is used to get the files from a directory.
        /// </summary>
        /// <param name="subFolderName">Contains the sub-folder name to get the files.</param>
        /// <param name="searchPattern">Contains an optional file name search pattern. If not specified, *.* is used.</param>
        /// <param name="searchOption">Contains search options. If not specified, all sub-folders are searched for the file pattern.</param>
        /// <returns>Returns a list of files in the directory path.</returns>
        List<string> FindFiles(string subFolderName, string searchPattern = "*.*", SearchOption searchOption = SearchOption.AllDirectories);
    }
}