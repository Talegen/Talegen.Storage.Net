﻿/*
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

    /// <summary>
    /// Implements a custom storage exception to throw on critical errors and handled exceptions.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class StorageException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StorageException" /> class.
        /// </summary>
        public StorageException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public StorageException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageException" /> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception, or a null reference ( <see langword="Nothing" /> in Visual Basic) if no inner exception is specified.
        /// </param>
        public StorageException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}