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
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Talegen.Common.Core.Extensions;
    using Talegen.Storage.Net.Core;
    using Xunit;

    /// <summary>
    /// This class contains unit tests for exercising the storage service.
    /// </summary>
    /// <seealso cref="Talegen.Storage.Net.Tests.UnitTestBase" />
    public abstract class StorageTestsBase : UnitTestBase
    {
        /// <summary>
        /// Contains an existing folder name.
        /// </summary>
        private const string ExistingFolderName = "Existing";

        /// <summary>
        /// Defines a sub folder name.
        /// </summary>
        private const string SubFolderName = "SubFolder";

        /// <summary>
        /// Defines another sub folder name.
        /// </summary>
        private const string DeltaSubFolderName = "DeltaFolder";

        /// <summary>
        /// Defines a non-existing sub folder name.
        /// </summary>
        private const string NonExistingFolderName = "Nonexisting";

        /// <summary>
        /// The delete test folder name.
        /// </summary>
        private const string DeleteTestFolderName = "DeleteMe";

        /// <summary>
        /// Defines a test file name.
        /// </summary>
        private const string FileNameAlpha = "alpha.txt";

        /// <summary>
        /// Defines another test file name.
        /// </summary>
        private const string FileNameBeta = "beta.txt";

        /// <summary>
        /// Defines another test file name.
        /// </summary>
        private const string FileNameGamma = "gamma.txt";

        /// <summary>
        /// Defines another test file name.
        /// </summary>
        private const string FileNameDelta = "delta.txt";

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageTestsBase" /> class.
        /// </summary>
        /// <param name="storageService">The storage service.</param>
        public StorageTestsBase(IStorageService storageService)
            : base(storageService)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageTestsBase" /> class.
        /// </summary>
        /// <param name="storageContext">The storage context.</param>
        public StorageTestsBase(IStorageContext storageContext)
            : base(storageContext)
        {
        }

        /// <summary>
        /// Cleanups the tests.
        /// </summary>
        public virtual void CleanupTests()
        {
        }

        /// <summary>
        /// Verifies the root path tests.
        /// </summary>
        [Fact]
        public void VerifyPathTests()
        {
            // create an existing set of test content.
            this.StorageService.CreateDirectory(ExistingFolderName);

            Assert.Throws<StorageException>(() => this.StorageService.DeleteDirectory(Path.Combine("z:", ExistingFolderName), false));

            // clean-up
            this.CleanupTests();
        }

        /// <summary>
        /// Create directory tests.
        /// </summary>
        /// <param name="folderName">Name of the folder.</param>
        /// <param name="silentExists">if set to <c>true</c> [silent exists].</param>
        /// <param name="expectSuccess">if set to <c>true</c> [expect success].</param>
        [Theory]
        [InlineData(null, false, false)]
        [InlineData("new", true, true)]
        [InlineData(ExistingFolderName, false, false)]
        public void CreateDirectoryTests(string folderName, bool silentExists, bool expectSuccess)
        {
            // create an existing set of test content.
            this.StorageService.CreateDirectory(ExistingFolderName);

            if (expectSuccess)
            {
                // test the inputs
                string result = this.StorageService.CreateDirectory(folderName, silentExists);
                Assert.True(this.StorageService.DirectoryExists(folderName));
            }
            else if (!silentExists)
            {
                if (folderName == null)
                {
                    // ensure null check
                    Assert.Throws<ArgumentNullException>(() => this.StorageService.CreateDirectory(null));
                }
                else
                {
                    Assert.Throws<StorageException>(() => this.StorageService.CreateDirectory(folderName, silentExists));
                }
            }

            // clean-up
            this.CleanupTests();
        }

        /// <summary>
        /// Delete directory tests.
        /// </summary>
        /// <param name="folderName">Name of the folder.</param>
        /// <param name="recursive">if set to <c>true</c> [recursive].</param>
        /// <param name="silentNoExist">if set to <c>true</c> [silent no exist].</param>
        /// <param name="expectSuccess">if set to <c>true</c> [expect success].</param>
        [Theory]
        [InlineData(null, false, false, false)]
        [InlineData(NonExistingFolderName, true, false, false)]
        [InlineData(ExistingFolderName, true, false, true)]
        [InlineData(ExistingFolderName, false, true, false)]
        public void DeleteDirectoryTests(string folderName, bool recursive, bool silentNoExist, bool expectSuccess)
        {
            // create a folder and file.
            this.StorageService.CreateDirectory(ExistingFolderName, true);
            this.StorageService.WriteTextFile(Path.Combine(ExistingFolderName, FileNameAlpha), "test");

            // create a subfolder and file.
            string subFolder = Path.Combine(ExistingFolderName, SubFolderName);
            this.StorageService.CreateDirectory(subFolder, true);
            this.StorageService.WriteTextFile(Path.Combine(subFolder, FileNameBeta), "test");

            if (expectSuccess)
            {
                this.StorageService.DeleteDirectory(folderName, recursive, silentNoExist);
                Assert.False(this.StorageService.DirectoryExists(folderName));
            }
            else if (!silentNoExist)
            {
                if (folderName == null)
                {
                    Assert.Throws<ArgumentNullException>(() => this.StorageService.DeleteDirectory(folderName));
                }
                else
                {
                    Assert.Throws<StorageException>(() => this.StorageService.DeleteDirectory(folderName, recursive, silentNoExist));
                }
            }
            else
            {
                // expected to fail...
                Assert.Throws<StorageException>(() => this.StorageService.DeleteDirectory(folderName, recursive, silentNoExist));
            }

            // clean-up
            this.CleanupTests();
        }

        /// <summary>
        /// Delete file tests.
        /// </summary>
        /// <param name="targetFolderName">Name of the target folder.</param>
        /// <param name="targetFileName">Name of the target file.</param>
        /// <param name="deleteDirectory">if set to <c>true</c> [delete directory].</param>
        /// <param name="expectSuccess">if set to <c>true</c> [expect success].</param>
        [Theory]
        [InlineData(ExistingFolderName, null, false, false)]
        [InlineData(ExistingFolderName, FileNameAlpha, false, true)]
        [InlineData(NonExistingFolderName, FileNameAlpha, false, false)]
        [InlineData(ExistingFolderName + "\\" + SubFolderName, FileNameBeta, true, false)]
        [InlineData(ExistingFolderName + "\\" + DeltaSubFolderName, FileNameDelta, true, true)]
        public void DeleteFileTests(string targetFolderName, string targetFileName, bool deleteDirectory, bool expectSuccess)
        {
            // create a folder and file.
            this.StorageService.CreateDirectory(ExistingFolderName, true);
            this.StorageService.WriteTextFile(Path.Combine(ExistingFolderName, FileNameAlpha), "test");

            // create a subfolder and file.
            string subFolder = Path.Combine(ExistingFolderName, SubFolderName);
            this.StorageService.CreateDirectory(subFolder, true);
            this.StorageService.WriteTextFile(Path.Combine(subFolder, FileNameBeta), "test");
            this.StorageService.WriteTextFile(Path.Combine(subFolder, FileNameGamma), "test");

            // create a subfolder with a single file
            string deltaFolder = Path.Combine(ExistingFolderName, DeltaSubFolderName);
            this.StorageService.CreateDirectory(deltaFolder, true);
            this.StorageService.WriteTextFile(Path.Combine(deltaFolder, FileNameDelta), "test");

            if (expectSuccess)
            {
                string absolutePath = Path.Combine(targetFolderName, targetFileName);

                this.StorageService.DeleteFile(absolutePath, deleteDirectory);

                Assert.False(this.StorageService.FileExists(absolutePath));

                if (deleteDirectory)
                {
                    Assert.False(this.StorageService.DirectoryExists(targetFolderName));
                }
            }
            else
            {
                if (targetFileName == null)
                {
                    Assert.Throws<ArgumentNullException>(() => this.StorageService.DeleteFile(targetFileName, deleteDirectory));
                }
                else
                {
                    Assert.Throws<StorageException>(() => this.StorageService.DeleteFile(Path.Combine(targetFolderName, targetFileName), deleteDirectory));
                }
            }

            // clean-up
            this.CleanupTests();
        }

        /// <summary>
        /// Delete files tests.
        /// </summary>
        [Fact]
        public void DeleteFilesTests()
        {
            // create a folder and file.
            this.StorageService.CreateDirectory(ExistingFolderName, true);
            this.StorageService.WriteTextFile(Path.Combine(ExistingFolderName, FileNameAlpha), "test");

            // create a subfolder and file.
            string subFolder = Path.Combine(ExistingFolderName, SubFolderName);
            this.StorageService.CreateDirectory(subFolder, true);
            this.StorageService.WriteTextFile(Path.Combine(subFolder, FileNameBeta), "test");
            this.StorageService.WriteTextFile(Path.Combine(subFolder, FileNameGamma), "test");

            // create a subfolder and file.
            string deltaSubFolder = Path.Combine(ExistingFolderName, DeltaSubFolderName);
            this.StorageService.CreateDirectory(deltaSubFolder, true);
            this.StorageService.WriteTextFile(Path.Combine(deltaSubFolder, FileNameDelta), "test");
            this.StorageService.WriteTextFile(Path.Combine(deltaSubFolder, FileNameGamma), "test");

            // create a subfolder and file.
            string deletedSubFolder = Path.Combine(ExistingFolderName, DeleteTestFolderName);
            this.StorageService.CreateDirectory(deletedSubFolder, true);
            this.StorageService.WriteTextFile(Path.Combine(deletedSubFolder, FileNameDelta), "test");
            this.StorageService.WriteTextFile(Path.Combine(deletedSubFolder, FileNameGamma), "test");

            // test null input
            Assert.Throws<ArgumentNullException>(() => this.StorageService.DeleteFiles(null, false));

            string missingFilePath = Path.Combine(NonExistingFolderName, FileNameAlpha);

            // test missing file
            Assert.Throws<StorageException>(() => this.StorageService.DeleteFiles(new List<string> { missingFilePath }, false));

            string targetFilePath = Path.Combine(subFolder, FileNameBeta);

            // test directory delete with remaining contents
            Assert.Throws<StorageException>(() => this.StorageService.DeleteFiles(new List<string> { targetFilePath }, true));

            // test successful clear
            var files = new List<string> { Path.Combine(deltaSubFolder, FileNameDelta), Path.Combine(deltaSubFolder, FileNameGamma) };

            this.StorageService.DeleteFiles(files, false);
            Assert.True(this.StorageService.DirectoryExists(subFolder));

            var deletedFolderFiles = new List<string> { Path.Combine(deletedSubFolder, FileNameDelta), Path.Combine(deletedSubFolder, FileNameGamma) };
            this.StorageService.DeleteFiles(deletedFolderFiles, true);
            Assert.False(this.StorageService.DirectoryExists(deletedSubFolder));

            // clean-up
            this.CleanupTests();
        }

        /// <summary>
        /// Directory exists tests.
        /// </summary>
        /// <param name="targetFolderName">Name of the target folder.</param>
        /// <param name="expectedValue">if set to <c>true</c> [expected value].</param>
        /// <param name="expectSuccess">if set to <c>true</c> [expect success].</param>
        [Theory]
        [InlineData(null, false, false)]
        [InlineData(NonExistingFolderName, false, true)]
        [InlineData(ExistingFolderName, true, true)]
        public void DirectoryExistTests(string targetFolderName, bool expectedValue, bool expectSuccess)
        {
            // create a folder and file.
            this.StorageService.CreateDirectory(ExistingFolderName, true);

            if (expectSuccess)
            {
                Assert.Equal(expectedValue, this.StorageService.DirectoryExists(targetFolderName));
            }
            else
            {
                if (targetFolderName == null)
                {
                    Assert.Throws<ArgumentNullException>(() => this.StorageService.DirectoryExists(targetFolderName));
                }
            }

            // clean-up
            this.CleanupTests();
        }

        /// <summary>
        /// File exists tests.
        /// </summary>
        /// <param name="targetFilePath">The target file path.</param>
        /// <param name="expectedValue">if set to <c>true</c> [expected value].</param>
        /// <param name="expectSuccess">if set to <c>true</c> [expect success].</param>
        [Theory]
        [InlineData(null, false, false)]
        [InlineData(ExistingFolderName + "\\" + FileNameAlpha, true, true)]
        [InlineData(ExistingFolderName + "\\" + FileNameGamma, false, true)]
        public void FileExistTests(string targetFilePath, bool expectedValue, bool expectSuccess)
        {
            // create a folder and file.
            this.StorageService.CreateDirectory(ExistingFolderName, true);
            this.StorageService.WriteTextFile(Path.Combine(ExistingFolderName, FileNameAlpha), "test");

            if (expectSuccess)
            {
                Assert.Equal(expectedValue, this.StorageService.FileExists(targetFilePath));
            }
            else
            {
                if (targetFilePath == null)
                {
                    Assert.Throws<ArgumentNullException>(() => this.StorageService.FileExists(targetFilePath));
                }
            }

            // clean-up
            this.CleanupTests();
        }

        /// <summary>
        /// Empty directory tests.
        /// </summary>
        /// <param name="targetFolderName">Name of the target folder.</param>
        /// <param name="expectSuccess">if set to <c>true</c> [expect success].</param>
        [Theory]
        [InlineData(null, false)]
        [InlineData(ExistingFolderName, true)]
        [InlineData(NonExistingFolderName, false)]
        public void EmptyDirectoryTests(string targetFolderName, bool expectSuccess)
        {
            // create a folder and file.
            this.StorageService.CreateDirectory(ExistingFolderName, true);
            this.StorageService.WriteTextFile(Path.Combine(ExistingFolderName, FileNameAlpha), "test");
            string subFolder = Path.Combine(ExistingFolderName, SubFolderName);
            this.StorageService.CreateDirectory(subFolder, true);
            this.StorageService.WriteTextFile(Path.Combine(subFolder, FileNameBeta), "test");

            if (expectSuccess)
            {
                string filePath = Path.Combine(targetFolderName, FileNameAlpha);
                this.StorageService.EmptyDirectory(targetFolderName);
                Assert.True(this.StorageService.DirectoryExists(targetFolderName));
                Assert.False(this.StorageService.FileExists(filePath));
            }
            else if (targetFolderName == null)

            {
                Assert.Throws<ArgumentNullException>(() => this.StorageService.EmptyDirectory(targetFolderName));
            }
            else
            {
                Assert.Throws<StorageException>(() => this.StorageService.EmptyDirectory(targetFolderName));
            }

            // clean-up
            this.CleanupTests();
        }

        /// <summary>
        /// Hash file tests.
        /// </summary>
        /// <param name="targetFilePath">The target file path.</param>
        /// <param name="contents">The contents.</param>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="expectSuccess">if set to <c>true</c> [expect success].</param>
        [Theory]
        [InlineData(null, "", "", false)]
        [InlineData(ExistingFolderName + "\\" + FileNameAlpha, "test", "9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08", true)]
        [InlineData(NonExistingFolderName + "\\" + FileNameAlpha, "", "", false)]
        public void HashFileTests(string targetFilePath, string contents, string expectedValue, bool expectSuccess)
        {
            // create a folder and file.
            this.StorageService.CreateDirectory(ExistingFolderName, true);
            this.StorageService.WriteTextFile(Path.Combine(ExistingFolderName, FileNameAlpha), contents);

            if (expectSuccess)
            {
                Assert.Equal(expectedValue, this.StorageService.FileHash(targetFilePath));
            }
            else
            {
                if (targetFilePath == null)
                {
                    Assert.Throws<ArgumentNullException>(() => this.StorageService.FileHash(targetFilePath));
                }
                else
                {
                    Assert.Throws<StorageException>(() => this.StorageService.FileHash(targetFilePath));
                }
            }

            // clean-up
            this.CleanupTests();
        }

        /// <summary>
        /// Find files tests.
        /// </summary>
        /// <param name="searchPathName">Name of the search path.</param>
        /// <param name="searchPattern">The search pattern.</param>
        /// <param name="options">The options.</param>
        /// <param name="expectResults">if set to <c>true</c> [expect results].</param>
        /// <param name="expectSuccess">if set to <c>true</c> [expect success].</param>
        [Theory]
        [InlineData(null, "", SearchOption.AllDirectories, false, false)]
        [InlineData(ExistingFolderName, "*.*", SearchOption.AllDirectories, true, true)]
        [InlineData(NonExistingFolderName, "*.*", SearchOption.AllDirectories, false, false)]
        [InlineData(ExistingFolderName + "\\" + SubFolderName, "*.txt", SearchOption.AllDirectories, true, true)]
        [InlineData(ExistingFolderName + "\\" + SubFolderName, "*.nothing", SearchOption.AllDirectories, false, true)]
        public void FindFilesTests(string searchPathName, string searchPattern, SearchOption options, bool expectResults, bool expectSuccess)
        {
            // create a folder and file.
            this.StorageService.CreateDirectory(ExistingFolderName, true);
            this.StorageService.WriteTextFile(Path.Combine(ExistingFolderName, FileNameAlpha), "test");

            // create a subfolder and files.
            string subFolder = Path.Combine(ExistingFolderName, SubFolderName);
            this.StorageService.CreateDirectory(subFolder, true);
            this.StorageService.WriteTextFile(Path.Combine(subFolder, FileNameBeta), "test");
            this.StorageService.WriteTextFile(Path.Combine(subFolder, FileNameGamma), "test");

            // create an empty subfolder
            string deltaFolder = Path.Combine(ExistingFolderName, DeltaSubFolderName);
            this.StorageService.CreateDirectory(deltaFolder, true);

            if (expectSuccess)
            {
                var results = this.StorageService.FindFiles(searchPathName, searchPattern, options);
                Assert.NotNull(results);
                Assert.Equal(expectResults, results.Any());
            }
            else
            {
                if (searchPathName == null)
                {
                    Assert.Throws<ArgumentNullException>(() => this.StorageService.FindFiles(searchPathName, searchPattern, options));
                }
                else
                {
                    Assert.Throws<StorageException>(() => this.StorageService.FindFiles(searchPathName, searchPattern, options));
                }
            }

            // clean-up
            this.CleanupTests();
        }

        /// <summary>
        /// Move file tests.
        /// </summary>
        /// <param name="sourcePathName">Name of the source path.</param>
        /// <param name="targetFolderName">Name of the target folder.</param>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="overwrite">if set to <c>true</c> [overwrite].</param>
        /// <param name="expectSuccess">if set to <c>true</c> [expect success].</param>
        [Theory]
        [InlineData(null, ExistingFolderName, "", false, false)]
        [InlineData(ExistingFolderName, null, "", false, false)]
        [InlineData(ExistingFolderName + "\\" + FileNameAlpha, ExistingFolderName + "\\" + DeltaSubFolderName, ExistingFolderName + "\\" + DeltaSubFolderName + "\\" + FileNameAlpha, false, true)]
        [InlineData(ExistingFolderName + "\\" + FileNameAlpha, ExistingFolderName + "\\" + DeltaSubFolderName + "\\" + FileNameDelta, ExistingFolderName + "\\" + DeltaSubFolderName + "\\" + FileNameDelta, false, true)]
        [InlineData(ExistingFolderName + "\\" + FileNameAlpha, NonExistingFolderName + "\\" + DeltaSubFolderName, NonExistingFolderName + "\\" + DeltaSubFolderName + "\\" + FileNameAlpha, false, true)]
        [InlineData(ExistingFolderName + "\\" + FileNameAlpha, ExistingFolderName + "\\" + FileNameAlpha, "", false, false)]
        [InlineData(ExistingFolderName + "\\" + FileNameAlpha, ExistingFolderName + "\\" + SubFolderName + "\\" + FileNameBeta, "", false, false)]
        public void CopyFileTests(string sourcePathName, string targetFolderName, string expectedValue, bool overwrite, bool expectSuccess)
        {
            // create a folder and file.
            this.StorageService.CreateDirectory(ExistingFolderName, true);
            this.StorageService.WriteTextFile(Path.Combine(ExistingFolderName, FileNameAlpha), "test");

            // create a subfolder and files.
            string subFolder = Path.Combine(ExistingFolderName, SubFolderName);
            this.StorageService.CreateDirectory(subFolder, true);
            this.StorageService.WriteTextFile(Path.Combine(subFolder, FileNameBeta), "test");
            this.StorageService.WriteTextFile(Path.Combine(subFolder, FileNameGamma), "test");

            // create an empty subfolder
            string deltaFolder = Path.Combine(ExistingFolderName, DeltaSubFolderName);
            this.StorageService.CreateDirectory(deltaFolder, true);

            if (expectSuccess)
            {
                this.StorageService.CopyFile(sourcePathName, targetFolderName, overwrite);
                Assert.True(this.StorageService.FileExists(expectedValue));
                Assert.True(this.StorageService.FileExists(sourcePathName));
            }
            else
            {
                if (sourcePathName == null || targetFolderName == null)
                {
                    Assert.Throws<ArgumentNullException>(() => this.StorageService.CopyFile(sourcePathName, targetFolderName, overwrite));
                }
                else
                {
                    Assert.Throws<StorageException>(() => this.StorageService.CopyFile(sourcePathName, targetFolderName, overwrite));
                }
            }

            // clean-up
            this.CleanupTests();
        }

        /// <summary>
        /// Move file tests.
        /// </summary>
        /// <param name="sourcePathName">Name of the source path.</param>
        /// <param name="targetFolderName">Name of the target folder.</param>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="overwrite">if set to <c>true</c> [overwrite].</param>
        /// <param name="expectSuccess">if set to <c>true</c> [expect success].</param>
        [Theory]
        [InlineData(null, ExistingFolderName, "", false, false)]
        [InlineData(ExistingFolderName, null, "", false, false)]
        [InlineData(ExistingFolderName + "\\" + FileNameAlpha, ExistingFolderName + "\\" + DeltaSubFolderName, ExistingFolderName + "\\" + DeltaSubFolderName + "\\" + FileNameAlpha, false, true)]
        [InlineData(ExistingFolderName + "\\" + FileNameAlpha, ExistingFolderName + "\\" + DeltaSubFolderName + "\\" + FileNameDelta, ExistingFolderName + "\\" + DeltaSubFolderName + "\\" + FileNameDelta, false, true)]
        [InlineData(ExistingFolderName + "\\" + FileNameAlpha, NonExistingFolderName + "\\" + DeltaSubFolderName, NonExistingFolderName + "\\" + DeltaSubFolderName + "\\" + FileNameAlpha, false, true)]
        [InlineData(ExistingFolderName + "\\" + FileNameAlpha, ExistingFolderName + "\\" + FileNameAlpha, "", false, false)]
        [InlineData(ExistingFolderName + "\\" + FileNameAlpha, ExistingFolderName + "\\" + SubFolderName + "\\" + FileNameBeta, "", false, false)]
        public void MoveFileTests(string sourcePathName, string targetFolderName, string expectedValue, bool overwrite, bool expectSuccess)
        {
            // create a folder and file.
            this.StorageService.CreateDirectory(ExistingFolderName, true);
            this.StorageService.WriteTextFile(Path.Combine(ExistingFolderName, FileNameAlpha), "test");

            // create a subfolder and files.
            string subFolder = Path.Combine(ExistingFolderName, SubFolderName);
            this.StorageService.CreateDirectory(subFolder, true);
            this.StorageService.WriteTextFile(Path.Combine(subFolder, FileNameBeta), "test");
            this.StorageService.WriteTextFile(Path.Combine(subFolder, FileNameGamma), "test");

            // create an empty subfolder
            string deltaFolder = Path.Combine(ExistingFolderName, DeltaSubFolderName);
            this.StorageService.CreateDirectory(deltaFolder, true);

            if (expectSuccess)
            {
                this.StorageService.MoveFile(sourcePathName, targetFolderName, overwrite);
                Assert.True(this.StorageService.FileExists(expectedValue));
                Assert.False(this.StorageService.FileExists(sourcePathName));
            }
            else
            {
                if (sourcePathName == null || targetFolderName == null)
                {
                    Assert.Throws<ArgumentNullException>(() => this.StorageService.MoveFile(sourcePathName, targetFolderName, overwrite));
                }
                else
                {
                    Assert.Throws<StorageException>(() => this.StorageService.MoveFile(sourcePathName, targetFolderName, overwrite));
                }
            }

            // clean-up
            this.CleanupTests();
        }

        /// <summary>
        /// Read binary file tests.
        /// </summary>
        /// <param name="sourceFilePath">The source file path.</param>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="expectSuccess">if set to <c>true</c> [expect success].</param>
        [Theory]
        [InlineData(null, "", false)]
        [InlineData(ExistingFolderName + "\\" + FileNameAlpha, "test", true)]
        [InlineData(NonExistingFolderName + "\\" + FileNameAlpha, "", false)]
        public void ReadBinaryFileTests(string sourceFilePath, string expectedValue, bool expectSuccess)
        {
            // create a folder and file.
            this.StorageService.CreateDirectory(ExistingFolderName, true);
            this.StorageService.WriteTextFile(Path.Combine(ExistingFolderName, FileNameAlpha), "test");

            if (expectSuccess)
            {
                var result = this.StorageService.ReadBinaryFile(sourceFilePath);
                Assert.Equal(expectedValue, Encoding.Default.GetString(result));
            }
            else
            {
                if (sourceFilePath == null)
                {
                    Assert.Throws<ArgumentNullException>(() => this.StorageService.ReadBinaryFile(sourceFilePath));
                }
                else
                {
                    Assert.Throws<StorageException>(() => this.StorageService.ReadBinaryFile(sourceFilePath));
                }
            }

            // clean-up
            this.CleanupTests();
        }

        /// <summary>
        /// Read binary file stream tests.
        /// </summary>
        /// <param name="sourceFilePath">The source file path.</param>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="expectSuccess">if set to <c>true</c> [expect success].</param>
        [Theory]
        [InlineData(null, "", false)]
        [InlineData(ExistingFolderName + "\\" + FileNameAlpha, "test", true)]
        [InlineData(NonExistingFolderName + "\\" + FileNameAlpha, "", false)]
        public void ReadBinaryFileStreamTests(string sourceFilePath, string expectedValue, bool expectSuccess)
        {
            // create a folder and file.
            this.StorageService.CreateDirectory(ExistingFolderName, true);
            this.StorageService.WriteTextFile(Path.Combine(ExistingFolderName, FileNameAlpha), expectedValue);

            if (expectSuccess)
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    this.StorageService.ReadBinaryFile(sourceFilePath, stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    Assert.Equal(expectedValue, stream.ReadString(Encoding.Default));
                }
            }
            else
            {
                if (sourceFilePath == null)
                {
                    Assert.Throws<ArgumentNullException>(() => this.StorageService.ReadBinaryFile(sourceFilePath));
                }
                else
                {
                    Assert.Throws<StorageException>(() => this.StorageService.ReadBinaryFile(sourceFilePath));
                }
            }

            // clean-up
            this.CleanupTests();
        }

        /// <summary>
        /// Read text file tests.
        /// </summary>
        /// <param name="sourceFilePath">The source file path.</param>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="utf8">if set to <c>true</c> [UTF8].</param>
        /// <param name="expectSuccess">if set to <c>true</c> [expect success].</param>
        [Theory]
        [InlineData(null, "", false, false)]
        [InlineData(ExistingFolderName + "\\" + FileNameAlpha, "test", true, true)]
        [InlineData(ExistingFolderName + "\\" + FileNameAlpha, "test", false, true)]
        [InlineData(NonExistingFolderName + "\\" + FileNameAlpha, "", false, false)]
        public void ReadTextFileTests(string sourceFilePath, string expectedValue, bool utf8, bool expectSuccess)
        {
            var encoder = utf8 ? Encoding.UTF8 : Encoding.Default;

            // create a folder and file.
            this.StorageService.CreateDirectory(ExistingFolderName, true);
            this.StorageService.WriteTextFile(Path.Combine(ExistingFolderName, FileNameAlpha), "test", encoder);

            if (expectSuccess)
            {
                var result = this.StorageService.ReadTextFile(sourceFilePath, encoder);
                Assert.Equal(expectedValue, result);
            }
            else
            {
                if (sourceFilePath == null)
                {
                    Assert.Throws<ArgumentNullException>(() => this.StorageService.ReadTextFile(sourceFilePath, encoder));
                }
                else
                {
                    Assert.Throws<StorageException>(() => this.StorageService.ReadTextFile(sourceFilePath, encoder));
                }
            }

            // clean-up
            this.CleanupTests();
        }

        /// <summary>
        /// Write binary file tests.
        /// </summary>
        /// <param name="targetFilePath">The target file path.</param>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="expectSuccess">if set to <c>true</c> [expect success].</param>
        [Theory]
        [InlineData(null, "", false)]
        [InlineData(ExistingFolderName + "\\" + FileNameAlpha, "test", true)]
        [InlineData(NonExistingFolderName + "\\" + FileNameAlpha, "", true)]
        [InlineData(ExistingFolderName + "\\" + DeltaSubFolderName + "\\" + FileNameDelta, "test", true)]
        public void WriteBinaryFileTests(string targetFilePath, string expectedValue, bool expectSuccess)
        {
            // create a folder and file.
            this.StorageService.CreateDirectory(ExistingFolderName, true);
            this.StorageService.WriteTextFile(Path.Combine(ExistingFolderName, FileNameAlpha), "test");

            byte[] byteContent = Encoding.Default.GetBytes(expectedValue);

            if (expectSuccess)
            {
                Assert.True(this.StorageService.WriteBinaryFile(targetFilePath, byteContent));
                var contents = this.StorageService.ReadBinaryFile(targetFilePath);
                Assert.Equal(expectedValue, Encoding.Default.GetString(contents));
            }
            else
            {
                if (targetFilePath == null)
                {
                    Assert.Throws<ArgumentNullException>(() => this.StorageService.WriteBinaryFile(targetFilePath, byteContent));
                }
                else
                {
                    Assert.Throws<StorageException>(() => this.StorageService.WriteBinaryFile(targetFilePath, byteContent));
                }
            }

            // clean-up
            this.CleanupTests();
        }

        /// <summary>
        /// Write binary file stream tests.
        /// </summary>
        /// <param name="targetFilePath">The target file path.</param>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="expectSuccess">if set to <c>true</c> [expect success].</param>
        [Theory]
        [InlineData(null, "", false)]
        [InlineData(ExistingFolderName + "\\" + FileNameAlpha, "test", true)]
        [InlineData(NonExistingFolderName + "\\" + FileNameAlpha, "", true)]
        [InlineData(ExistingFolderName + "\\" + DeltaSubFolderName + "\\" + FileNameDelta, "test", true)]
        public void WriteBinaryFileStreamTests(string targetFilePath, string expectedValue, bool expectSuccess)
        {
            // create a folder and file.
            this.StorageService.CreateDirectory(ExistingFolderName, true);
            this.StorageService.WriteTextFile(Path.Combine(ExistingFolderName, FileNameAlpha), "test");

            byte[] byteContent = Encoding.Default.GetBytes(expectedValue);
            using MemoryStream stream = new MemoryStream(byteContent);

            if (expectSuccess)
            {
                Assert.True(this.StorageService.WriteBinaryFile(targetFilePath, stream));
                var contents = this.StorageService.ReadBinaryFile(targetFilePath);
                Assert.Equal(expectedValue, Encoding.Default.GetString(contents));
            }
            else
            {
                if (targetFilePath == null)
                {
                    Assert.Throws<ArgumentNullException>(() => this.StorageService.WriteBinaryFile(targetFilePath, stream));
                }
                else
                {
                    Assert.Throws<StorageException>(() => this.StorageService.WriteBinaryFile(targetFilePath, stream));
                }
            }

            // clean-up
            this.CleanupTests();
        }
    }
}