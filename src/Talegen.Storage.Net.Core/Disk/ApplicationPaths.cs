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
    using System.IO;

    /// <summary>
    /// This class contains supporting properties and methods for managing configuration paths and files within a UAC secured Windows environment.
    /// </summary>
    public static class ApplicationPaths
    {
        /// <summary>
        /// Gets the full path to the currently running executable.
        /// </summary>
        public static string CurrentExecutablePath => System.Reflection.Assembly.GetEntryAssembly()?.Location;

        /// <summary>
        /// Gets the full path to the application base directory.
        /// </summary>
        public static string ApplicationPath => AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// Gets a value indicating whether the configuration folder exists.
        /// </summary>
        /// <param name="forAllUsers"><c>true</c> if the path is shared for all users on the system; otherwise <c>false</c>.</param>
        /// <param name="applicationTitle">Contains the name of the application to append to the path.</param>
        /// <returns>Returns a value indicating whether the configuration folder exists.</returns>
        public static bool ConfigurationFolderExists(bool forAllUsers = true, string applicationTitle = "")
        {
            return Directory.Exists(ConfigurationFolderPath(forAllUsers, applicationTitle));
        }

        /// <summary>
        /// Gets the file path to the application configuration file.
        /// </summary>
        /// <param name="configurationFileName">Contains the configuration file name.</param>
        /// <param name="forAllUsers"><c>true</c> if the path is shared for all users on the system; otherwise <c>false</c>.</param>
        /// <param name="applicationTitle">Contains the name of the application to append to the path.</param>
        /// <returns>Returns the full path to a configuration file.</returns>
        public static string ConfigurationFilePath(string configurationFileName, bool forAllUsers = true, string applicationTitle = "")
        {
            if (string.IsNullOrWhiteSpace(configurationFileName))
            {
                throw new ArgumentNullException(nameof(configurationFileName));
            }

            return Path.Combine(ConfigurationFolderPath(forAllUsers, applicationTitle), configurationFileName);
        }

        /// <summary>
        /// This method is used to get the folder path where configuration files are stored.
        /// </summary>
        /// <param name="forAllUsers"><c>true</c> if the path is shared for all users on the system; otherwise <c>false</c>.</param>
        /// <param name="applicationTitle">Contains the name of the application to append to the path.</param>
        /// <returns>Returns to a common application path where configurations can be stored.</returns>
        public static string ConfigurationFolderPath(bool forAllUsers = true, string applicationTitle = "")
        {
            return Path.Combine(Environment.GetFolderPath(forAllUsers ? Environment.SpecialFolder.CommonApplicationData : Environment.SpecialFolder.ApplicationData), applicationTitle);
        }

        /// <summary>
        /// This method is used to create the configuration path if it doesn't exist already.
        /// </summary>
        /// <param name="forAllUsers"><c>true</c> if the path is shared for all users on the system; otherwise <c>false</c>.</param>
        /// <param name="applicationTitle">Contains the name of the application to append to the path.</param>
        /// <returns>Returns a value indicating whether the configuration path was created successfully.</returns>
        public static bool CreateConfigurationPath(bool forAllUsers = true, string applicationTitle = "")
        {
            string configurationPath = ConfigurationFolderPath(forAllUsers, applicationTitle);
            bool result = Directory.Exists(configurationPath);

            if (!result)
            {
                Directory.CreateDirectory(configurationPath);
                result = Directory.Exists(configurationPath);
            }

            return result;
        }
    }
}