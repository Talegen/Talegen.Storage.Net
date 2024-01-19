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
    using System;
    using System.IO;
    using System.Text;
    using Newtonsoft.Json;
    using Talegen.Storage.Net.Core.Properties;

    /// <summary>
    /// This class contains storage extensions that extend the <see cref="IStorageService"/> interface.
    /// </summary>
    public static class StorageExtensions
    {
        /// <summary>
        /// This method is used to serialize an object and store it to a file, using JSON serializer.
        /// </summary>
        /// <typeparam name="T">Contains the object type to serialize.</typeparam>
        /// <param name="storageService">Contains the storage service this method extends.</param>
        /// <param name="filePath">Contains the path to the file where the serialized data will be stored.</param>
        /// <param name="obj">Contains the object to serialize.</param>
        /// <param name="encoding">Contains an optional encoding for the file out text.</param>
        /// <returns>Returns a value indicating whether the object was successfully serialized and the file exists.</returns>
        public static bool SerializeToFile<T>(this IStorageService storageService, string filePath, T obj, Encoding encoding = default)
        {
            if (storageService == null) 
            {
                throw new ArgumentNullException(nameof(storageService));
            }
            
            string contents = JsonConvert.SerializeObject(obj);
            return storageService.WriteTextFile(filePath, contents, encoding);
        }

        /// <summary>
        /// This method is used to deserialize an object from a file, using JSON serializer.
        /// </summary>
        /// <typeparam name="T">Contains the object type to deserialize.</typeparam>
        /// <param name="storageService">Contains the storage service this method extends.</param>
        /// <param name="filePath">Contains the path to the file containing the serialized data.</param>
        /// <param name="encoding">Contains an optional text encoding for the input file deserialized.</param>
        /// <returns>Returns the serialized object if deserialized successfully.</returns>
        public static T DeserializeFromFile<T>(this IStorageService storageService, string filePath, Encoding encoding = default)
        {
            if (storageService == null)
            {
                throw new ArgumentNullException(nameof(storageService));
            }

            string contents = storageService.ReadTextFile(filePath, encoding);
            T result;

            if (!string.IsNullOrWhiteSpace(contents))
            {
                try
                {
                    result = JsonConvert.DeserializeObject<T>(contents);
                }
                catch (JsonReaderException readEx)
                {
                    throw new StorageException(string.Format(Resources.FileContentsCannotBeDeserializedErrorText, filePath, nameof(T)), readEx);
                }
                catch (Exception ex)
                {
                    throw new StorageException(ex.Message, ex);
                }
            }
            else
            {
                throw new InvalidDataException(string.Format(Resources.FileContentsCannotBeDeserializedErrorText, filePath, nameof(T)));
            }

            return result;
        }
    }
}
